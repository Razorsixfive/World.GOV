namespace World.GOV.Core;

public sealed record GovernmentDiagnostic
{
    public required string Id { get; init; }
    public required string Country { get; init; }
    public required string Severity { get; init; }
    public required string Message { get; init; }
    public required string IncidentId { get; init; }
    public string? StandardId { get; init; }
    public string? SourceLocation { get; init; }
    public bool Acknowledged { get; init; }
    public bool IgnoredByPolicy { get; init; }
}
