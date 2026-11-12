using System.Net;
using System.Text.Json.Nodes;
using World.GOV.Tooling;
using World.GOV.Web.Components;
using World.GOV.Web.Pages;
using Xunit;

namespace World.GOV.Tests;

public sealed partial class ContentPipelineTests
{
    private const string ArchiveIncidentFile = "countries/denmark/incidents/DK-1963-001.json";

    [Fact]
    public async Task ExistingIncidentDoesNotAcquireAnArchivePanelOrExportField()
    {
        var portal = await LoadPortalAsync();
        var html = await RenderPortalPageAsync<IncidentDetails>(portal,
            new Dictionary<string, object?> { [nameof(IncidentDetails.IncidentId)] = "DK-1849-001" });
        Assert.Contains("HISTORICAL FACTS", html);
        Assert.DoesNotContain("archive-dossier", html);
        var output = StaticSiteExporter.Export(Load());
        var countryData = File.ReadAllText(Path.Combine(output, "incidents", "DK.json"));
        var incidents = JsonNode.Parse(countryData)!.AsArray();
        var existing = incidents.Single(item => item!["id"]!.GetValue<string>() == "DK-1849-001")!;
        Assert.False(existing["satire"]!.AsObject().ContainsKey("archive"));
    }

    [Fact]
    public async Task ArchiveStartsRedactedAndCanDisplayTheFullMemo()
    {
        var portal = await LoadPortalAsync();
        var archive = portal.FindIncident("DK-1963-001")!.Satire.Archive!;
        var parameters = new Dictionary<string, object?> { [nameof(ArchiveDossier.Document)] = archive };
        var redacted = WebUtility.HtmlDecode(await RenderPortalPageAsync<ArchiveDossier>(portal, parameters));
        Assert.Contains("ARCHIVE COPY · FICTION", redacted);
        Assert.Contains(">CLASSIFIED</span>", redacted);
        Assert.Contains(archive.ClassifiedMessage!, redacted);
        Assert.Contains("aria-label=\"Redacted text\"", redacted);
        foreach (var line in archive.Lines)
        {
            Assert.Contains(line.Label, redacted);
            if (line.Redacted) Assert.DoesNotContain(line.Text, redacted);
            else Assert.Contains(line.Text, redacted);
        }

        parameters[nameof(ArchiveDossier.Unredacted)] = true;
        var revealed = WebUtility.HtmlDecode(await RenderPortalPageAsync<ArchiveDossier>(portal, parameters));
        Assert.Contains(">DECLASSIFIED</span>", revealed);
        Assert.Contains(archive.DeclassifiedMessage!, revealed);
        Assert.DoesNotContain(archive.ClassifiedMessage!, revealed);
        Assert.DoesNotContain("class=\"archive-redaction\"", revealed);
        Assert.All(archive.Lines, line => Assert.Contains(line.Text, revealed));
        Assert.Contains(archive.Lines, line => line.Redacted);
    }

    [Fact]
    public async Task DanishArchiveUsesTranslatedPublicTextAndEnglishDeveloperOutput()
    {
        var portal = await LoadPortalAsync();
        var incident = portal.FindIncident("DK-1963-001")!;
        var html = WebUtility.HtmlDecode(await RenderPortalPageAsync<IncidentDetails>(portal,
            new Dictionary<string, object?> { [nameof(IncidentDetails.IncidentId)] = incident.Id }, "da-DK"));
        Assert.Contains("ARKIVKOPI · FIKTION", html);
        Assert.Contains("Fuld tekst", html);
        Assert.Contains(incident.Translations["da-DK"].Archive!.ClassifiedMessage!, html);
        Assert.Contains(incident.Translations["da-DK"].Archive!.Title, html);
        Assert.Contains("class=\"archive-lines\" lang=\"da-DK\"", html);
        Assert.Contains("Kilder og dokumentation", html);
        Assert.DoesNotContain("Noget indhold findes ikke", html);

        var developer = WebUtility.HtmlDecode(await RenderPortalPageAsync<DeveloperImplementation>(portal,
            new Dictionary<string, object?> { [nameof(DeveloperImplementation.Incident)] = incident }, "da-DK"));
        Assert.Contains(incident.Satire.CodeExample!, developer);
        Assert.Contains(incident.Satire.DiagnosticMessage, developer);
        Assert.Contains("SATIRICAL IMPLEMENTATION · FICTION", developer);
    }

