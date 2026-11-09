using Denmark.NET;
using Xunit;

namespace World.GOV.Tests;

public sealed class GovernmentPolicyTests
{
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
