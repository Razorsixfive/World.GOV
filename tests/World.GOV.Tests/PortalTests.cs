using System.Net;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using World.GOV.Tooling;
using World.GOV.Web.Pages;
using World.GOV.Web.Services;
using Xunit;

namespace World.GOV.Tests;

public sealed partial class ContentPipelineTests
{
    [Fact]
    public async Task PortalTimelineAndRenderedHistoryUseHistoricalDates()
    {
        var portal = await LoadPortalAsync();
        Assert.Null(portal.LoadError);
        Assert.Equal("DK-0965-001", portal.Timelines[0].IncidentId);

        var html = await RenderPortalPageAsync<History>(portal);
        var jelling = html.IndexOf("href=\"incident/DK-0965-001\"", StringComparison.Ordinal);
        var constitution = html.IndexOf("href=\"incident/DK-1849-001\"", StringComparison.Ordinal);
        var membership = html.IndexOf("href=\"incident/DK-1973-001\"", StringComparison.Ordinal);
        Assert.True(jelling >= 0 && constitution > jelling && membership > constitution,
            "The rendered timeline must place approximate medieval dates before 1849 and 1973.");
    }

    [Fact]
    public async Task PortalLoadsSharedIncidentsOnceAndIncludesEveryListedCountry()
    {
        Change(IncidentFile, json => json["countryIds"] = new JsonArray("DK", "DE"));
        var portal = await LoadPortalAsync();
        Assert.Null(portal.LoadError);
        Assert.Equal(Load().Incidents.Count, portal.Incidents.Count);
        Assert.Single(portal.Incidents, incident => incident.Id == "DK-1849-001");
        Assert.Contains(portal.GetIncidentsByCountry("DK"), incident => incident.Id == "DK-1849-001");
        Assert.Contains(portal.GetIncidentsByCountry("de"), incident => incident.Id == "DK-1849-001");
        Assert.Equal(new[] { "DE", "DK" }, portal.Timelines
            .Where(commit => commit.IncidentId == "DK-1849-001").Select(commit => commit.Country).Order());

        var countryHtml = await RenderPortalPageAsync<CountryDetails>(portal,
            new Dictionary<string, object?> { [nameof(CountryDetails.CountryId)] = "DE" });
        Assert.Contains("href=\"incident/DK-1849-001\"", countryHtml);

        var historyHtml = await RenderPortalPageAsync<History>(portal);
        Assert.Single(Regex.Matches(historyHtml, "href=\"incident/DK-1849-001\""));
    }

    [Fact]
    public async Task PortalKeepsDistinctCountryPerspectivesOnOneSharedTimelineEvent()
    {
        var portal = await LoadPortalAsync();
        var html = await RenderPortalPageAsync<History>(portal);
        Assert.Contains("Denmark perspective", html);
        Assert.Contains("Germany perspective", html);
        Assert.Single(Regex.Matches(html, "href=\"incident/DK-1940-001\""));
        Assert.Single(Regex.Matches(html, "href=\"incident/DE-1940-001\""));
    }

    [Fact]
    public async Task PortalOffersOnlyEnglishAndAvailableIncidentTranslations()
    {
        Change("countries/denmark/country.json", json => json["supportedLocales"]!.AsArray().Add("fr-FR"));
        var portal = await LoadPortalAsync();
        Assert.Null(portal.LoadError);
        Assert.Contains(portal.Sources, source => source.Language == "de-DE");
        Assert.Contains(portal.Countries, country => country.SupportedLocales.Contains("fr-FR"));
        Assert.Equal(new[] { "en-US", "da-DK" }, portal.AvailableLocales);
    }

    private async Task<PortalDataService> LoadPortalAsync()
    {
        var output = StaticSiteExporter.Export(Load());
        using var http = new HttpClient(new ExportedDataHandler(output))
        {
            BaseAddress = new Uri("https://worldgov.test/")
        };
        var portal = new PortalDataService(http);
        await portal.EnsureLoadedAsync();
        return portal;
    }

    private static async Task<string> RenderPortalPageAsync<TComponent>(PortalDataService portal,
        IDictionary<string, object?>? parameters = null, string? locale = null) where TComponent : IComponent
    {
        using var services = new ServiceCollection().AddLogging().AddSingleton(portal).BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        return await renderer.Dispatcher.InvokeAsync(async () =>
        {
            if (locale is not null)
            {
                RenderFragment content = builder =>
                {
                    builder.OpenComponent<TComponent>(0);
                    if (parameters is not null)
                        foreach (var parameter in parameters) builder.AddAttribute(1, parameter.Key, parameter.Value);
                    builder.CloseComponent();
                };
                var localizedPage = await renderer.RenderComponentAsync<CascadingValue<string>>(ParameterView.FromDictionary(
                    new Dictionary<string, object?> { ["Value"] = locale, ["ChildContent"] = content }));
                return localizedPage.ToHtmlString();
            }
            var page = await renderer.RenderComponentAsync<TComponent>(parameters is null ? ParameterView.Empty : ParameterView.FromDictionary(parameters));
            return page.ToHtmlString();
        });
    }

    private sealed class ExportedDataHandler(string outputRoot) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var relativePath = request.RequestUri!.AbsolutePath["/data/".Length..];
            var path = Path.Combine(outputRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            return Task.FromResult(File.Exists(path)
                ? new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(File.ReadAllText(path)) }
                : new HttpResponseMessage(HttpStatusCode.NotFound));
        }
    }
}
