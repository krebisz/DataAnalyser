using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Services.Abstractions;

/// <summary>
///     Unified smoothing service for chart data.
///     Eliminates duplication across all strategies.
/// </summary>
public interface ISmoothingService
{
    /// <summary>
    ///     Creates smoothed values for a series of health metric data.
    /// </summary>
    IReadOnlyList<double> SmoothSeries(IReadOnlyList<MetricData> orderedData, IReadOnlyList<DateTime> timestamps, DateTime from, DateTime to);
}