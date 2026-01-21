using FamilyRelocation.Infrastructure.Persistence.Seeders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyRelocation.API.Controllers;

/// <summary>
/// Administrative operations including data seeding.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly RedfnPropertySeeder _propertySeeder;
    private readonly ILogger<AdminController> _logger;

    public AdminController(RedfnPropertySeeder propertySeeder, ILogger<AdminController> logger)
    {
        _propertySeeder = propertySeeder;
        _logger = logger;
    }

    /// <summary>
    /// Seed properties from Redfin CSV export files.
    /// </summary>
    /// <param name="csvPaths">Paths to CSV files to import</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Summary of seeded properties</returns>
    [HttpPost("seed-properties")]
    [ProducesResponseType(typeof(SeedResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SeedProperties([FromBody] SeedPropertiesRequest request, CancellationToken ct)
    {
        if (request.CsvPaths == null || request.CsvPaths.Count == 0)
        {
            return BadRequest(new { message = "At least one CSV path is required" });
        }

        // Validate all paths exist
        var missingFiles = request.CsvPaths.Where(p => !System.IO.File.Exists(p)).ToList();
        if (missingFiles.Any())
        {
            return BadRequest(new { message = "Files not found", files = missingFiles });
        }

        try
        {
            await _propertySeeder.SeedFromMultipleCsvsAsync(request.CsvPaths, ct);
            return Ok(new SeedResult
            {
                Message = $"Successfully processed {request.CsvPaths.Count} CSV file(s)",
                FilesProcessed = request.CsvPaths
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed properties");
            return BadRequest(new { message = "Failed to seed properties", error = ex.Message });
        }
    }
}

public class SeedPropertiesRequest
{
    public List<string> CsvPaths { get; set; } = new();
}

public class SeedResult
{
    public string Message { get; set; } = string.Empty;
    public List<string> FilesProcessed { get; set; } = new();
}
