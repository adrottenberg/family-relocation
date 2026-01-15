using System.Runtime.CompilerServices;

namespace FamilyRelocation.IntegrationTests;

/// <summary>
/// Module initializer that runs before any tests to set the environment.
/// </summary>
public static class Startup
{
    [ModuleInitializer]
    public static void Initialize()
    {
        // Set the environment before any tests run
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
    }
}
