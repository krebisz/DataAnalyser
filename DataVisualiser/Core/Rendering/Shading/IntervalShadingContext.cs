namespace DataVisualiser.Core.Rendering.Shading;

/// <summary>
///     Context data needed for interval shading calculations.
/// </summary>
public class IntervalShadingContext
{
    /// <summary>
    ///     The intervals (bins) being shaded. Each tuple represents [Min, Max) range.
    /// </summary>
    public List<(double Min, double Max)> Intervals { get; set; } = new();

    /// <summary>
    ///     Raw frequency counts per bucket per interval.
    ///     Key: bucketIndex (0 - n), Value: Dictionary of intervalIndex -> frequency count
    /// </summary>
    public Dictionary<int, Dictionary<int, int>> FrequenciesPerBucket { get; set; } = new();

    /// <summary>
    ///     Raw values per bucket for additional calculations.
    ///     Key: bucketIndex (0 - n), Value: List of values for that bucket
    /// </summary>
    public Dictionary<int, List<double>> BucketValues { get; set; } = new();

    /// <summary>
    ///     Global minimum value across all data.
    /// </summary>
    public double GlobalMin { get; set; }

    /// <summary>
    ///     Global maximum value across all data.
    /// </summary>
    public double GlobalMax { get; set; }
}