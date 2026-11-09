using System.Text.Json;
using System.Text.Json.Nodes;
using World.GOV.Cli;
using World.GOV.Core;
using World.GOV.Tooling;
using Xunit;

namespace World.GOV.Tests;

public sealed class ContentPipelineTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "worldgov-tests-" + Guid.NewGuid().ToString("N"));
    private const string IncidentFile = "countries/denmark/incidents/DK-1849-001.json";

    public ContentPipelineTests()
    {
        var source = ContentCatalog.FindRepositoryRoot();
        foreach (var directory in new[] { "countries", "schemas" })
            foreach (var file in Directory.EnumerateFiles(Path.Combine(source, directory), "*", SearchOption.AllDirectories))
            {
                var target = Path.Combine(_root, Path.GetRelativePath(source, file));
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                File.Copy(file, target);
            }
    }

    private ContentCatalog Load() => ContentCatalog.Load(_root);
    private void Change(string path, Action<JsonObject> mutate)
    {
        var fullPath = Path.Combine(_root, path);
        var json = JsonNode.Parse(File.ReadAllText(fullPath))!.AsObject();
        mutate(json);
        File.WriteAllText(fullPath, json.ToJsonString());
    }

    [Fact]
    public void PublishedContentAndDataOnlyCountryValidate()
    {
        var catalog = Load();
        Assert.Empty(ContentValidator.Validate(catalog));
        Assert.NotEmpty(catalog.GetByCountry("DE"));
        Assert.Contains(catalog.GetBlame("DK-1973-001"), item => item.Country == "DE");
    }

    [Theory]
    [InlineData("historical")]
    [InlineData("satire")]
    [InlineData("relations")]
    [InlineData("id")]
    public void NullRequiredShapesAreReportedWithoutCrashing(string property)
    {
        Change(IncidentFile, json => json[property] = null);
        Assert.Contains(ContentValidator.Validate(Load()), issue => issue.Code == "GOVDATA001");
    }

    [Fact]
    public void UnknownJsonPropertiesAreRejected()
    {
        Change(IncidentFile, json => json["typo"] = true);
        Assert.NotEmpty(Load().LoadErrors);
    }

    [Fact]
    public void MissingSourcesPreventExport()
    {
        Change(IncidentFile, json => json["historical"]!["sources"] = new JsonArray());
        Assert.Throws<InvalidOperationException>(() => StaticSiteExporter.Export(Load()));
    }

    [Fact]
    public void DuplicateIdsFailValidation()
    {
        File.Copy(Path.Combine(_root, IncidentFile), Path.Combine(_root, "countries/denmark/incidents/duplicate.json"));
        Change("countries/denmark/country.json", json => json["incidentFiles"]!.AsArray().Add("incidents/duplicate.json"));
        Assert.Contains(ContentValidator.Validate(Load()), issue => issue.Code == "GOVDATA005");
    }

    [Theory]
    [InlineData(30, 2)]
    [InlineData(1, 13)]
    public void InvalidHistoricalDatesAreRejected(int day, int month)
    {
        Change(IncidentFile, json => {
            json["date"]!["day"] = day;
            json["date"]!["month"] = month;
        });
        Assert.NotEmpty(ContentValidator.Validate(Load()));
    }

    [Fact]
    public void UnknownReferencesFailValidation()
    {
        Change(IncidentFile, json => {
            json["relations"]![0]!["incidentId"] = "DK-1900-999";
            json["satire"]!["standards"] = new JsonArray("GOV-404");
        });
        var issues = ContentValidator.Validate(Load());
        Assert.Contains(issues, issue => issue.Code == "GOVDATA028");
        Assert.Contains(issues, issue => issue.Code == "GOVDATA017");
    }

    [Theory]
    [InlineData("../../README.md")]
    [InlineData("..\\..\\README.md")]
    public void ManifestCannotEscapeCountryDirectory(string path)
    {
        Change("countries/denmark/country.json", json => json["incidentFiles"] = new JsonArray(path));
        Assert.NotEmpty(Load().LoadErrors);
    }

    [Fact]
    public void IncidentMustBelongToItsManifest()
    {
        Change(IncidentFile, json => json["countryIds"] = new JsonArray("DE"));
        Assert.NotEmpty(Load().LoadErrors);
    }

    [Fact]
    public void UncertainYearKeepsPrecisionAndRequiresExplanation()
    {
        Change(IncidentFile, json => {
            json["date"]!["month"] = null;
            json["date"]!["day"] = null;
            json["date"]!["precision"] = "year";
            json["date"]!["displayText"] = "1849";
            json["historical"]!["evidenceStatus"] = "Uncertain";
        });
        Assert.Contains(ContentValidator.Validate(Load()), issue => issue.Code == "GOVDATA032");
        Change(IncidentFile, json => json["historical"]!["uncertainty"] = "Fixture: only the year is known.");
        Assert.Empty(ContentValidator.Validate(Load()));
        Assert.Equal(1849, Load().GetHistory("DK").Single(item => item.IncidentId == "DK-1849-001").HistoricalDate.StartYear);
    }

    [Fact]
    public void DatePrecisionAndSourceRegistryArePublishedSeparately()
    {
        var catalog = Load();
        var jelling = catalog.FindById("DK-0965-001")!;
        Assert.Equal("approximate", jelling.Date.Precision);
        Assert.Equal(960, jelling.Date.StartYear);
        Assert.Equal(985, jelling.Date.EndYear);
        Assert.All(jelling.Historical.Sources, sourceId => Assert.NotNull(catalog.FindSource(sourceId)));

        var output = StaticSiteExporter.Export(catalog);
        using var sources = JsonDocument.Parse(File.ReadAllText(Path.Combine(output, "sources", "index.json")));
        Assert.True(sources.RootElement.GetArrayLength() > 0);
        using var incidents = JsonDocument.Parse(File.ReadAllText(Path.Combine(output, "incidents", "DK.json")));
        var sourceIds = incidents.RootElement.EnumerateArray().First(item => item.GetProperty("id").GetString() == "DK-0965-001")
            .GetProperty("historical").GetProperty("sources");
        Assert.Equal(JsonValueKind.String, sourceIds[0].ValueKind);
    }

    [Fact]
    public void UnknownSourceReferencesFailValidation()
    {
        Change(IncidentFile, json => json["historical"]!["sources"]![0] = "SRC-DK-MISSING");
        Assert.Contains(ContentValidator.Validate(Load()), issue => issue.Code == "GOVDATA037");
    }

    [Fact]
    public void MultiCountryIncidentAppearsInEveryListedCountry()
    {
        Change(IncidentFile, json => json["countryIds"] = new JsonArray("DK", "DE"));
        var catalog = Load();
        Assert.Contains(catalog.GetByCountry("DE"), incident => incident.Id == "DK-1849-001");
        Assert.Empty(ContentValidator.Validate(catalog));

        var output = StaticSiteExporter.Export(catalog);
        using var incidents = JsonDocument.Parse(File.ReadAllText(Path.Combine(output, "incidents", "DE.json")));
        Assert.Contains(incidents.RootElement.EnumerateArray(), item => item.GetProperty("id").GetString() == "DK-1849-001");
        using var diagnostics = JsonDocument.Parse(File.ReadAllText(Path.Combine(output, "diagnostics", "DE.json")));
        Assert.Contains(diagnostics.RootElement.EnumerateArray(), item => item.GetProperty("incidentId").GetString() == "DK-1849-001");
    }

    [Fact]
    public void VirtualHistoryIsStableAndExplicitlyFictional()
    {
        var catalog = Load();
        var history = catalog.GetHistory("DK");
        Assert.Equal(history.OrderBy(item => item.HistoricalDate), history);
        Assert.All(history, item => Assert.StartsWith("SIM-", item.Hash));
        Assert.Equal(history[0].Hash, VirtualHistoryBuilder.Create(catalog.FindById(history[0].IncidentId)!).Hash);
        Assert.Contains("not an authentic Git commit", history[0].Label);
    }

    [Fact]
    public void GovernmentFailuresDoNotFabricateBuildSuccess()
    {
        var output = StaticSiteExporter.Export(Load());
        using var report = JsonDocument.Parse(File.ReadAllText(Path.Combine(output, "build-report.json")));
        Assert.Equal("Not reported", report.RootElement.GetProperty("buildStatus").GetString());
        Assert.True(report.RootElement.GetProperty("registeredDiagnostics").GetInt32() > 0);
        Assert.True(File.Exists(Path.Combine(output, "incidents", "DE.json")));
        Assert.True(File.Exists(Path.Combine(output, "standards", "docs", "DK-GOV-014.md")));
    }

    [Fact]
    public void ExportCannotEscapeRepository() =>
        Assert.Throws<InvalidOperationException>(() => StaticSiteExporter.Export(Load(), "../worldgov-outside"));

    [Fact]
    public async Task InvalidContentReturnsFailingCliExitCode()
    {
        Change(IncidentFile, json => json.Remove("historical"));
        Assert.Equal(1, await WorldGovCommand.RunAsync(["validate", "--root", _root]));
        Assert.Equal(1, await WorldGovCommand.RunAsync(["export", "--root", _root]));
    }

    [Fact]
    public void ReferenceModuleUsesSameIncidentRecords()
    {
        var module = new Denmark.NET.DenmarkCountryModule();
        Assert.Equal(Load().GetByCountry("DK").Select(item => item.Id).Order(), module.GetIncidents().Select(item => item.Id).Order());
    }

    public void Dispose()
    {
        if (Path.GetFullPath(_root).StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase))
            Directory.Delete(_root, true);
    }
}
