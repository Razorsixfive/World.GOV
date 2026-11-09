using System.Text.Json.Serialization;

namespace World.GOV.Core;

public sealed record SourceReference
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Url { get; init; }
    public string? Publisher { get; init; }
    public string? Language { get; init; }
    public string? PublishedDate { get; init; }
    public string? AccessedDate { get; init; }
    public string? Notes { get; init; }
    [JsonIgnore]
    public string CountryId { get; init; } = string.Empty;
}

public sealed record SourceRegistryFile
{
    public int SchemaVersion { get; init; } = 1;
    public IReadOnlyList<SourceReference> Sources { get; init; } = [];
}

public sealed record PublishedSource
{
    public required string Id { get; init; }
    public required string Country { get; init; }
    public required string Title { get; init; }
    public required string Url { get; init; }
    public string? Publisher { get; init; }
    public string? Language { get; init; }
    public string? PublishedDate { get; init; }
    public string? AccessedDate { get; init; }
    public string? Notes { get; init; }
    public IReadOnlyList<string> IncidentIds { get; init; } = [];
}
