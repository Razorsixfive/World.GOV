namespace World.GOV.Core;

public sealed record HistoricalDate : IComparable<HistoricalDate>
{
    public int? StartYear { get; init; }
    public int? Month { get; init; }
    public int? Day { get; init; }
    public int? EndYear { get; init; }
    public required string Precision { get; init; }
    public required string Calendar { get; init; }
    public bool Circa { get; init; }
    public required string DisplayText { get; init; }

    public int CompareTo(HistoricalDate? other)
    {
        if (other is null) return 1;
        var comparison = Nullable.Compare(StartYear, other.StartYear);
        if (comparison != 0) return StartYear is null ? 1 : comparison;
        comparison = Nullable.Compare(Month, other.Month);
        if (comparison != 0) return comparison;
        comparison = Nullable.Compare(Day, other.Day);
        if (comparison != 0) return comparison;
        return Nullable.Compare(EndYear, other.EndYear);
    }

    public override string ToString() => DisplayText;
}
