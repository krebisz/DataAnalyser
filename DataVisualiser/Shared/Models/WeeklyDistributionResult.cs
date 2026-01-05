namespace DataVisualiser.Shared.Models;

/// <summary>
///     Contains the results of weekly distribution computation including frequency binning data.
/// </summary>
public class WeeklyDistributionResult
{
    // Basic min/max data (existing)
    public List<double> Mins   { get; init; } = new();
    public List<double> Maxs   { get; init; } = new();
    public List<double> Ranges { get; init; } = new();
    public List<int>    Counts { get; init; } = new();

    // Raw data values per day (needed for frequency counting)
    // dayIndex: 0=Monday, 1=Tuesday, ..., 6=Sunday
    public Dictionary<int, List<double>> BucketValues { get; init; } = new();

    // Frequency binning data (new)
    public double                         GlobalMin { get; init; }
    public double                         GlobalMax { get; init; }
    public double                         BinSize   { get; init; }
    public List<(double Min, double Max)> Bins      { get; init; } = new();

    // Frequency counts per day per bin: [dayIndex][binIndex] = frequency
    // dayIndex: 0=Monday, 1=Tuesday, ..., 6=Sunday
    // binIndex: index into Bins list
    public Dictionary<int, Dictionary<int, int>> FrequenciesPerBucket { get; init; } = new();

    // Normalized frequencies: [dayIndex][binIndex] = normalized frequency [0.0, 1.0]
    public Dictionary<int, Dictionary<int, double>> NormalizedFrequenciesPerBucket { get; init; } = new();

    public string? Unit { get; init; }
}