using System.Text.Json;
using World.GOV.Core;

namespace World.GOV.Tooling;

public static class StaticSiteExporter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public static string Export(
        ContentCatalog catalog,
        string? outputPath = null,
        string? buildStatus = null)
    {
        var validationIssues = ContentValidator.Validate(catalog);
        var errors = validationIssues.Where(issue => issue.Severity == "ERROR").ToArray();
        if (errors.Length > 0)
            throw new InvalidOperationException($"Export refused because {errors.Length} content validation error(s) were found.");

        var outputRoot = Path.GetFullPath(Path.Combine(catalog.RepositoryRoot, outputPath ?? "publish-data"));
        var repositoryRoot = Path.GetFullPath(catalog.RepositoryRoot).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!outputRoot.StartsWith(repositoryRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Export output must stay inside the World.GOV repository.");

        Directory.CreateDirectory(outputRoot);
        var generatedAt = GetGeneratedAt();
        var countries = catalog.Countries.OrderBy(country => country.Id, StringComparer.Ordinal).ToArray();
        var diagnostics = catalog.Incidents
            .SelectMany(incident => incident.CountryIds.Select(countryId => ToDiagnostic(incident, countryId)))
            .ToArray();
        var buildReport = new BuildReport
        {
            BuildStatus = NormalizeBuildStatus(buildStatus),
            StatusSource = string.Equals(Environment.GetEnvironmentVariable("GITHUB_ACTIONS"), "true", StringComparison.OrdinalIgnoreCase)
                ? "GitHub Actions build step"
                : string.IsNullOrWhiteSpace(buildStatus) ? "Local export; no build result supplied" : "Local CLI build-status option",
            GeneratedAt = generatedAt,
            Commit = Environment.GetEnvironmentVariable("GITHUB_SHA"),
            Workflow = Environment.GetEnvironmentVariable("GITHUB_WORKFLOW"),
            RegisteredDiagnostics = diagnostics.Length
        };

        WriteJson(outputRoot, "manifest.json", new PublishManifest(
            1,
            generatedAt,
            countries.Select(country => country.Id).ToArray(),
            ["countries.json", "incidents", "timelines", "diagnostics", "standards", "sources/index.json", "build-report.json"],
            GetRepositoryUrl(),
            Environment.GetEnvironmentVariable("GITHUB_REF_NAME")));
        WriteJson(outputRoot, "countries.json", countries);
        WriteJson(outputRoot, "sources/index.json", catalog.GetSourceRegistry());
        WriteJson(outputRoot, "standards/index.json", catalog.Standards
            .OrderBy(standard => standard.Id, StringComparer.Ordinal)
            .Select(standard => new StandardIndexEntry(
                standard.Id,
                standard.Country,
                standard.Title,
                $"standards/docs/{standard.Country}-{standard.Id}.md"))
            .ToArray());
        WriteJson(outputRoot, "build-report.json", buildReport);

        foreach (var standard in catalog.Standards)
        {
            var relative = $"standards/docs/{standard.Country}-{standard.Id}.md";
            var fullPath = Path.Combine(outputRoot, relative.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllText(fullPath, standard.Content, new System.Text.UTF8Encoding(false));
        }

        foreach (var country in countries)
        {
            var countryIncidents = catalog.GetByCountry(country.Id)
                .OrderBy(incident => incident.Date)
                .ThenBy(incident => incident.Id, StringComparer.Ordinal)
                .ToArray();
            WriteJson(outputRoot, $"incidents/{country.Id}.json", countryIncidents);
            WriteJson(outputRoot, $"timelines/{country.Id}.json", VirtualHistoryBuilder.Create(countryIncidents));
            WriteJson(outputRoot, $"diagnostics/{country.Id}.json", diagnostics
                .Where(diagnostic => string.Equals(diagnostic.Country, country.Id, StringComparison.OrdinalIgnoreCase))
                .OrderBy(diagnostic => diagnostic.Id, StringComparer.Ordinal)
                .ToArray());
        }

        return outputRoot;
    }

    private static GovernmentDiagnostic ToDiagnostic(GovernmentIncident incident, string countryId) => new()
    {
        Id = incident.Satire.DiagnosticId,
        Country = countryId,
        Severity = incident.Satire.Severity,
        Message = incident.Satire.DiagnosticMessage,
        IncidentId = incident.Id,
        StandardId = incident.Satire.Standards.FirstOrDefault(),
        SourceLocation = incident.SourcePath,
        Acknowledged = incident.Satire.Acknowledged,
        IgnoredByPolicy = incident.Satire.IgnoredByPolicy
    };

    private static void WriteJson(string outputRoot, string relativePath, object value)
    {
        var fullPath = Path.Combine(outputRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, JsonSerializer.Serialize(value, JsonOptions), new System.Text.UTF8Encoding(false));
    }

    private static DateTimeOffset GetGeneratedAt()
    {
        var sourceDateEpoch = Environment.GetEnvironmentVariable("SOURCE_DATE_EPOCH");
        if (long.TryParse(sourceDateEpoch, out var seconds))
            return DateTimeOffset.FromUnixTimeSeconds(seconds);
        return DateTimeOffset.UtcNow;
    }

    private static string NormalizeBuildStatus(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "Not reported";
        if (string.Equals(value, "succeeded", StringComparison.OrdinalIgnoreCase)) return "Succeeded";
        if (string.Equals(value, "failed", StringComparison.OrdinalIgnoreCase)) return "Failed";
        return "Not reported";
    }

    private static string? GetRepositoryUrl()
    {
        var repository = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY");
        return string.IsNullOrWhiteSpace(repository) ? null : $"https://github.com/{repository.Trim('/')}";
    }
}
