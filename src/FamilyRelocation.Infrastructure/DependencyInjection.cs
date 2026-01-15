using Amazon.CognitoIdentityProvider;
using FamilyRelocation.Application.Auth;
using FamilyRelocation.Infrastructure.AWS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyRelocation.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // AWS Services
        services.AddDefaultAWSOptions(configuration.GetAWSOptions());
        services.AddAWSService<IAmazonCognitoIdentityProvider>();

        // Authentication
        services.AddScoped<IAuthenticationService, CognitoAuthenticationService>();

        return services;
    }
}
