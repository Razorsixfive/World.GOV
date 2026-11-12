using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using World.GOV.Core;
using World.GOV.Tooling;
using World.GOV.Web.Components;
using World.GOV.Web.Pages;
using World.GOV.Web.Services;
using Xunit;

namespace World.GOV.Tests;

public sealed partial class ContentPipelineTests
{
    [Fact]
    public void EnglishCatalogCoversEveryLiteralWebsiteTranslation()
    {
        var english = Load().WebsiteLocales.Single(locale => locale.Locale == "en-US");
        var webRoot = Path.Combine(ContentCatalog.FindRepositoryRoot(), "src", "World.GOV.Web");
        foreach (var path in Directory.EnumerateFiles(webRoot, "*.razor", SearchOption.AllDirectories)
            .Where(path => !Path.GetRelativePath(webRoot, path).Split(Path.DirectorySeparatorChar).Any(part => part is "bin" or "obj")))
        {
            foreach (Match match in Regex.Matches(File.ReadAllText(path), "\\bT\\(\\s*(\"(?:[^\"\\\\]|\\\\.)*\")"))
            {
                var text = JsonSerializer.Deserialize<string>(match.Groups[1].Value)!;
                Assert.True(english.Strings.ContainsKey(text), $"English text is missing from locales/en-US.json: {text} ({path})");
            }
        }
        Assert.DoesNotContain("Developer view", english.Strings.Keys);
        Assert.DoesNotContain("SATIRICAL IMPLEMENTATION · FICTION", english.Strings.Keys);
        Assert.All(english.Strings, item => Assert.Equal(item.Key, item.Value));
    }

    [Theory]
    [InlineData("Page {0} of {1}", "Side {0}", "GOVI18N005")]
    [InlineData("Page {0} of {1}", "Side {0", "GOVI18N005")]
    public void TranslationValidationRejectsBrokenPlaceholders(string key, string value, string code)
    {
        Change("locales/da-DK.json", json => json["strings"]![key] = value);
        Assert.Contains(ContentValidator.Validate(Load()), issue => issue.Code == code);
        Assert.Throws<InvalidOperationException>(() => StaticSiteExporter.Export(Load()));
    }

    [Fact]
    public async Task StaleCommunityTextDoesNotBlockEnglishPublicationOrOverrideProtectedText()
    {
        Change("locales/da-DK.json", json =>
        {
            json["strings"]!["Developer view"] = "LOCALIZED DEVELOPER";
            json["strings"]!["Old English label"] = "Outdated translation";
        });
        var issues = ContentValidator.Validate(Load());
        Assert.Contains(issues, issue => issue.Code == "GOVI18N003" && issue.Severity == "WARNING");
        Assert.DoesNotContain(issues, issue => issue.Severity == "ERROR");
        var portal = await LoadPortalAsync();
        Assert.Equal("Developer view", portal.Translate("Developer view", "da-DK"));
        Assert.Equal("Old English label", portal.Translate("Old English label", "da-DK"));
        var danish = portal.WebsiteLocales.Single(locale => locale.Locale == "da-DK");
        Assert.Equal(portal.WebsiteLocales.Single(locale => locale.Locale == "en-US").Strings.Count, portal.TranslatedStringCount(danish));
    }

    [Fact]
    public void EnglishWebsiteSourceCannotBeOverriddenByATranslatedValue()
    {
        Change("locales/en-US.json", json => json["strings"]!["Overview"] = "Replacement");
        Assert.Contains(ContentValidator.Validate(Load()), issue => issue.Code == "GOVI18N004");
    }

    [Fact]
    public async Task NewLanguageNeedsNoCountryOrCodeChangesAndAllowsPartialTranslations()
    {
        WebsiteTranslations.CreateTemplate(Load(), "fr-FR", "Français", "French");
        var draft = await LoadPortalAsync();
        Assert.DoesNotContain("fr-FR", draft.AvailableLocales);
        Change("locales/fr-FR.json", json => json["strings"]!["Overview"] = "Vue d'ensemble");
        Assert.Empty(ContentValidator.Validate(Load()));
        var portal = await LoadPortalAsync();
        Assert.Contains("fr-FR", portal.AvailableLocales);
        Assert.Equal("Vue d'ensemble", portal.Translate("Overview", "fr-FR"));
        Assert.Equal("Countries", portal.Translate("Countries", "fr-FR"));
        Assert.Equal("Overview", portal.Translate("Overview", "en-US"));
        Assert.Equal("Overview", portal.Translate("Overview", "unknown"));
        Assert.Equal("Page 2 of 4", portal.Translate("Page {0} of {1}", "fr-FR", 2, 4));
        Assert.Equal("Français", portal.LanguageName("fr-FR", native: true));
    }

