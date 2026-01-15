using FamilyRelocation.Application.Auth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyRelocation.IntegrationTests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
    where TProgram : class
{
    public IAuthenticationService? MockAuthService { get; set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Use Testing environment to pick up appsettings.Testing.json from API project
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            if (MockAuthService != null)
            {
                // Remove the real auth service registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IAuthenticationService));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add the mock
                services.AddSingleton(MockAuthService);
            }
        });
    }
}
