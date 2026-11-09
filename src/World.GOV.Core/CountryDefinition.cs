namespace World.GOV.Core;

public sealed record CountryDefinition
{
    public int SchemaVersion { get; init; } = 1;
    public required string Id { get; init; }
    public required string Slug { get; init; }
    public required string DisplayName { get; init; }
    public required string NativeName { get; init; }
    public required string DefaultLocale { get; init; }
    public IReadOnlyList<string> SupportedLocales { get; init; } = [];
    public HistoricalCoverage HistoricalCoverage { get; init; } = new();
    public IReadOnlyList<string> IncidentFiles { get; init; } = [];
    public IReadOnlyList<string> StandardFiles { get; init; } = [];
    public string? ExtensionAssembly { get; init; }
}

public sealed record HistoricalCoverage
{
    public int? StartYear { get; init; }
    public int? EndYear { get; init; }
}
