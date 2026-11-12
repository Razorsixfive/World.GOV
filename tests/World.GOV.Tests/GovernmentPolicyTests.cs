using Denmark.NET;
using Xunit;

namespace World.GOV.Tests;

public sealed class GovernmentPolicyTests
{
    [Fact]
    public void ReclassifyingAnOverrunAsAForecastDoesNotRecoverAnyMoney()
    {
        var original = new BudgetReport(ApprovedBudget: 100m, ActualExpenditure: 470m);
        var revised = new BudgetAllocationService().ReclassifyOverrunAsForecast(original);

        Assert.Equal(0m, revised.ReportedOverrun);
        Assert.Equal(370m, revised.OriginalOverrun);
        Assert.Equal(original.ApprovedBudget, revised.ApprovedBudget);
        Assert.Equal(original.ActualExpenditure, revised.ActualExpenditure);
        Assert.Equal(100m, original.ReportingBaseline);
        Assert.Equal(370m, original.ReportedOverrun);
    }

    [Theory]
    [InlineData(80)]
    [InlineData(100)]
    public void StayingWithinBudgetDoesNotRequireARetroactiveRevision(int actualExpenditure)
    {
        var original = new BudgetReport(ApprovedBudget: 100m, ActualExpenditure: actualExpenditure);
        var revised = new BudgetAllocationService().ReclassifyOverrunAsForecast(original);

        Assert.Equal(original, revised);
        Assert.Equal(0m, revised.ReportedOverrun);
    }

    [Fact]
    public void AcknowledgingACriticalIncidentRestoresAdministrativeHealthWithoutFixingIt()
    {
        var original = new DenmarkCountryModule().GetIncidents().First(incident => incident.Satire.Severity == "CRITICAL");
        var unresolved = original with { Satire = original.Satire with { Acknowledged = false } };
        var acknowledged = unresolved with { Satire = unresolved.Satire with { Acknowledged = true } };
        var policy = new OperationalContinuityService();
        Assert.False(policy.IsAdministrativelyHealthy([unresolved]));
        Assert.True(policy.IsAdministrativelyHealthy([acknowledged]));
        Assert.Equal(unresolved.Satire.DiagnosticMessage, acknowledged.Satire.DiagnosticMessage);
        Assert.Equal("CRITICAL", acknowledged.Satire.Severity);
    }
}
