namespace DataVisualiser.Shared.Models;

public sealed class DistributionRangeResult
{
    public DistributionRangeResult(IReadOnlyList<double> mins, IReadOnlyList<double> maxs, IReadOnlyList<double> averages, double globalMin, double globalMax, string? unit)
    {
        Mins = mins;
        Maxs = maxs;
        Averages = averages;
        GlobalMin = globalMin;
        GlobalMax = globalMax;
        Unit = unit;
    }

    public IReadOnlyList<double> Mins { get; }
    public IReadOnlyList<double> Maxs { get; }
    public IReadOnlyList<double> Averages { get; }
    public double GlobalMin { get; }
    public double GlobalMax { get; }
    public string? Unit { get; }
}