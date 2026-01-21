using FamilyRelocation.Domain.Common;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using FamilyRelocation.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace FamilyRelocation.Infrastructure.Persistence.Seeders;

/// <summary>
/// Seeds properties from Redfin CSV export files.
/// CSV columns: SALE TYPE, SOLD DATE, PROPERTY TYPE, ADDRESS, CITY, STATE OR PROVINCE,
/// ZIP OR POSTAL CODE, PRICE, BEDS, BATHS, LOCATION, SQUARE FEET, LOT SIZE, YEAR BUILT,
/// DAYS ON MARKET, $/SQUARE FEET, HOA/MONTH, STATUS, NEXT OPEN HOUSE START TIME,
/// NEXT OPEN HOUSE END TIME, URL, SOURCE, MLS#, FAVORITE, INTERESTED, LATITUDE, LONGITUDE
/// </summary>
public class RedfnPropertySeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RedfnPropertySeeder> _logger;

    public RedfnPropertySeeder(ApplicationDbContext context, ILogger<RedfnPropertySeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedFromCsvAsync(string csvFilePath, CancellationToken ct = default)
    {
        if (!File.Exists(csvFilePath))
        {
            _logger.LogError("CSV file not found: {FilePath}", csvFilePath);
            return;
        }

        var lines = await File.ReadAllLinesAsync(csvFilePath, ct);
        if (lines.Length < 2)
        {
            _logger.LogWarning("CSV file has no data rows: {FilePath}", csvFilePath);
            return;
        }

        var header = ParseCsvLine(lines[0]);
        var columnMap = CreateColumnMap(header);

        var propertiesToAdd = new List<Property>();
        var existingMlsNumbers = await _context.Set<Property>()
            .Where(p => !p.IsDeleted)
            .Select(p => p.MlsNumber)
            .Where(m => m != null)
            .ToListAsync(ct);

        var existingMlsSet = new HashSet<string>(existingMlsNumbers!);

        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i];

            // Skip disclaimer line or empty lines
            if (string.IsNullOrWhiteSpace(line) || line.Contains("In accordance with local MLS rules"))
                continue;

            try
            {
                var property = ParsePropertyFromLine(line, columnMap, existingMlsSet);
                if (property != null)
                {
                    propertiesToAdd.Add(property);
                    if (property.MlsNumber != null)
                        existingMlsSet.Add(property.MlsNumber);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse line {LineNumber}: {Line}", i + 1, line);
            }
        }

        if (propertiesToAdd.Count > 0)
        {
            foreach (var property in propertiesToAdd)
            {
                _context.Add(property);
            }
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Seeded {Count} properties from {FilePath}", propertiesToAdd.Count, csvFilePath);
        }
        else
        {
            _logger.LogInformation("No new properties to seed from {FilePath}", csvFilePath);
        }
    }

    public async Task SeedFromMultipleCsvsAsync(IEnumerable<string> csvFilePaths, CancellationToken ct = default)
    {
        foreach (var path in csvFilePaths)
        {
            await SeedFromCsvAsync(path, ct);
        }
    }

    private Dictionary<string, int> CreateColumnMap(string[] header)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < header.Length; i++)
        {
            map[header[i].Trim()] = i;
        }
        return map;
    }

    private Property? ParsePropertyFromLine(string line, Dictionary<string, int> columnMap, HashSet<string> existingMlsNumbers)
    {
        var values = ParseCsvLine(line);

        string GetValue(string columnName)
        {
            if (columnMap.TryGetValue(columnName, out var index) && index < values.Length)
                return values[index].Trim().Trim('"');
            return string.Empty;
        }

        int? GetIntValue(string columnName)
        {
            var val = GetValue(columnName);
            if (string.IsNullOrEmpty(val)) return null;
            return int.TryParse(val, out var result) ? result : null;
        }

        decimal? GetDecimalValue(string columnName)
        {
            var val = GetValue(columnName);
            if (string.IsNullOrEmpty(val)) return null;
            // Remove commas and dollar signs
            val = val.Replace(",", "").Replace("$", "");
            return decimal.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : null;
        }

        // Parse MLS number first - skip if already exists
        var mlsNumber = GetValue("MLS#");
        if (!string.IsNullOrEmpty(mlsNumber) && existingMlsNumbers.Contains(mlsNumber))
        {
            _logger.LogDebug("Skipping duplicate MLS#: {MlsNumber}", mlsNumber);
            return null;
        }

        // Parse required fields
        var addressStr = GetValue("ADDRESS");
        var city = GetValue("CITY");
        var state = GetValue("STATE OR PROVINCE");
        var zip = GetValue("ZIP OR POSTAL CODE");
        var priceValue = GetDecimalValue("PRICE");
        var beds = GetIntValue("BEDS");
        var baths = GetDecimalValue("BATHS");

        // Validate required fields
        if (string.IsNullOrEmpty(addressStr) || string.IsNullOrEmpty(city) || !priceValue.HasValue || !beds.HasValue || !baths.HasValue)
        {
            _logger.LogDebug("Skipping row with missing required fields: {Address}", addressStr);
            return null;
        }

        // Parse optional fields
        var sqft = GetIntValue("SQUARE FEET");
        var lotSizeValue = GetDecimalValue("LOT SIZE");
        var yearBuilt = GetIntValue("YEAR BUILT");
        var redfnStatus = GetValue("STATUS");
        var url = GetValue("URL (SEE https://www.redfin.com/buy-a-home/comparative-market-analysis FOR INFO ON PRICING)");
        var location = GetValue("LOCATION");

        // Map Redfin status to our ListingStatus
        var status = MapRedfnStatus(redfnStatus);

        // Build notes with Redfin URL and additional info
        var notes = BuildNotes(url, location, GetValue("PROPERTY TYPE"));

        // Create address (Redfin doesn't give us street2)
        var address = new Address(
            street: addressStr,
            street2: null,
            city: city,
            state: state,
            zipCode: zip
        );

        // Create price
        var price = new Money(priceValue.Value);

        // Create property using system user for seeding
        var property = Property.Create(
            address: address,
            price: price,
            bedrooms: beds.Value,
            bathrooms: baths.Value,
            createdBy: WellKnownIds.SystemUserId,
            squareFeet: sqft,
            lotSize: lotSizeValue,
            yearBuilt: yearBuilt,
            annualTaxes: null, // Redfin doesn't provide this in CSV
            features: null,
            mlsNumber: string.IsNullOrEmpty(mlsNumber) ? null : mlsNumber,
            notes: notes
        );

        // Update status if not Active (default)
        if (status != ListingStatus.Active)
        {
            property.UpdateStatus(status, WellKnownIds.SystemUserId);
        }

        return property;
    }

    private static ListingStatus MapRedfnStatus(string redfnStatus)
    {
        return redfnStatus?.ToLower() switch
        {
            "active" => ListingStatus.Active,
            "pre on-market" => ListingStatus.Active,
            "coming soon" => ListingStatus.Active,
            "pending" => ListingStatus.UnderContract,
            "under contract" => ListingStatus.UnderContract,
            "contingent" => ListingStatus.UnderContract,
            "sold" => ListingStatus.Sold,
            "off market" => ListingStatus.OffMarket,
            "withdrawn" => ListingStatus.OffMarket,
            "expired" => ListingStatus.OffMarket,
            _ => ListingStatus.Active
        };
    }

    private static string? BuildNotes(string url, string location, string propertyType)
    {
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(propertyType))
            parts.Add($"Type: {propertyType}");

        if (!string.IsNullOrEmpty(location))
            parts.Add($"Area: {location}");

        if (!string.IsNullOrEmpty(url))
            parts.Add($"Redfin: {url}");

        return parts.Count > 0 ? string.Join(" | ", parts) : null;
    }

    private static string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        var current = new System.Text.StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        result.Add(current.ToString());

        return result.ToArray();
    }
}
