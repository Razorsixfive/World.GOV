namespace World.GOV.Core;

public sealed record VirtualCommit
{
    public required string Hash { get; init; }
    public required string Country { get; init; }
    public required string IncidentId { get; init; }
    public required string Date { get; init; }
    public HistoricalDate HistoricalDate { get; init; } = new() { Precision = "year", Calendar = "gregorian", DisplayText = string.Empty };
    public required string Message { get; init; }
    public required string Label { get; init; }
    public string? SourcePath { get; init; }
    public string? IntroducedInGitCommit { get; init; }
}
