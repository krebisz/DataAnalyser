using DataVisualiser.Models;

namespace DataVisualiser.Services.Abstractions
{
    /// <summary>
    /// Unified smoothing service for chart data.
    /// Eliminates duplication across all strategies.
    /// </summary>
    public interface ISmoothingService
    {
        /// <summary>
        /// Creates smoothed values for a series of health metric data.
        /// </summary>
        IReadOnlyList<double> SmoothSeries(
            IReadOnlyList<HealthMetricData> orderedData,
            IReadOnlyList<DateTime> timestamps,
            DateTime from,
            DateTime to);
    }
}

