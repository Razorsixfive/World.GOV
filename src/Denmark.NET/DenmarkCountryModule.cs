using System.Reflection;
using System.Text.Json;
using World.GOV.Core;

namespace Denmark.NET;

public sealed class DenmarkCountryModule : ICountryModule
{
    private const string IncidentResourcePrefix = "Denmark.NET.Incidents.";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public CountryDefinition Definition { get; } = ReadDefinition();

    public IEnumerable<GovernmentIncident> GetIncidents()
    {
        var assembly = typeof(DenmarkCountryModule).Assembly;
        var resourceNames = assembly.GetManifestResourceNames()
            .Where(name => name.StartsWith(IncidentResourcePrefix, StringComparison.Ordinal))
            .OrderBy(name => name, StringComparer.Ordinal);

        var incidents = new List<GovernmentIncident>();
        foreach (var resourceName in resourceNames)
        {
            using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Could not read embedded incident {resourceName}.");
            var incident = JsonSerializer.Deserialize<GovernmentIncident>(stream, JsonOptions)
                ?? throw new InvalidOperationException($"Incident resource {resourceName} was empty.");
            incidents.Add(incident);
        }

        return incidents.OrderBy(incident => incident.Date).ToArray();
    }

    private static CountryDefinition ReadDefinition()
    {
        var assembly = typeof(DenmarkCountryModule).Assembly;
        using var stream = assembly.GetManifestResourceStream("Denmark.NET.country.json")
            ?? throw new InvalidOperationException("The Denmark country manifest is missing.");
        return JsonSerializer.Deserialize<CountryDefinition>(stream, JsonOptions)
            ?? throw new InvalidOperationException("The Denmark country manifest is empty.");
    }
}
