using System.Net.Http.Json;
using System.Text.Json;
using World.GOV.Core;

namespace World.GOV.Web.Services;

public sealed class PortalDataService(HttpClient http)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private Task? _loadTask;

    public PublishManifest? Manifest { get; private set; }
    public IReadOnlyList<CountryDefinition> Countries { get; private set; } = [];
    public IReadOnlyList<GovernmentIncident> Incidents { get; private set; } = [];
    public IReadOnlyList<PublishedSource> Sources { get; private set; } = [];
    public IReadOnlyList<VirtualCommit> Timelines { get; private set; } = [];
    public IReadOnlyList<GovernmentDiagnostic> Diagnostics { get; private set; } = [];
    public IReadOnlyList<StandardIndexEntry> Standards { get; private set; } = [];
    public BuildReport? BuildReport { get; private set; }
    public string? LoadError { get; private set; }

    public Task EnsureLoadedAsync() => _loadTask ??= LoadAsync();

    private async Task LoadAsync()
    {
        try
        {
            Manifest = await http.GetFromJsonAsync<PublishManifest>($"data/manifest.json?fresh={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}", JsonOptions);
            Countries = await http.GetFromJsonAsync<CountryDefinition[]>(Versioned("data/countries.json"), JsonOptions) ?? [];
            Sources = await http.GetFromJsonAsync<PublishedSource[]>(Versioned("data/sources/index.json"), JsonOptions) ?? [];
            var incidents = new List<GovernmentIncident>();
            var timelines = new List<VirtualCommit>();
            var diagnostics = new List<GovernmentDiagnostic>();
            foreach (var country in Countries)
            {
                incidents.AddRange(await http.GetFromJsonAsync<GovernmentIncident[]>(Versioned($"data/incidents/{country.Id}.json"), JsonOptions) ?? []);
                timelines.AddRange(await http.GetFromJsonAsync<VirtualCommit[]>(Versioned($"data/timelines/{country.Id}.json"), JsonOptions) ?? []);
                diagnostics.AddRange(await http.GetFromJsonAsync<GovernmentDiagnostic[]>(Versioned($"data/diagnostics/{country.Id}.json"), JsonOptions) ?? []);
            }
            Incidents = incidents.OrderBy(incident => incident.Date).ToArray();
            Timelines = timelines.OrderBy(commit => commit.Date, StringComparer.Ordinal).ToArray();
            Diagnostics = diagnostics.OrderBy(diagnostic => diagnostic.Id, StringComparer.Ordinal).ToArray();
            Standards = await http.GetFromJsonAsync<StandardIndexEntry[]>(Versioned("data/standards/index.json"), JsonOptions) ?? [];
            BuildReport = await http.GetFromJsonAsync<BuildReport>(Versioned("data/build-report.json"), JsonOptions);
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException or NotSupportedException)
        {
            LoadError = "Published project data is not available yet. Run the worldgov export command and reload this page.";
        }
    }

    public GovernmentIncident? FindIncident(string incidentId) => Incidents.FirstOrDefault(incident =>
        string.Equals(incident.Id, incidentId, StringComparison.OrdinalIgnoreCase));

    public PublishedSource? FindSource(string sourceId) => Sources.FirstOrDefault(source =>
        string.Equals(source.Id, sourceId, StringComparison.OrdinalIgnoreCase));

    public CountryDefinition? FindCountry(string countryId) => Countries.FirstOrDefault(country =>
        string.Equals(country.Id, countryId, StringComparison.OrdinalIgnoreCase));

    public async Task<string?> GetStandardContentAsync(string contentPath)
    {
        try
        {
            return await http.GetStringAsync(Versioned($"data/{contentPath}"));
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public string? GetSourceCodeUrl(string? sourcePath)
    {
        if (string.IsNullOrWhiteSpace(Manifest?.RepositoryUrl) || string.IsNullOrWhiteSpace(sourcePath)) return null;
        var branch = string.IsNullOrWhiteSpace(Manifest.SourceBranch) ? "main" : Manifest.SourceBranch;
        var encodedPath = string.Join('/', sourcePath.Split('/').Select(Uri.EscapeDataString));
        return $"{Manifest.RepositoryUrl.TrimEnd('/')}/blob/{Uri.EscapeDataString(branch)}/{encodedPath}";
    }

    private string Versioned(string path)
    {
        var version = Manifest?.GeneratedAt.ToUnixTimeSeconds();
        return version is null ? path : $"{path}?v={version.Value}";
    }
}
