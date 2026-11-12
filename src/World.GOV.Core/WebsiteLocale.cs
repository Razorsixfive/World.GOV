namespace World.GOV.Core;

public sealed record WebsiteLocale
{
    public int SchemaVersion { get; init; } = 1;
    public required string Locale { get; init; }
    public required string NativeName { get; init; }
    public required string EnglishName { get; init; }
    public string Direction { get; init; } = "ltr";
    public IReadOnlyDictionary<string, string> Strings { get; init; } = new Dictionary<string, string>();
}
