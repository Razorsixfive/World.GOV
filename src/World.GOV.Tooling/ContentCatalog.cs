using System.Text.Json;
using System.Text.RegularExpressions;
using World.GOV.Core;

namespace World.GOV.Tooling;

public sealed class ContentCatalog : IIncidentCatalog, IHistoricalTimeline
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly Dictionary<string, IReadOnlyList<GovernmentIncident>> _byCountry;

    private ContentCatalog(
        string repositoryRoot,
        IReadOnlyList<CountryDefinition> countries,
        IReadOnlyList<GovernmentIncident> incidents,
        IReadOnlyList<StandardDocument> standards,
        IReadOnlyList<SourceReference> sources,
        IReadOnlyList<string> loadErrors)
    {
        RepositoryRoot = repositoryRoot;
        Countries = countries;
        Incidents = incidents;
        Standards = standards;
        Sources = sources;
        LoadErrors = loadErrors;
        _byCountry = incidents
            .SelectMany(incident => incident.CountryIds.Select(countryId => new { CountryId = countryId, Incident = incident }))
            .GroupBy(item => item.CountryId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<GovernmentIncident>)group.Select(item => item.Incident).DistinctBy(item => item.Id).ToArray(), StringComparer.OrdinalIgnoreCase);
    }

    public string RepositoryRoot { get; }
    public IReadOnlyList<CountryDefinition> Countries { get; }
    public IReadOnlyList<GovernmentIncident> Incidents { get; }
    public IReadOnlyList<StandardDocument> Standards { get; }
    public IReadOnlyList<SourceReference> Sources { get; }
    public IReadOnlyList<string> LoadErrors { get; }

    public static string FindRepositoryRoot(string? startDirectory = null)
    {
        foreach (var start in new[] { startDirectory, Environment.CurrentDirectory, AppContext.BaseDirectory })
        {
            if (string.IsNullOrWhiteSpace(start)) continue;

            DirectoryInfo? current = new(Path.GetFullPath(start));
            while (current is not null)
            {
                if (Directory.Exists(Path.Combine(current.FullName, "countries")) &&
                    File.Exists(Path.Combine(current.FullName, "World.GOV.sln")))
                {
                    return current.FullName;
                }
                current = current.Parent;
            }
        }

        throw new DirectoryNotFoundException("Could not find the World.GOV repository root. Pass --root <path>.");
    }

    public static ContentCatalog Load(string repositoryRoot)
    {
        var root = Path.GetFullPath(repositoryRoot);
        var countriesRoot = Path.Combine(root, "countries");
        if (!Directory.Exists(countriesRoot))
            throw new DirectoryNotFoundException($"Country data directory was not found: {countriesRoot}");

        var countries = new List<CountryDefinition>();
        var incidents = new List<GovernmentIncident>();
        var standards = new List<StandardDocument>();
        var sources = new List<SourceReference>();
        var errors = new List<string>();
        var countrySchema = new ContentSchema(Path.Combine(root, "schemas", "country-v1.schema.json"));
        var incidentSchema = new ContentSchema(Path.Combine(root, "schemas", "incident-v1.schema.json"));
        var sourceSchema = new ContentSchema(Path.Combine(root, "schemas", "source-registry-v1.schema.json"));

        foreach (var directory in Directory.EnumerateDirectories(countriesRoot).OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            var manifestPath = Path.Combine(directory, "country.json");
            if (!File.Exists(manifestPath))
            {
                errors.Add($"Missing country manifest: {ToRepoPath(root, manifestPath)}");
                continue;
            }

            CountryDefinition? country;
            try
            {
                country = countrySchema.Read<CountryDefinition>(manifestPath, JsonOptions);
                if (country is null) throw new JsonException("Manifest contained no object.");
                countries.Add(country);
            }
            catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException)
            {
                errors.Add($"Could not read {ToRepoPath(root, manifestPath)}: {exception.Message}");
                continue;
            }

            var sourceRegistryPath = Path.Combine(directory, "sources.json");
            if (!File.Exists(sourceRegistryPath))
            {
                errors.Add($"Missing source registry: {ToRepoPath(root, sourceRegistryPath)}");
            }
            else
            {
                try
                {
                    var registry = sourceSchema.Read<SourceRegistryFile>(sourceRegistryPath, JsonOptions);
                    if (registry.SchemaVersion != 1)
                        throw new JsonException($"Unsupported source registry schemaVersion {registry.SchemaVersion}.");
                    sources.AddRange(registry.Sources.Select(source => source with { CountryId = country.Id }));
                }
                catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException)
                {
                    errors.Add($"Could not read {ToRepoPath(root, sourceRegistryPath)}: {exception.Message}");
                }
            }

            foreach (var relativePath in country.IncidentFiles)
            {
                if (!TryResolveInside(root, directory, relativePath, out var incidentPath, out var pathError))
                {
                    errors.Add($"{ToRepoPath(root, manifestPath)}: {pathError}");
                    continue;
                }
                if (!File.Exists(incidentPath))
                {
                    errors.Add($"Incident file listed in {ToRepoPath(root, manifestPath)} does not exist: {relativePath}");
                    continue;
                }

                try
                {
                    var incident = incidentSchema.Read<GovernmentIncident>(incidentPath, JsonOptions);
                    if (incident is null) throw new JsonException("Incident file contained no object.");
                    if (!incident.CountryIds.Contains(country.Id, StringComparer.OrdinalIgnoreCase))
                        throw new JsonException($"Incident countryIds do not include its manifest country {country.Id}.");
                    incidents.Add(incident with
                    {
                        SourcePath = ToRepoPath(root, incidentPath)
                    });
                }
                catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException)
                {
                    errors.Add($"Could not read {ToRepoPath(root, incidentPath)}: {exception.Message}");
                }
            }

            foreach (var relativePath in country.StandardFiles)
            {
                if (!TryResolveInside(root, directory, relativePath, out var standardPath, out var pathError))
                {
                    errors.Add($"{ToRepoPath(root, manifestPath)}: {pathError}");
                    continue;
                }
                if (!File.Exists(standardPath))
                {
                    errors.Add($"Standard file listed in {ToRepoPath(root, manifestPath)} does not exist: {relativePath}");
                    continue;
                }

                var content = File.ReadAllText(standardPath);
                var firstHeading = Regex.Match(content, "(?m)^#\\s+(?<id>GOV-[A-Za-z0-9-]+):\\s*(?<title>.+)$");
                if (!firstHeading.Success)
                {
                    errors.Add($"Standard has no '# GOV-XXX: Title' heading: {ToRepoPath(root, standardPath)}");
                    continue;
                }
                standards.Add(new StandardDocument(
                    country.Id,
                    firstHeading.Groups["id"].Value,
                    firstHeading.Groups["title"].Value.Trim(),
                    ToRepoPath(root, standardPath),
                    content));
            }
        }

        return new ContentCatalog(root, countries, incidents, standards, sources, errors);
    }

    public IReadOnlyCollection<GovernmentIncident> GetByCountry(string countryId) =>
        _byCountry.TryGetValue(countryId, out var incidents) ? incidents : [];

    public GovernmentIncident? FindById(string incidentId) =>
        Incidents.FirstOrDefault(incident => string.Equals(incident.Id, incidentId, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<VirtualCommit> GetHistory(string countryId) =>
        VirtualHistoryBuilder.Create(GetByCountry(countryId));

    public IReadOnlyList<VirtualCommit> GetBlame(string entityId)
    {
        if (string.IsNullOrWhiteSpace(entityId)) return [];
        var incident = FindById(entityId);
        if (incident is null)
        {
            var byCountry = GetByCountry(entityId);
            if (byCountry.Count > 0) return VirtualHistoryBuilder.Create(byCountry);
            return Incidents.Where(item => string.Equals(item.SourcePath, entityId, StringComparison.OrdinalIgnoreCase))
                .SelectMany(item => new[] { item }.Concat(Incidents.Where(other => other.Relations.Any(relation => string.Equals(relation.IncidentId, item.Id, StringComparison.OrdinalIgnoreCase)))))
                .DistinctBy(item => item.Id)
                .OrderBy(item => item.Date)
                .Select(VirtualHistoryBuilder.Create)
                .ToArray();
        }

        var related = Incidents.Where(item => item.Id == incident.Id ||
                item.Relations.Any(relation => string.Equals(relation.IncidentId, incident.Id, StringComparison.OrdinalIgnoreCase)) ||
                incident.Relations.Any(relation => string.Equals(relation.IncidentId, item.Id, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(item => item.Date);
        return VirtualHistoryBuilder.Create(related);
    }

    public SourceReference? FindSource(string sourceId) => Sources.FirstOrDefault(source =>
        string.Equals(source.Id, sourceId, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<PublishedSource> GetSourceRegistry() => Sources
        .Select(source => new PublishedSource
        {
            Id = source.Id,
            Country = source.CountryId,
            Title = source.Title,
            Url = source.Url,
            Publisher = source.Publisher,
            Language = source.Language,
            PublishedDate = source.PublishedDate,
            AccessedDate = source.AccessedDate,
            Notes = source.Notes,
            IncidentIds = Incidents.Where(incident => incident.Historical.Sources.Contains(source.Id, StringComparer.OrdinalIgnoreCase))
                .Select(incident => incident.Id).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(id => id, StringComparer.Ordinal).ToArray()
        })
        .OrderBy(source => source.Id, StringComparer.OrdinalIgnoreCase)
        .ToArray();

    private static bool TryResolveInside(string root, string countryDirectory, string relativePath, out string fullPath, out string error)
    {
        fullPath = string.Empty;
        error = string.Empty;
        if (Path.IsPathRooted(relativePath) || relativePath.Contains('\\') || relativePath.Split('/').Contains(".."))
        {
            error = $"Path must be relative to its country folder: {relativePath}";
            return false;
        }

        var countryRoot = Path.GetFullPath(countryDirectory).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        fullPath = Path.GetFullPath(Path.Combine(countryDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        if (!fullPath.StartsWith(countryRoot, StringComparison.OrdinalIgnoreCase))
        {
            error = $"Path escapes its country folder: {relativePath}";
            return false;
        }
        if (!fullPath.StartsWith(Path.GetFullPath(root), StringComparison.OrdinalIgnoreCase))
        {
            error = $"Path escapes the repository: {relativePath}";
            return false;
        }
        return true;
    }

    private static string ToRepoPath(string root, string path) =>
        Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/');
}

public static class VirtualHistoryBuilder
{
    public static IReadOnlyList<VirtualCommit> Create(IEnumerable<GovernmentIncident> incidents) => incidents
        .OrderBy(incident => incident.Date)
        .ThenBy(incident => incident.Id, StringComparer.Ordinal)
        .Select(Create)
        .ToArray();

    public static VirtualCommit Create(GovernmentIncident incident)
    {
        var stableInput = $"{incident.Country}\n{incident.Id}\n{incident.Date.DisplayText}";
        var hashBytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(stableInput));
        var simulatedHash = Convert.ToHexString(hashBytes.AsSpan(0, 5)).ToLowerInvariant();
        return new VirtualCommit
        {
            Hash = $"SIM-{simulatedHash}",
            Country = incident.Country,
            IncidentId = incident.Id,
            Date = incident.Date.DisplayText,
            HistoricalDate = incident.Date,
            Message = incident.Satire.CommitMessage,
            Label = "Historical simulation — not an authentic Git commit",
            SourcePath = incident.SourcePath,
            IntroducedInGitCommit = incident.IntroducedInGitCommit
        };
    }
}