    [Fact]
    public void TranslationTemplatesCannotOverwriteFilesOrEscapeTheLocaleDirectory()
    {
        Assert.Throws<IOException>(() => WebsiteTranslations.CreateTemplate(Load(), "da-DK", "Dansk", "Danish"));
        Assert.Throws<ArgumentException>(() => WebsiteTranslations.CreateTemplate(Load(), "../outside", "Fixture", "Fixture"));
        Assert.Throws<ArgumentException>(() => WebsiteTranslations.CreateTemplate(Load(), "en-US", "Fixture", "Fixture"));
    }

    [Theory]
    [InlineData("zh-Hant-TW", "ltr")]
    [InlineData("ar", "rtl")]
    public async Task ScriptAndRightToLeftLocalesWorkForWebsiteAndIncidentTranslations(string locale, string direction)
    {
        WebsiteTranslations.CreateTemplate(Load(), locale, "Fixture language", "Fixture language", direction);
        Change($"locales/{locale}.json", json => json["strings"]!["Overview"] = "Fixture overview");
        Change(IncidentFile, json => json["translations"]![locale] = new JsonObject { ["title"] = "Fixture title" });
        Assert.Empty(ContentValidator.Validate(Load()));
        var portal = await LoadPortalAsync();
        Assert.Contains(locale, portal.AvailableLocales);
        Assert.Equal(direction, portal.TextDirection(locale));
        Assert.Equal("Fixture title", IncidentLocalization.Title(portal.FindIncident("DK-1849-001")!, locale));
    }

    [Fact]
    public async Task DanishPublicInterfaceWorksWhenAnIncidentFallsBackToEnglish()
    {
        Change("countries/germany/incidents/DE-1990-001.json", json => json.Remove("translations"));
        var portal = await LoadPortalAsync();
        var html = WebUtility.HtmlDecode(await RenderPortalPageAsync<IncidentDetails>(portal,
            new Dictionary<string, object?> { [nameof(IncidentDetails.IncidentId)] = "DE-1990-001" }, locale: "da-DK"));
        Assert.Contains("Hvad skete der", html);
        Assert.Contains("German Reunification", html);
        Assert.Contains("manglende tekst vises på engelsk", html);
        Assert.Contains("Developer view", html);
        Assert.DoesNotContain("Udviklervisning", html);
    }

    [Fact]
    public async Task DeveloperOutputAndEnglishFactsIgnoreLegacyTechnicalTranslations()
    {
        var incident = Load().FindById("DK-1849-001")!;
        var translated = new IncidentTranslation
        {
            Title = "LOCALIZED TITLE", CommitMessage = "LOCALIZED COMMIT", DiagnosticMessage = "LOCALIZED DIAGNOSTIC"
        };
        incident = incident with { Translations = new Dictionary<string, IncidentTranslation> { ["da-DK"] = translated, ["en-US"] = translated } };
        Assert.Equal(incident.Title, IncidentLocalization.Title(incident, "en-US"));
        Assert.Equal(incident.Satire.CommitMessage, IncidentLocalization.CommitMessage(incident, "da-DK"));
        Assert.Equal(incident.Satire.DiagnosticMessage, IncidentLocalization.DiagnosticMessage(incident, "da-DK"));
        var html = WebUtility.HtmlDecode(await RenderPortalPageAsync<DeveloperImplementation>(await LoadPortalAsync(),
            new Dictionary<string, object?> { [nameof(DeveloperImplementation.Incident)] = incident }, locale: "da-DK"));
        Assert.Contains("SATIRICAL IMPLEMENTATION · FICTION", html);
        Assert.Contains("Developer view", html);
        Assert.Contains(incident.Satire.CommitMessage, html);
        Assert.Contains(incident.Satire.DiagnosticMessage, html);
        Assert.Contains("lang=\"en\" dir=\"ltr\"", html);
        Assert.DoesNotContain("LOCALIZED", html);
    }
}
