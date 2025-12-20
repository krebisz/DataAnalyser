using System.Windows.Media;

namespace DataVisualiser.Services.Shading
{
    /// <summary>
    /// Context data needed for interval shading calculations.
    /// </summary>
    public class IntervalShadingContext
    {
        /// <summary>
        /// The intervals (bins) being shaded. Each tuple represents [Min, Max) range.
        /// </summary>
        public List<(double Min, double Max)> Intervals { get; set; } = new();

        /// <summary>
        /// Raw frequency counts per day per interval.
        /// Key: dayIndex (0=Monday, 6=Sunday), Value: Dictionary of intervalIndex -> frequency count
        /// </summary>
        public Dictionary<int, Dictionary<int, int>> FrequenciesPerDay { get; set; } = new();

        /// <summary>
        /// Raw values per day for additional calculations.
        /// Key: dayIndex (0=Monday, 6=Sunday), Value: List of values for that day
        /// </summary>
        public Dictionary<int, List<double>> DayValues { get; set; } = new();

        /// <summary>
        /// Global minimum value across all data.
        /// </summary>
        public double GlobalMin { get; set; }

        /// <summary>
        /// Global maximum value across all data.
        /// </summary>
        public double GlobalMax { get; set; }
    }

    /// <summary>
    /// Strategy interface for calculating color shading for intervals in the weekly distribution chart.
    /// Different implementations can use different metrics (frequency, percentage, etc.) to determine shading.
    /// </summary>
    public interface IIntervalShadingStrategy
    {
        /// <summary>
        /// Calculates the color map for all intervals across all days.
        /// </summary>
        /// <param name="context">Context containing intervals, frequencies, and other data needed for calculation</param>
        /// <returns>
        /// Dictionary mapping dayIndex -> Dictionary mapping intervalIndex -> Color.
        /// Only non-zero intervals should be included in the color map.
        /// Zero-frequency intervals are handled separately (white shading).
        /// </returns>
        Dictionary<int, Dictionary<int, Color>> CalculateColorMap(IntervalShadingContext context);

        /// <summary>
        /// Calculates the color for a specific interval on a specific day.
        /// This is used when rendering individual intervals and may use different normalization
        /// (e.g., per-interval max frequency vs global max frequency).
        /// </summary>
        /// <param name="context">Context containing intervals, frequencies, and other data</param>
        /// <param name="dayIndex">Day index (0=Monday, 6=Sunday)</param>
        /// <param name="intervalIndex">Interval index within the intervals list</param>
        /// <param name="intervalMaxFrequency">Maximum frequency for this specific interval across all days</param>
        /// <param name="globalMaxFrequency">Global maximum frequency across all days and intervals</param>
        /// <returns>The color to use for this interval on this day, or null if it should be white (zero frequency)</returns>
        Color? CalculateIntervalColor(
            IntervalShadingContext context,
            int dayIndex,
            int intervalIndex,
            int intervalMaxFrequency,
            int globalMaxFrequency);
    }
}

