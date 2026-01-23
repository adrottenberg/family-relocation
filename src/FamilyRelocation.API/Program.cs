using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using FamilyRelocation.API.Middleware;
using FamilyRelocation.API.Services;
using FamilyRelocation.Application;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "FamilyRelocation API",
        Version = "v1",
        Description = "API for managing Orthodox Jewish family relocation to Union County, NJ"
    });

    // Add JWT Bearer security definition (Swashbuckle v10+ pattern)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });

    // Include XML comments from API project
    var apiXmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var apiXmlPath = Path.Combine(AppContext.BaseDirectory, apiXmlFile);
    if (File.Exists(apiXmlPath))
        options.IncludeXmlComments(apiXmlPath);

    // Include XML comments from Application project
    var appXmlFile = "FamilyRelocation.Application.xml";
    var appXmlPath = Path.Combine(AppContext.BaseDirectory, appXmlFile);
    if (File.Exists(appXmlPath))
        options.IncludeXmlComments(appXmlPath);
});

// Add Application services (MediatR, FluentValidation)
builder.Services.AddApplication();

// Add Infrastructure services (AWS Cognito, Database, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// Add HTTP context accessor for CurrentUserService
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "postgresql",
        tags: ["db", "sql", "postgresql"]);

// Add memory cache for image proxying
builder.Services.AddMemoryCache(options =>
{
    // Limit cache to ~100MB for images
    options.SizeLimit = 100 * 1024 * 1024;
});

// Add JWT authentication
var cognitoAuthority = builder.Configuration["AWS:Cognito:Authority"]
    ?? throw new InvalidOperationException("AWS:Cognito:Authority configuration is required");
var cognitoClientId = builder.Configuration["AWS:Cognito:ClientId"]
    ?? throw new InvalidOperationException("AWS:Cognito:ClientId configuration is required");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = cognitoAuthority;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateLifetime = true,
            // NOTE: ValidateAudience is disabled because AWS Cognito ACCESS tokens use 'client_id'
            // claim instead of 'aud' for audience. ID tokens have 'aud', but we use access tokens
            // for API authorization. We manually validate 'client_id' in OnTokenValidated below.
            // See: https://docs.aws.amazon.com/cognito/latest/developerguide/amazon-cognito-user-pools-using-tokens-verifying-a-jwt.html
            ValidateAudience = false
        };

        // Validate Cognito's client_id claim and map database roles after token is validated
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var clientIdClaim = context.Principal?.FindFirst("client_id");
                if (clientIdClaim == null || clientIdClaim.Value != cognitoClientId)
                {
                    context.Fail("Invalid client_id claim");
                    return;
                }

                // Get user's Cognito ID (sub claim) from the access token
                var identity = context.Principal?.Identity as System.Security.Claims.ClaimsIdentity;
                var logger = context.HttpContext.RequestServices.GetService<ILogger<Program>>();

                // Log all claims for debugging
                if (identity != null)
                {
                    var allClaims = identity.Claims.Select(c => $"{c.Type}={c.Value}");
                    logger?.LogInformation("Access token claims: {Claims}", string.Join(", ", allClaims));
                }

                // Try multiple claim names for sub (Cognito sometimes uses different formats)
                var subClaim = context.Principal?.FindFirst("sub")
                    ?? context.Principal?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
                    ?? context.Principal?.FindFirst("username");

                if (identity != null && subClaim != null)
                {
                    var cognitoUserId = subClaim.Value;
                    logger?.LogInformation("Looking up roles for user: {UserId}", cognitoUserId);

                    try
                    {
                        // Look up roles from the database
                        var userRoleService = context.HttpContext.RequestServices.GetRequiredService<IUserRoleService>();
                        var roles = await userRoleService.GetUserRolesAsync(cognitoUserId);

                        logger?.LogInformation("Found {Count} roles in database for user {UserId}: {Roles}",
                            roles.Count, cognitoUserId, string.Join(", ", roles));

                        // Add roles as claims
                        foreach (var role in roles)
                        {
                            if (!identity.HasClaim(System.Security.Claims.ClaimTypes.Role, role))
                            {
                                identity.AddClaim(new System.Security.Claims.Claim(
                                    System.Security.Claims.ClaimTypes.Role, role));
                            }
                        }

                        // Log final roles
                        var finalRoles = identity.Claims
                            .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                            .Select(c => c.Value);
                        logger?.LogInformation("Final roles added to identity: {Roles}", string.Join(", ", finalRoles));
                    }
                    catch (Exception ex)
                    {
                        logger?.LogError(ex, "Failed to look up user roles from database");
                        // If role lookup fails, continue without roles
                        // The user is still authenticated via access token
                    }
                }
                else
                {
                    logger?.LogWarning("No sub claim found in access token");
                }
            }
        };
    });

builder.Services.AddAuthorization();

// Add CORS policy (H-006: No hardcoded fallback in production)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

        if (allowedOrigins == null || allowedOrigins.Length == 0)
        {
            // Allow localhost fallback in Development and Testing environments only
            // Testing environment uses HttpClient directly (no CORS), but needs valid policy
            var isNonProductionEnv = builder.Environment.IsDevelopment() ||
                                     builder.Environment.EnvironmentName == "Testing";

            if (isNonProductionEnv)
            {
                allowedOrigins = ["http://localhost:5173", "http://localhost:3000"];
            }
            else
            {
                throw new InvalidOperationException(
                    "Cors:AllowedOrigins must be configured in production. " +
                    "Add it to appsettings.Production.json or environment variables.");
            }
        }

        policy.WithOrigins(allowedOrigins)
              .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS")
              .WithHeaders("Authorization", "Content-Type", "X-Cognito-Id-Token")
              .AllowCredentials();
    });
});

// Add rate limiting for auth endpoints (H-003: Brute force protection)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Auth endpoints: 10 requests per minute per IP (login, forgot-password, etc.)
    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Stricter limit for login specifically: 5 attempts per minute per IP
    options.AddPolicy("auth-login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Public form submissions: 5 per hour per IP
    options.AddPolicy("public-form", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromHours(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

// Add global exception handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "FamilyRelocation API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseRateLimiter();
app.UseAuthentication();

// TEMPORARY: Bypass auth in Development for testing
if (app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        // Add fake admin identity for unauthenticated requests
        if (context.User.Identity?.IsAuthenticated != true)
        {
            var devUserId = "00000000-0000-0000-0000-000000000001";
            var claims = new[]
            {
                new System.Security.Claims.Claim("sub", devUserId),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, devUserId),
                new System.Security.Claims.Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Admin"),
                new System.Security.Claims.Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Coordinator"),
                new System.Security.Claims.Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Broker"),
                new System.Security.Claims.Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "BoardMember"),
            };
            var identity = new System.Security.Claims.ClaimsIdentity(claims, "DevBypass");
            context.User = new System.Security.Claims.ClaimsPrincipal(identity);
        }
        await next();
    });
}

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
