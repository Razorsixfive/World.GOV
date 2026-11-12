using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using World.GOV.Core;

namespace World.GOV.Tooling;

public static partial class WebsiteTranslations
{
    public const string EnglishLocale = "en-US";

    internal static IReadOnlyList<WebsiteLocale> Load(string root, List<string> errors)
    {
        var locales = new List<WebsiteLocale>();
        var directory = Path.Combine(root, "locales");
        if (!Directory.Exists(directory))
        {
            errors.Add("Missing website translation directory: locales");
            return locales;
        }
        var schema = new ContentSchema(Path.Combine(root, "schemas", "website-locale-v1.schema.json"));
        foreach (var path in Directory.EnumerateFiles(directory, "*.json").Order(StringComparer.Ordinal))
        {
            try
            {
                var locale = schema.Read<WebsiteLocale>(path, new JsonSerializerOptions(JsonSerializerDefaults.Web));
                if (!string.Equals(Path.GetFileNameWithoutExtension(path), locale.Locale, StringComparison.Ordinal))
                    throw new JsonException("The filename must match the locale exactly.");
                locales.Add(locale);
            }
            catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException)
            {
                errors.Add($"Could not read locales/{Path.GetFileName(path)}: {exception.Message}");
            }
        }
        return locales;
    }

    public static IReadOnlyList<ValidationIssue> Validate(IReadOnlyList<WebsiteLocale> locales)
    {
        var issues = new List<ValidationIssue>();
        var english = locales.FirstOrDefault(locale => locale.Locale == EnglishLocale);
        if (english is null || english.Strings.Count == 0)
        {
            issues.Add(new("ERROR", "GOVI18N001", "The complete en-US website catalog is required.", "locales/en-US.json"));
            return issues;
        }
        foreach (var duplicate in locales.GroupBy(locale => locale.Locale, StringComparer.OrdinalIgnoreCase).Where(group => group.Count() > 1))
            issues.Add(new("ERROR", "GOVI18N002", $"Duplicate website locale: {duplicate.Key}"));

        foreach (var locale in locales)
        {
            foreach (var (key, value) in locale.Strings)
            {
                var path = $"locales/{locale.Locale}.json";
                if (!english.Strings.ContainsKey(key))
                {
                    issues.Add(new("WARNING", "GOVI18N003", $"Unused or misspelled English text; ignored until updated: {key}", path));
                    continue;
                }
                if (locale.Locale == EnglishLocale && key != value)
                    issues.Add(new("ERROR", "GOVI18N004", $"The English value must match its source text: {key}", path));
                if (string.IsNullOrWhiteSpace(value)) continue; // Empty translations deliberately fall back to English.
                try
                {
                    CompositeFormat.Parse(value);
                    CompositeFormat.Parse(key);
                    if (!Placeholders(key).SequenceEqual(Placeholders(value)))
                        throw new FormatException("Numbered placeholders must match the English text.");
                }
                catch (FormatException exception)
                {
                    issues.Add(new("ERROR", "GOVI18N005", $"Invalid translation of '{key}': {exception.Message}", path));
                }
            }
        }
        return issues;
    }

    public static string CreateTemplate(ContentCatalog catalog, string locale, string nativeName, string englishName, string direction = "ltr")
    {
        if (!LocalePattern().IsMatch(locale) || locale == EnglishLocale)
            throw new ArgumentException("Choose a language code such as de-DE, fr-FR, or zh-Hant-TW; en-US is the English baseline.");
        if (string.IsNullOrWhiteSpace(nativeName) || string.IsNullOrWhiteSpace(englishName) || direction is not ("ltr" or "rtl"))
            throw new ArgumentException("A native name, English name, and ltr or rtl direction are required.");
        var english = catalog.WebsiteLocales.Single(item => item.Locale == EnglishLocale);
        var template = new WebsiteLocale
        {
            Locale = locale, NativeName = nativeName, EnglishName = englishName, Direction = direction,
            Strings = english.Strings.Keys.Order(StringComparer.Ordinal).ToDictionary(key => key, _ => string.Empty)
        };
        var path = Path.Combine(catalog.RepositoryRoot, "locales", $"{locale}.json");
        using var file = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(file, template, new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true });
        return path;
    }

    private static IEnumerable<string> Placeholders(string value) => PlaceholderPattern().Matches(value)
        .Select(match => match.Groups[1].Value).Distinct().Order(StringComparer.Ordinal);

    [GeneratedRegex("(?<!\\{)\\{(\\d+)")]
    private static partial Regex PlaceholderPattern();

    [GeneratedRegex("^[a-z]{2,3}(?:-[A-Z][a-z]{3})?(?:-[A-Z]{2})?$")]
    private static partial Regex LocalePattern();
}
