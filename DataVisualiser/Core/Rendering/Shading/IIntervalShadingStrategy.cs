using System.Windows.Media;

namespace DataVisualiser.Core.Rendering.Shading;

/// <summary>
///     Strategy interface for calculating color shading for intervals in the weekly distribution chart.
///     Different implementations can use different metrics (frequency, percentage, etc.) to determine shading.
/// </summary>
public interface IIntervalShadingStrategy
{
    /// <summary>
    ///     Calculates the color map for all intervals across all days.
    /// </summary>
    /// <param name="context">Context containing intervals, frequencies, and other data needed for calculation</param>
    /// <returns>
    ///     Dictionary mapping dayIndex -> Dictionary mapping intervalIndex -> Color.
    ///     Only non-zero intervals should be included in the color map.
    ///     Zero-frequency intervals are handled separately (white shading).
    /// </returns>
    Dictionary<int, Dictionary<int, Color>> CalculateColorMap(IntervalShadingContext context);

    /// <summary>
    ///     Calculates the color for a specific interval on a specific day.
    ///     This is used when rendering individual intervals and may use different normalization
    ///     (e.g., per-interval max frequency vs global max frequency).
    /// </summary>
    /// <param name="context">Context containing intervals, frequencies, and other data</param>
    /// <param name="bucketIndex">Bucket index (0 - n)</param>
    /// <param name="intervalIndex">Interval index within the intervals list</param>
    /// <param name="intervalMaxFrequency">Maximum frequency for this specific interval across all days</param>
    /// <param name="globalMaxFrequency">Global maximum frequency across all days and intervals</param>
    /// <returns>The color to use for this interval on this day, or null if it should be white (zero frequency)</returns>
    Color? CalculateIntervalColor(IntervalShadingContext context, int bucketIndex, int intervalIndex, int intervalMaxFrequency, int globalMaxFrequency);
}