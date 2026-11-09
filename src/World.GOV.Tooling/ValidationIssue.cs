namespace World.GOV.Tooling;

public sealed record ValidationIssue(
    string Severity,
    string Code,
    string Message,
    string? FilePath = null);

public sealed record StandardDocument(
    string Country,
    string Id,
    string Title,
    string FilePath,
    string Content);

