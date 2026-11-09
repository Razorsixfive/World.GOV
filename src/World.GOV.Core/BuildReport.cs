namespace World.GOV.Core;

public sealed record BuildReport
{
    public required string BuildStatus { get; init; }
    public required string StatusSource { get; init; }
    public required DateTimeOffset GeneratedAt { get; init; }
    public string? Commit { get; init; }
    public string? Workflow { get; init; }
    public int RegisteredDiagnostics { get; init; }
    public string DiagnosticStatusLabel { get; init; } = "Satirical domain report; not a compiler result";
}
