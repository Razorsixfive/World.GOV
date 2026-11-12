namespace Denmark.NET;

/// <summary>Fictional GOV-003 reporting policy. Amounts remain visible after administrative success.</summary>
public sealed class BudgetAllocationService
{
    public BudgetReport ReclassifyOverrunAsForecast(BudgetReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        // The expenditure is final. The definition of "within budget" remains negotiable.
        return report with { ReportingBaseline = Math.Max(report.ReportingBaseline, report.ActualExpenditure) };
    }
}

public sealed record BudgetReport(decimal ApprovedBudget, decimal ActualExpenditure)
{
    public decimal ReportingBaseline { get; init; } = ApprovedBudget;
    public decimal OriginalOverrun => Math.Max(0m, ActualExpenditure - ApprovedBudget);
    public decimal ReportedOverrun => Math.Max(0m, ActualExpenditure - ReportingBaseline);
}
