using Amazon.CognitoIdentityProvider;
using Amazon.S3;
using FamilyRelocation.Application.Auth;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Infrastructure.AWS;
using FamilyRelocation.Infrastructure.Persistence;
using FamilyRelocation.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyRelocation.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Interceptors
        services.AddScoped<AuditingInterceptor>();

        // Database
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                }
            );

            // Add audit interceptor
            options.AddInterceptors(serviceProvider.GetRequiredService<AuditingInterceptor>());
        });

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // AWS Services
        services.AddDefaultAWSOptions(configuration.GetAWSOptions());
        services.AddAWSService<IAmazonCognitoIdentityProvider>();
        services.AddAWSService<IAmazonS3>();

        // Authentication
        services.AddScoped<IAuthenticationService, CognitoAuthenticationService>();

        // Document Storage
        services.AddScoped<IDocumentStorageService, S3DocumentStorageService>();

        return services;
    }
}
