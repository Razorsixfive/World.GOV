using System.Text.Json.Serialization;

namespace World.GOV.Core;

public sealed record ArchiveDocument
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ClassifiedMessage { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DeclassifiedMessage { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ReclassifiedMessage { get; init; }
    public IReadOnlyList<ArchiveLine> Lines { get; init; } = [];
}

public sealed record ArchiveLine
{
    public required string Label { get; init; }
    public required string Text { get; init; }
    public bool Redacted { get; init; }
}
