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
    ///     Raw frequency counts per day per interval.
    ///     Key: dayIndex (0=Monday, 6=Sunday), Value: Dictionary of intervalIndex -> frequency count
    /// </summary>
    public Dictionary<int, Dictionary<int, int>> FrequenciesPerDay { get; set; } = new();

    /// <summary>
    ///     Raw values per day for additional calculations.
    ///     Key: dayIndex (0=Monday, 6=Sunday), Value: List of values for that day
    /// </summary>
    public Dictionary<int, List<double>> DayValues { get; set; } = new();

    /// <summary>
    ///     Global minimum value across all data.
    /// </summary>
    public double GlobalMin { get; set; }

    /// <summary>
    ///     Global maximum value across all data.
    /// </summary>
    public double GlobalMax { get; set; }
}