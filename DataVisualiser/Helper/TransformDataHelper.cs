using DataVisualiser.Models;

namespace DataVisualiser.Helper;

/// <summary>
///     Phase 4: Helper methods for transform data processing.
/// </summary>
public static class TransformDataHelper
{
    /// <summary>
    ///     Creates result data objects for transform grid display.
    /// </summary>
    public static List<object> CreateTransformResultData(List<HealthMetricData> dataList, List<double> results)
    {
        return dataList.Zip(results, (d, r) => new
            {
                Timestamp = d.NormalizedTimestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                Value = double.IsNaN(r) ? "NaN" : r.ToString("F4")
            }).
            ToList<object>();
    }
}