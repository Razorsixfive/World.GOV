namespace World.GOV.Core;

public interface ICountryModule
{
    CountryDefinition Definition { get; }
    IEnumerable<GovernmentIncident> GetIncidents();
}

public interface IIncidentCatalog
{
    IReadOnlyCollection<GovernmentIncident> GetByCountry(string countryId);
    GovernmentIncident? FindById(string incidentId);
}

public interface IHistoricalTimeline
{
    IReadOnlyList<VirtualCommit> GetHistory(string countryId);
    IReadOnlyList<VirtualCommit> GetBlame(string entityId);
}
