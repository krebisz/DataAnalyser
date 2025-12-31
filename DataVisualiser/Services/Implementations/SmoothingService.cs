using DataVisualiser.Helper;
using DataVisualiser.Models;
using DataVisualiser.Services.Abstractions;

namespace DataVisualiser.Services.Implementations
{
    /// <summary>
    /// Implementation of ISmoothingService.
    /// Provides unified smoothing logic with consistent behavior across all strategies.
    /// </summary>
    public sealed class SmoothingService : ISmoothingService
    {
        public IReadOnlyList<double> SmoothSeries(
            IReadOnlyList<HealthMetricData> orderedData,
            IReadOnlyList<DateTime> timestamps,
            DateTime from,
            DateTime to)
        {
            if (orderedData == null || orderedData.Count == 0)
                return Array.Empty<double>();

            if (timestamps == null || timestamps.Count == 0)
                return Array.Empty<double>();

            // Convert IReadOnlyList to List if necessary (MathHelper requires List)
            var dataList = orderedData is List<HealthMetricData> list 
                ? list 
                : orderedData.ToList();

            // Create smoothed data points
            var smoothedPoints = MathHelper.CreateSmoothedData(dataList, from, to);

            // Interpolate to match timestamps
            var timestampsList = timestamps is List<DateTime> tsList ? tsList : timestamps.ToList();
            return MathHelper.InterpolateSmoothedData(smoothedPoints, timestampsList);
        }
    }
}

