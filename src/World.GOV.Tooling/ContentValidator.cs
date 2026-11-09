using System.Text.RegularExpressions;
using World.GOV.Core;

namespace World.GOV.Tooling;

public static partial class ContentValidator
{
    private static readonly HashSet<string> EvidenceStatuses = new(StringComparer.OrdinalIgnoreCase)
        { "Documented", "Interpretive", "Uncertain", "Disputed" };
    private static readonly HashSet<string> DiagnosticSeverities = new(StringComparer.OrdinalIgnoreCase)
        { "CRITICAL", "WARNING", "INFO" };
    private static readonly HashSet<string> DatePrecisions = new(StringComparer.OrdinalIgnoreCase)
        { "day", "month", "year", "range", "approximate", "unknown" };

    public static IReadOnlyList<ValidationIssue> Validate(ContentCatalog catalog)
    {
        var issues = new List<ValidationIssue>();
        foreach (var error in catalog.LoadErrors)
            issues.Add(new ValidationIssue("ERROR", "GOVDATA001", error));

        if (catalog.Countries.Count == 0)
            issues.Add(new ValidationIssue("ERROR", "GOVDATA002", "No valid country manifests were found."));

        foreach (var duplicate in catalog.Countries.GroupBy(country => country.Id, StringComparer.OrdinalIgnoreCase).Where(group => group.Count() > 1))
            issues.Add(new ValidationIssue("ERROR", "GOVDATA003", $"Duplicate country id: {duplicate.Key}"));
        foreach (var duplicate in catalog.Countries.GroupBy(country => country.Slug, StringComparer.OrdinalIgnoreCase).Where(group => group.Count() > 1))
            issues.Add(new ValidationIssue("ERROR", "GOVDATA004", $"Duplicate country slug: {duplicate.Key}"));
        foreach (var duplicate in catalog.Incidents.GroupBy(incident => incident.Id, StringComparer.OrdinalIgnoreCase).Where(group => group.Count() > 1))
            issues.Add(new ValidationIssue("ERROR", "GOVDATA005", $"Duplicate incident id: {duplicate.Key}"));

        var countriesById = catalog.Countries
            .GroupBy(country => country.Id, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        var incidentsById = catalog.Incidents
            .GroupBy(incident => incident.Id, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        var standardsByCountry = catalog.Standards
            .GroupBy(standard => standard.Country, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Select(standard => standard.Id).ToHashSet(StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);

        foreach (var country in catalog.Countries)
        {
            if (country.SchemaVersion != 1)
                issues.Add(new ValidationIssue("ERROR", "GOVDATA006", $"Unsupported schemaVersion {country.SchemaVersion}; version 1 is supported.", $"countries/{country.Slug}/country.json"));
            if (!CountryIdPattern().IsMatch(country.Id))
                issues.Add(new ValidationIssue("ERROR", "GOVDATA007", $"Country id must be a two- or three-letter uppercase code: {country.Id}"));
            if (!SlugPattern().IsMatch(country.Slug))
                issues.Add(new ValidationIssue("ERROR", "GOVDATA008", $"Country slug must be lowercase kebab-case: {country.Slug}"));
            if (string.IsNullOrWhiteSpace(country.DisplayName) || string.IsNullOrWhiteSpace(country.NativeName))
                issues.Add(new ValidationIssue("ERROR", "GOVDATA009", $"Country {country.Id} needs displayName and nativeName."));
            if (string.IsNullOrWhiteSpace(country.DefaultLocale) || !country.SupportedLocales.Contains(country.DefaultLocale, StringComparer.OrdinalIgnoreCase))
                issues.Add(new ValidationIssue("ERROR", "GOVDATA010", $"Country {country.Id} defaultLocale must be listed in supportedLocales."));
            if (country.HistoricalCoverage.StartYear is 0 || country.HistoricalCoverage.EndYear is 0 ||
                country.HistoricalCoverage.StartYear is int startYear && country.HistoricalCoverage.EndYear is int endYear && startYear > endYear)
                issues.Add(new ValidationIssue("ERROR", "GOVDATA011", $"Country {country.Id} has an invalid historical coverage range."));

            if (country.IncidentFiles.Count != country.IncidentFiles.Distinct(StringComparer.OrdinalIgnoreCase).Count())
                issues.Add(new ValidationIssue("ERROR", "GOVDATA012", $"Country {country.Id} lists an incident file more than once."));
        }

        var standardIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var standard in catalog.Standards)
        {
            if (!standardIds.Add($"{standard.Country}:{standard.Id}"))
                issues.Add(new ValidationIssue("ERROR", "GOVDATA013", $"Duplicate standard id {standard.Id} in {standard.Country}.", standard.FilePath));
        }

        foreach (var duplicate in catalog.Sources.GroupBy(source => source.Id, StringComparer.OrdinalIgnoreCase).Where(group => group.Count() > 1))
            issues.Add(new ValidationIssue("ERROR", "GOVDATA033", $"Duplicate source id: {duplicate.Key}"));
        foreach (var source in catalog.Sources)
        {
            if (!SourceIdPattern().IsMatch(source.Id))
                issues.Add(new ValidationIssue("ERROR", "GOVDATA034", $"Invalid source id: {source.Id}", $"countries/{source.CountryId}/sources.json"));
            if (string.IsNullOrWhiteSpace(source.Title) || !Uri.TryCreate(source.Url, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
                issues.Add(new ValidationIssue("ERROR", "GOVDATA024", $"Source {source.Id} needs a title and valid HTTPS URL: {source.Url}", $"countries/{source.CountryId}/sources.json"));
        }

        foreach (var incident in catalog.Incidents)
        {
            if (incident.SchemaVersion != 1)
                issues.Add(new ValidationIssue("ERROR", "GOVDATA030", $"Incident {incident.Id} uses unsupported schemaVersion {incident.SchemaVersion}; version 1 is supported.", incident.SourcePath));
            if (!IncidentIdPattern().IsMatch(incident.Id))
                issues.Add(new ValidationIssue("ERROR", "GOVDATA014", $"Incident id must use COUNTRY-YEAR-NNN format: {incident.Id}", incident.SourcePath));
            if (incident.CountryIds.Count == 0 || incident.CountryIds.Count != incident.CountryIds.Distinct(StringComparer.OrdinalIgnoreCase).Count())
                issues.Add(new ValidationIssue("ERROR", "GOVDATA035", $"Incident {incident.Id} needs unique countryIds.", incident.SourcePath));
            var idCountry = IncidentIdPattern().Match(incident.Id).Groups["country"].Value;
            if (!incident.CountryIds.Contains(idCountry, StringComparer.OrdinalIgnoreCase))
                issues.Add(new ValidationIssue("ERROR", "GOVDATA031", "Incident id country must appear in countryIds.", incident.SourcePath));
            if (incident.Historical.EvidenceStatus != "Documented" && string.IsNullOrWhiteSpace(incident.Historical.Uncertainty))
                issues.Add(new ValidationIssue("ERROR", "GOVDATA032", "Non-documented evidence needs an explanation of uncertainty or interpretation.", incident.SourcePath));
            foreach (var countryId in incident.CountryIds)
            {
                if (!countriesById.TryGetValue(countryId, out var country))
                {
                    issues.Add(new ValidationIssue("ERROR", "GOVDATA015", $"Incident {incident.Id} refers to unknown country {countryId}.", incident.SourcePath));
                    continue;
                }
            }

            var incidentStandards = incident.CountryIds
                .Where(countriesById.ContainsKey)
                .SelectMany(countryId => standardsByCountry.TryGetValue(countryId, out var standards) ? standards : [])
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var standardId in incident.Satire.Standards)
                if (!incidentStandards.Contains(standardId))
                    issues.Add(new ValidationIssue("ERROR", "GOVDATA017", $"Incident {incident.Id} references missing standard {standardId} for its countryIds.", incident.SourcePath));

            ValidateDate(incident, issues);
            var idYear = IncidentIdPattern().Match(incident.Id);
            if (idYear.Success && incident.Date.StartYear is int eventYear && eventYear > 0)
            {
                var encodedYear = int.Parse(idYear.Groups["year"].Value, System.Globalization.CultureInfo.InvariantCulture);
                var lastYear = incident.Date.EndYear ?? eventYear;
                if (encodedYear < eventYear || encodedYear > lastYear)
                    issues.Add(new ValidationIssue("ERROR", "GOVDATA016", $"Incident id year must fall within the event date range for {incident.Id}.", incident.SourcePath));
            }

            if (string.IsNullOrWhiteSpace(incident.Category) || string.IsNullOrWhiteSpace(incident.Title) || string.IsNullOrWhiteSpace(incident.Historical.Summary))
                issues.Add(new ValidationIssue("ERROR", "GOVDATA021", $"Incident {incident.Id} needs a title, category, and factual summary.", incident.SourcePath));
            if (!EvidenceStatuses.Contains(incident.Historical.EvidenceStatus))
                issues.Add(new ValidationIssue("ERROR", "GOVDATA022", $"Incident {incident.Id} has unsupported evidenceStatus '{incident.Historical.EvidenceStatus}'.", incident.SourcePath));
            if (incident.Historical.Sources.Count == 0)
                issues.Add(new ValidationIssue("ERROR", "GOVDATA023", $"Incident {incident.Id} has no historical source.", incident.SourcePath));
            if (incident.Historical.Sources.Count != incident.Historical.Sources.Distinct(StringComparer.OrdinalIgnoreCase).Count())
                issues.Add(new ValidationIssue("ERROR", "GOVDATA036", $"Incident {incident.Id} repeats a source id.", incident.SourcePath));
            foreach (var sourceId in incident.Historical.Sources)
            {
                var source = catalog.FindSource(sourceId);
                if (source is null)
                    issues.Add(new ValidationIssue("ERROR", "GOVDATA037", $"Incident {incident.Id} references unknown source {sourceId}.", incident.SourcePath));
                else if (!incident.CountryIds.Contains(source.CountryId, StringComparer.OrdinalIgnoreCase))
                    issues.Add(new ValidationIssue("ERROR", "GOVDATA038", $"Source {sourceId} belongs to {source.CountryId}, which is not listed in {incident.Id} countryIds.", incident.SourcePath));
            }

            if (string.IsNullOrWhiteSpace(incident.Satire.CommitMessage) || string.IsNullOrWhiteSpace(incident.Satire.DiagnosticMessage) ||
                !DiagnosticIdPattern().IsMatch(incident.Satire.DiagnosticId))
                issues.Add(new ValidationIssue("ERROR", "GOVDATA025", $"Incident {incident.Id} needs a simulated commit message and GOV diagnostic id/message.", incident.SourcePath));
            if (!DiagnosticSeverities.Contains(incident.Satire.Severity))
                issues.Add(new ValidationIssue("ERROR", "GOVDATA026", $"Incident {incident.Id} has unsupported diagnostic severity '{incident.Satire.Severity}'.", incident.SourcePath));

            foreach (var relation in incident.Relations)
            {
                if (string.Equals(relation.IncidentId, incident.Id, StringComparison.OrdinalIgnoreCase))
                    issues.Add(new ValidationIssue("ERROR", "GOVDATA027", $"Incident {incident.Id} cannot relate to itself.", incident.SourcePath));
                else if (!incidentsById.ContainsKey(relation.IncidentId))
                    issues.Add(new ValidationIssue("ERROR", "GOVDATA028", $"Incident {incident.Id} references missing related incident {relation.IncidentId}.", incident.SourcePath));
                if (string.IsNullOrWhiteSpace(relation.Type) || string.IsNullOrWhiteSpace(relation.Label))
                    issues.Add(new ValidationIssue("ERROR", "GOVDATA029", $"Incident {incident.Id} relation to {relation.IncidentId} needs a type and label.", incident.SourcePath));
            }
        }

        return issues.OrderBy(issue => issue.Severity, StringComparer.Ordinal).ThenBy(issue => issue.Code, StringComparer.Ordinal).ThenBy(issue => issue.FilePath, StringComparer.Ordinal).ToArray();
    }

    private static void ValidateDate(GovernmentIncident incident, List<ValidationIssue> issues)
    {
        var date = incident.Date;
        var invalid = false;
        if (!DatePrecisions.Contains(date.Precision))
        {
            issues.Add(new ValidationIssue("ERROR", "GOVDATA018", $"Incident {incident.Id} has unsupported date precision '{date.Precision}'.", incident.SourcePath));
            invalid = true;
        }
        if (string.IsNullOrWhiteSpace(date.Calendar) || string.IsNullOrWhiteSpace(date.DisplayText))
        {
            issues.Add(new ValidationIssue("ERROR", "GOVDATA019", $"Incident {incident.Id} date needs a calendar and displayText.", incident.SourcePath));
            invalid = true;
        }
        if (date.StartYear == 0 || date.EndYear == 0 || date.StartYear is < -9999 or > 9999 || date.EndYear is < -9999 or > 9999)
        {
            issues.Add(new ValidationIssue("ERROR", "GOVDATA020", $"Incident {incident.Id} has an invalid year; the calendar convention has no year zero.", incident.SourcePath));
            invalid = true;
        }
        if (date.StartYear is int start && date.EndYear is int end && end < start)
        {
            issues.Add(new ValidationIssue("ERROR", "GOVDATA021", $"Incident {incident.Id} date end precedes its start.", incident.SourcePath));
            invalid = true;
        }
        if (date.Month is < 1 or > 12 || date.Day is < 1 or > 31 || date.Day is not null && date.Month is null)
        {
            issues.Add(new ValidationIssue("ERROR", "GOVDATA022", $"Incident {incident.Id} has an invalid month/day combination.", incident.SourcePath));
            invalid = true;
        }
        if (date.Precision.Equals("day", StringComparison.OrdinalIgnoreCase) && (date.StartYear is null || date.Month is null || date.Day is null) ||
            date.Precision.Equals("month", StringComparison.OrdinalIgnoreCase) && (date.StartYear is null || date.Month is null || date.Day is not null) ||
            date.Precision.Equals("year", StringComparison.OrdinalIgnoreCase) && (date.StartYear is null || date.Month is not null || date.Day is not null || date.EndYear is not null) ||
            date.Precision.Equals("range", StringComparison.OrdinalIgnoreCase) && (date.StartYear is null || date.EndYear is null) ||
            date.Precision.Equals("unknown", StringComparison.OrdinalIgnoreCase) && (date.StartYear is not null || date.Month is not null || date.Day is not null || date.EndYear is not null))
        {
            issues.Add(new ValidationIssue("ERROR", "GOVDATA039", $"Incident {incident.Id} date fields do not match precision '{date.Precision}'.", incident.SourcePath));
            invalid = true;
        }
        if (!invalid && date.Precision.Equals("day", StringComparison.OrdinalIgnoreCase) && date.Calendar.Equals("gregorian", StringComparison.OrdinalIgnoreCase))
        {
            var absoluteYear = Math.Abs(date.StartYear!.Value);
            var leap = absoluteYear % 4 == 0 && (absoluteYear % 100 != 0 || absoluteYear % 400 == 0);
            var maximumDay = date.Month == 2 ? leap ? 29 : 28 : date.Month is 4 or 6 or 9 or 11 ? 30 : 31;
            if (date.Day > maximumDay)
                issues.Add(new ValidationIssue("ERROR", "GOVDATA040", $"Incident {incident.Id} has an impossible Gregorian date.", incident.SourcePath));
        }
    }

    [GeneratedRegex("^[A-Z]{2,3}$")]
    private static partial Regex CountryIdPattern();

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugPattern();

    [GeneratedRegex("^(?<country>[A-Z]{2,3})-(?<year>\\d{4})-\\d{3,}$")]
    private static partial Regex IncidentIdPattern();

    [GeneratedRegex("^SRC-[A-Z]{2,3}-[A-Za-z0-9-]+$")]
    private static partial Regex SourceIdPattern();

    [GeneratedRegex("^GOV\\d{3,4}$")]
    private static partial Regex DiagnosticIdPattern();
}
