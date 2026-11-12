using System.Text.Json.Serialization;

namespace World.GOV.Core;

public sealed record SatiricalContext
{
    public string? PublicSummary { get; init; }
    public string? CodeExample { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ArchiveDocument? Archive { get; init; }
    public required string CommitMessage { get; init; }
    public required string DiagnosticId { get; init; }
    public required string DiagnosticMessage { get; init; }
    public string Severity { get; init; } = "CRITICAL";
    public IReadOnlyList<string> Standards { get; init; } = [];
    public bool Acknowledged { get; init; }
    public bool IgnoredByPolicy { get; init; } = true;
}
