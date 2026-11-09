namespace World.GOV.Core;

public sealed record IncidentRelation
{
    public required string IncidentId { get; init; }
    public required string Type { get; init; }
    public required string Label { get; init; }
}
