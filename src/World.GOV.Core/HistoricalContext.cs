namespace World.GOV.Core;

public sealed record HistoricalContext
{
    public required string Summary { get; init; }
    public required string EvidenceStatus { get; init; }
    public string? Uncertainty { get; init; }
    public IReadOnlyList<string> Sources { get; init; } = [];
}
