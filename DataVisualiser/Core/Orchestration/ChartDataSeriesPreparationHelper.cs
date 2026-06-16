using DataVisualiser.Core.Computation.TimeSeries;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Orchestration;

internal static class ChartDataSeriesPreparationHelper
{
    internal static (
        IReadOnlyList<DateTime> Timestamps,
        IReadOnlyList<double> RawValues1,
        IReadOnlyList<double> RawValues2,
        IReadOnlyList<double> SmoothedValues1,
        IReadOnlyList<double> SmoothedValues2,
        IReadOnlyList<double> DifferenceValues,
        IReadOnlyList<double> RatioValues,
        IReadOnlyList<double> NormalizedValues1,
        IReadOnlyList<double> NormalizedValues2)
        Prepare(List<MetricData> primarySeries, List<MetricData> secondarySeries, int smoothingWindow)
    {
        var prepared = TimeSeriesPreparation.Prepare(primarySeries, secondarySeries, smoothingWindow);

        return (
            prepared.Timestamps,
            prepared.RawValues1,
            prepared.RawValues2,
            prepared.SmoothedValues1,
            prepared.SmoothedValues2,
            prepared.DifferenceValues,
            prepared.RatioValues,
            prepared.NormalizedValues1,
            prepared.NormalizedValues2);
    }
}