    [Fact]
    public async Task UntranslatedArchiveFallsBackToEnglishWithoutHidingTheHistoricalRecord()
    {
        Change(ArchiveIncidentFile, json => json["translations"]!["da-DK"]!.AsObject().Remove("archive"));
        var portal = await LoadPortalAsync();
        var incident = portal.FindIncident("DK-1963-001")!;
        var html = WebUtility.HtmlDecode(await RenderPortalPageAsync<IncidentDetails>(portal,
            new Dictionary<string, object?> { [nameof(IncidentDetails.IncidentId)] = incident.Id }, "da-DK"));
        Assert.Contains("Noget indhold findes ikke", html);
        Assert.Contains("class=\"archive-lines\" lang=\"en-US\"", html);
        Assert.Contains(incident.Translations["da-DK"].HistoricalSummary!, html);
        Assert.Contains(portal.FindSource("SRC-DK-023")!.Url, html);
        Assert.Contains(incident.Satire.Archive!.Title, html);
    }

    [Theory]
    [InlineData("classifiedMessage", false)]
    [InlineData("declassifiedMessage", true)]
    public async Task MissingArchiveStatusTranslationUsesCanonicalEnglish(string field, bool unredacted)
    {
        Change(ArchiveIncidentFile, json => json["translations"]!["da-DK"]!["archive"]!.AsObject().Remove(field));
        var portal = await LoadPortalAsync();
        var incident = portal.FindIncident("DK-1963-001")!;
        var canonical = incident.Satire.Archive!;
        var translated = incident.Translations["da-DK"].Archive!;
        var html = WebUtility.HtmlDecode(await RenderPortalPageAsync<ArchiveDossier>(portal,
            new Dictionary<string, object?>
            {
                [nameof(ArchiveDossier.Document)] = translated,
                [nameof(ArchiveDossier.EnglishDocument)] = canonical,
                [nameof(ArchiveDossier.DocumentLocale)] = "da-DK",
                [nameof(ArchiveDossier.Unredacted)] = unredacted
            }, "da-DK"));
        Assert.Contains(unredacted ? canonical.DeclassifiedMessage! : canonical.ClassifiedMessage!, html);
        Assert.Contains("class=\"archive-status\" role=\"status\" lang=\"en-US\"", html);
        Assert.Contains(translated.Title, html);

        var page = WebUtility.HtmlDecode(await RenderPortalPageAsync<IncidentDetails>(portal,
            new Dictionary<string, object?> { [nameof(IncidentDetails.IncidentId)] = incident.Id }, "da-DK"));
        Assert.Contains("Noget indhold findes ikke", page);
    }

    [Theory]
    [InlineData("id")]
    [InlineData("lines")]
    [InlineData("redaction")]
    public void ArchiveTranslationsPreserveTheCanonicalStructure(string change)
    {
        Change(ArchiveIncidentFile, json =>
        {
            var archive = json["translations"]!["da-DK"]!["archive"]!;
            if (change == "id") archive["id"] = "SIM-ARCHIVE-DIFFERENT";
            else if (change == "lines") archive["lines"]!.AsArray().RemoveAt(0);
            else archive["lines"]![0]!["redacted"] = true;
        });
        Assert.Contains(ContentValidator.Validate(Load()), issue => issue.Code == "GOVDATA041");
        Assert.Throws<InvalidOperationException>(() => StaticSiteExporter.Export(Load()));
    }

    [Theory]
    [InlineData("identifier")]
    [InlineData("empty")]
    [InlineData("null")]
    public void MalformedArchivesAreRejectedByTheSchema(string change)
    {
        Change(ArchiveIncidentFile, json =>
        {
            var archive = json["satire"]!["archive"]!;
            if (change == "identifier") archive["id"] = "ACTUAL-SECRET";
            else if (change == "empty") archive["lines"] = new JsonArray();
            else archive["lines"]![0]!["text"] = null;
        });
        Assert.Contains(ContentValidator.Validate(Load()), issue => issue.Code == "GOVDATA001");
    }
}
