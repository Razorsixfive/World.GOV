namespace World.GOV.Core;

public sealed record PublishManifest(
    int SchemaVersion,
    DateTimeOffset GeneratedAt,
    IReadOnlyList<string> Countries,
    IReadOnlyList<string> Datasets,
    string? RepositoryUrl = null,
    string? SourceBranch = null);

public sealed record StandardIndexEntry(
    string Id,
    string Country,
    string Title,
    string ContentPath);
