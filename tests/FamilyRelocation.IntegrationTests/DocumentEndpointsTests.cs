using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FamilyRelocation.IntegrationTests;

public class DocumentEndpointsTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public DocumentEndpointsTests()
    {
        var dbName = "TestDb_" + Guid.NewGuid().ToString();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                {
                    // Remove all EF Core related services
                    var descriptorsToRemove = services
                        .Where(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>)
                            || d.ServiceType == typeof(DbContextOptions)
                            || d.ServiceType == typeof(IApplicationDbContext)
                            || d.ServiceType == typeof(ApplicationDbContext)
                            || d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true)
                        .ToList();

                    foreach (var descriptor in descriptorsToRemove)
                    {
                        services.Remove(descriptor);
                    }

                    // Add in-memory database
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase(dbName);
                    });

                    // Re-add IApplicationDbContext pointing to the in-memory context
                    services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

                    // Add test authentication scheme
                    services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = "TestScheme";
                        options.DefaultChallengeScheme = "TestScheme";
                    })
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", _ => { });
                });
            });

        // Seed the database
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();
        }

        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    #region Document Types Tests

    [Fact]
    public async Task GetDocumentTypes_ReturnsOk_WithDocumentTypesList()
    {
        // Act
        var response = await _client.GetAsync("/api/document-types");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var documentTypes = JsonSerializer.Deserialize<List<DocumentTypeDto>>(content, _jsonOptions);
        documentTypes.Should().NotBeNull();
        // Should have seeded document types (BrokerAgreement, CommunityTakanos)
        documentTypes!.Should().Contain(dt => dt.Name == "BrokerAgreement");
        documentTypes!.Should().Contain(dt => dt.Name == "CommunityTakanos");
    }

    [Fact]
    public async Task GetDocumentTypes_WithActiveOnlyFalse_ReturnsAllTypes()
    {
        // Act
        var response = await _client.GetAsync("/api/document-types?activeOnly=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Applicant Documents Tests

    [Fact]
    public async Task GetApplicantDocuments_WithValidApplicant_ReturnsOk()
    {
        // Arrange - Create an applicant first (anonymous endpoint)
        var createResponse = await _client.PostAsJsonAsync("/api/applicants", new
        {
            husband = new { firstName = "Test", lastName = "DocUser" }
        });
        createResponse.EnsureSuccessStatusCode();
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createResult = JsonSerializer.Deserialize<CreateApplicantResponse>(createContent, _jsonOptions);

        // Act
        var response = await _client.GetAsync($"/api/documents/applicant/{createResult!.ApplicantId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var documents = JsonSerializer.Deserialize<List<ApplicantDocumentDto>>(content, _jsonOptions);
        documents.Should().NotBeNull();
        documents.Should().BeEmpty(); // No documents uploaded yet
    }

    [Fact]
    public async Task GetApplicantDocuments_WithInvalidApplicant_ReturnsEmptyList()
    {
        // Act - For non-existent applicant, API returns empty list (not 404)
        var response = await _client.GetAsync($"/api/documents/applicant/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var documents = JsonSerializer.Deserialize<List<ApplicantDocumentDto>>(content, _jsonOptions);
        documents.Should().BeEmpty();
    }

    #endregion

    #region Stage Requirements Tests

    [Fact]
    public async Task GetStageRequirements_SearchingToUnderContract_ReturnsEmptyRequirements()
    {
        // Note: With the refactored stage model, document requirements for board approval
        // are now checked at the Applicant level, not HousingSearch transitions.
        // This test verifies that valid stage transitions can be queried.

        // Act
        var response = await _client.GetAsync("/api/stage-requirements/Searching/UnderContract");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var requirements = JsonSerializer.Deserialize<StageRequirementsDto>(content, _jsonOptions);
        requirements.Should().NotBeNull();
        requirements!.FromStage.Should().Be("Searching");
        requirements.ToStage.Should().Be("UnderContract");
        // No document requirements configured for this transition by default
        requirements.Requirements.Should().BeEmpty();
    }

    [Fact]
    public async Task GetStageRequirements_WithApplicantId_ReturnsValidResponse()
    {
        // Arrange - Create an applicant first
        var createResponse = await _client.PostAsJsonAsync("/api/applicants", new
        {
            husband = new { firstName = "Test", lastName = "ReqUser" }
        });
        createResponse.EnsureSuccessStatusCode();
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createResult = JsonSerializer.Deserialize<CreateApplicantResponse>(createContent, _jsonOptions);

        // Act - Query a valid stage transition with applicantId
        var response = await _client.GetAsync($"/api/stage-requirements/Searching/UnderContract?applicantId={createResult!.ApplicantId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var requirements = JsonSerializer.Deserialize<StageRequirementsDto>(content, _jsonOptions);
        requirements.Should().NotBeNull();
        // No requirements configured, so empty list (but with applicant context working)
        requirements!.Requirements.Should().BeEmpty();
    }

    [Fact]
    public async Task GetStageRequirements_UnderContractToClosed_ReturnsEmptyList()
    {
        // Act - Try a transition that has no document requirements
        var response = await _client.GetAsync("/api/stage-requirements/UnderContract/Closed");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var requirements = JsonSerializer.Deserialize<StageRequirementsDto>(content, _jsonOptions);
        requirements.Should().NotBeNull();
        requirements!.Requirements.Should().BeEmpty();
    }

    #endregion

    #region Delete Document Tests

    [Fact]
    public async Task DeleteDocument_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/documents/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region DTOs for deserialization

    private record DocumentTypeDto(Guid Id, string Name, string DisplayName, string? Description, bool IsActive, bool IsSystemType);
    private record ApplicantDocumentDto(Guid Id, Guid DocumentTypeId, string DocumentTypeName, string FileName, string StorageKey, string ContentType, long FileSizeBytes, DateTime UploadedAt, string? UploadedBy);
    private record StageRequirementsDto(string FromStage, string ToStage, List<DocumentRequirementDto> Requirements);
    private record DocumentRequirementDto(Guid DocumentTypeId, string DocumentTypeName, bool IsRequired, bool IsUploaded);
    private record CreateApplicantResponse(Guid ApplicantId, Guid HousingSearchId);

    #endregion
}

/// <summary>
/// Test authentication handler that automatically authenticates all requests.
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim(ClaimTypes.Name, "test@example.com"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestScheme");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
