using System.Reflection;
using System.Text.Json;
using World.GOV.Core;

namespace World.GOV.CountryTemplate;

public sealed class CountryModule : ICountryModule
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public CountryDefinition Definition { get; } = ReadDefinition();

    public IEnumerable<GovernmentIncident> GetIncidents()
    {
        var assembly = typeof(CountryModule).Assembly;
        var resourcePrefix = $"{assembly.GetName().Name}.Incidents.";
        var resourceNames = assembly.GetManifestResourceNames()
            .Where(name => name.StartsWith(resourcePrefix, StringComparison.Ordinal))
            .OrderBy(name => name, StringComparer.Ordinal);

        foreach (var resourceName in resourceNames)
        {
            using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Could not read embedded incident {resourceName}.");
            yield return JsonSerializer.Deserialize<GovernmentIncident>(stream, JsonOptions)
                ?? throw new InvalidOperationException($"Incident resource {resourceName} was empty.");
        }
    }

    private static CountryDefinition ReadDefinition()
    {
        var assembly = typeof(CountryModule).Assembly;
        var resourceName = $"{assembly.GetName().Name}.country.json";
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException("The country manifest is missing from this module.");
        return JsonSerializer.Deserialize<CountryDefinition>(stream, JsonOptions)
            ?? throw new InvalidOperationException("The country manifest is empty.");
    }
}
