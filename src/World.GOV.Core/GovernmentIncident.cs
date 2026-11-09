using System.Text.Json.Serialization;

namespace World.GOV.Core;

public sealed record GovernmentIncident
{
    public int SchemaVersion { get; init; } = 1;
    public required string Id { get; init; }
    public IReadOnlyList<string> CountryIds { get; init; } = [];
    public string? GlobalEventId { get; init; }
    public IReadOnlyList<string> Regions { get; init; } = [];
    public required HistoricalDate Date { get; init; }
    public required string Category { get; init; }
    public required string Title { get; init; }
    public required HistoricalContext Historical { get; init; }
    public required SatiricalContext Satire { get; init; }
    public IReadOnlyDictionary<string, IncidentTranslation> Translations { get; init; } = new Dictionary<string, IncidentTranslation>();
    public IReadOnlyList<IncidentRelation> Relations { get; init; } = [];
    public IReadOnlyList<string> Tags { get; init; } = [];
    public string? SourcePath { get; init; }
    public string? IntroducedInGitCommit { get; init; }

    [JsonIgnore]
    public string Country => CountryIds.FirstOrDefault() ?? string.Empty;
}

public sealed record IncidentTranslation
{
    public string? Title { get; init; }
    public string? Category { get; init; }
    public string? EvidenceStatus { get; init; }
    public string? HistoricalSummary { get; init; }
    public string? HistoricalUncertainty { get; init; }
    public string? PublicSummary { get; init; }
    public string? CommitMessage { get; init; }
    public string? DiagnosticMessage { get; init; }
    public IReadOnlyDictionary<string, string> RelationLabels { get; init; } = new Dictionary<string, string>();
}
