namespace DataVisualiser.Shared.Models;

public sealed class DistributionRangeResult
{
    public DistributionRangeResult(IReadOnlyList<double> mins, IReadOnlyList<double> maxs, double globalMin, double globalMax, string? unit)
    {
        Mins = mins;
        Maxs = maxs;
        GlobalMin = globalMin;
        GlobalMax = globalMax;
        Unit = unit;
    }

    public IReadOnlyList<double> Mins { get; }
    public IReadOnlyList<double> Maxs { get; }
    public double GlobalMin { get; }
    public double GlobalMax { get; }
    public string? Unit { get; }
}
