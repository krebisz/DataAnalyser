namespace DataVisualiser.Shared.Models;

/// <summary>
///     Contains the results of weekly distribution computation including frequency binning data.
/// </summary>
public class HourlyDistributionResult
{
    // Basic min/max data (existing)
    public List<double> Mins   { get; init; } = new();
    public List<double> Maxs   { get; init; } = new();
    public List<double> Ranges { get; init; } = new();
    public List<int>    Counts { get; init; } = new();

    // Raw data values per hour (needed for frequency counting)
    // hourIndex: 0=12AM, 1=1AM, ..., 23=11PM
    public Dictionary<int, List<double>> HourValues { get; init; } = new();

    // Frequency binning data (new)
    public double                         GlobalMin { get; init; }
    public double                         GlobalMax { get; init; }
    public double                         BinSize   { get; init; }
    public List<(double Min, double Max)> Bins      { get; init; } = new();

    // Frequency counts per hour per bin: [hourIndex][binIndex] = frequency
    // hourIndex: 0=12AM, 1=1AM, ..., 23=11PM
    // binIndex: index into Bins list
    public Dictionary<int, Dictionary<int, int>> FrequenciesPerHour { get; init; } = new();

    // Normalized frequencies: [hourIndex][binIndex] = normalized frequency [0.0, 1.0]
    public Dictionary<int, Dictionary<int, double>> NormalizedFrequenciesPerHour { get; init; } = new();

    public string? Unit { get; init; }
}