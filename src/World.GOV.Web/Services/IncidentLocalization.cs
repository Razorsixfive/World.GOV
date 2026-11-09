using World.GOV.Core;

namespace World.GOV.Web.Services;

public static class IncidentLocalization
{
    public static IncidentTranslation? ForLocale(GovernmentIncident? incident, string? locale)
    {
        if (incident is null || string.IsNullOrWhiteSpace(locale)) return null;
        return incident.Translations.FirstOrDefault(item =>
            string.Equals(item.Key, locale, StringComparison.OrdinalIgnoreCase)).Value;
    }

    public static bool HasTranslation(GovernmentIncident? incident, string? locale) => ForLocale(incident, locale) is not null;

    public static string Title(GovernmentIncident incident, string? locale) =>
        ForLocale(incident, locale)?.Title ?? incident.Title;

    public static string Category(GovernmentIncident incident, string? locale) =>
        ForLocale(incident, locale)?.Category ?? CategoryLabel(incident.Category, locale);

    public static string CategoryLabel(string category, string? locale)
    {
        if (locale?.StartsWith("da", StringComparison.OrdinalIgnoreCase) != true) return category;
        return category switch
        {
            "Political" => "Politisk",
            "Economic" => "Økonomisk",
            "NaturalDisaster" => "Naturkatastrofe",
            "War" => "Krig",
            "PublicAdministration" => "Offentlig forvaltning",
            "Technology" => "Teknologi",
            "Infrastructure" => "Infrastruktur",
            "Social" => "Samfund",
            "International" => "Internationalt",
            _ => "Andet"
        };
    }

    public static string EvidenceStatus(GovernmentIncident incident, string? locale) =>
        ForLocale(incident, locale)?.EvidenceStatus ?? EvidenceStatusLabel(incident.Historical.EvidenceStatus, locale);

    public static string EvidenceStatusLabel(string evidenceStatus, string? locale) =>
        locale?.StartsWith("da", StringComparison.OrdinalIgnoreCase) == true
            ? evidenceStatus switch
            {
                "Documented" => "Dokumenteret",
                "Interpretive" => "Fortolkende",
                "Uncertain" => "Usikkert",
                "Disputed" => "Omstridt",
                _ => evidenceStatus
            }
            : evidenceStatus;

    public static string Summary(GovernmentIncident incident, string? locale) =>
        ForLocale(incident, locale)?.HistoricalSummary ?? incident.Historical.Summary;

    public static string PublicSummary(GovernmentIncident incident, string? locale) =>
        ForLocale(incident, locale)?.PublicSummary ?? incident.Satire.PublicSummary ?? incident.Satire.DiagnosticMessage;

    public static string CommitMessage(GovernmentIncident incident, string? locale) =>
        ForLocale(incident, locale)?.CommitMessage ?? incident.Satire.CommitMessage;

    public static string DiagnosticMessage(GovernmentIncident incident, string? locale) =>
        ForLocale(incident, locale)?.DiagnosticMessage ?? incident.Satire.DiagnosticMessage;

    public static string RelationLabel(GovernmentIncident incident, IncidentRelation relation, string? locale)
    {
        var translation = ForLocale(incident, locale);
        return translation is not null && translation.RelationLabels.TryGetValue(relation.IncidentId, out var label)
            ? label
            : relation.Label;
    }
}
