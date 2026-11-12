using System.Net.Http.Json;
using System.Globalization;
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
    public IReadOnlyList<WebsiteLocale> WebsiteLocales { get; private set; } = [];

    public IReadOnlyList<string> AvailableLocales => Incidents
        .SelectMany(incident => incident.Translations.Keys)
        .Concat(WebsiteLocales.Where(locale => TranslatedStringCount(locale) > 0).Select(locale => locale.Locale))
        .Append("en-US")
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderBy(locale => string.Equals(locale, "en-US", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
        .ThenBy(locale => locale, StringComparer.OrdinalIgnoreCase)
        .ToArray();

    public Task EnsureLoadedAsync() => _loadTask ??= LoadAsync();

    private async Task LoadAsync()
    {
        try
        {
            Manifest = await http.GetFromJsonAsync<PublishManifest>($"data/manifest.json?fresh={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}", JsonOptions);
            Countries = await http.GetFromJsonAsync<CountryDefinition[]>(Versioned("data/countries.json"), JsonOptions) ?? [];
            Sources = await http.GetFromJsonAsync<PublishedSource[]>(Versioned("data/sources/index.json"), JsonOptions) ?? [];
            if (Manifest?.Datasets.Contains("locales/index.json") == true)
                WebsiteLocales = await http.GetFromJsonAsync<WebsiteLocale[]>(Versioned("data/locales/index.json"), JsonOptions) ?? [];
            var incidents = new List<GovernmentIncident>();
            var timelines = new List<VirtualCommit>();
            var diagnostics = new List<GovernmentDiagnostic>();
            foreach (var country in Countries)
            {
                incidents.AddRange(await http.GetFromJsonAsync<GovernmentIncident[]>(Versioned($"data/incidents/{country.Id}.json"), JsonOptions) ?? []);
                timelines.AddRange(await http.GetFromJsonAsync<VirtualCommit[]>(Versioned($"data/timelines/{country.Id}.json"), JsonOptions) ?? []);
                diagnostics.AddRange(await http.GetFromJsonAsync<GovernmentDiagnostic[]>(Versioned($"data/diagnostics/{country.Id}.json"), JsonOptions) ?? []);
            }
            Incidents = incidents
                .DistinctBy(incident => incident.Id, StringComparer.OrdinalIgnoreCase)
                .OrderBy(incident => incident.Date)
                .ThenBy(incident => incident.Id, StringComparer.Ordinal)
                .ToArray();
            Timelines = timelines
                .DistinctBy(commit => (commit.Country, commit.IncidentId))
                .OrderBy(commit => commit.HistoricalDate)
                .ThenBy(commit => commit.IncidentId, StringComparer.Ordinal)
                .ThenBy(commit => commit.Country, StringComparer.Ordinal)
                .ToArray();
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

    public IEnumerable<GovernmentIncident> GetIncidentsByCountry(string countryId) => Incidents.Where(incident =>
        incident.CountryIds.Contains(countryId, StringComparer.OrdinalIgnoreCase));

    public string Translate(string english, string? locale, params object?[] arguments)
    {
        var catalog = WebsiteLocales.FirstOrDefault(item => string.Equals(item.Locale, locale, StringComparison.OrdinalIgnoreCase));
        var knownEnglish = WebsiteLocales.FirstOrDefault(item => item.Locale == "en-US")?.Strings.ContainsKey(english) == true;
        var text = knownEnglish && !string.Equals(locale, "en-US", StringComparison.OrdinalIgnoreCase) &&
            catalog?.Strings.TryGetValue(english, out var translated) == true && !string.IsNullOrWhiteSpace(translated)
            ? translated : english;
        if (arguments.Length == 0) return text;
        try { return string.Format(CultureInfo.InvariantCulture, text, arguments); }
        catch (FormatException) { return string.Format(CultureInfo.InvariantCulture, english, arguments); }
    }

    public int TranslatedStringCount(WebsiteLocale locale)
    {
        var english = WebsiteLocales.FirstOrDefault(item => item.Locale == "en-US");
        return locale.Strings.Count(item => english?.Strings.ContainsKey(item.Key) == true && !string.IsNullOrWhiteSpace(item.Value));
    }

    public string LanguageName(string locale, bool native = false)
    {
        var catalog = WebsiteLocales.FirstOrDefault(item => string.Equals(item.Locale, locale, StringComparison.OrdinalIgnoreCase));
        if (catalog is not null) return native ? catalog.NativeName : catalog.EnglishName;
        try
        {
            var culture = CultureInfo.GetCultureInfo(locale);
            return native ? culture.NativeName : culture.EnglishName;
        }
        catch (CultureNotFoundException) { return locale; }
    }

    public string TextDirection(string locale) => WebsiteLocales.FirstOrDefault(item =>
        string.Equals(item.Locale, locale, StringComparison.OrdinalIgnoreCase))?.Direction ?? "ltr";

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
