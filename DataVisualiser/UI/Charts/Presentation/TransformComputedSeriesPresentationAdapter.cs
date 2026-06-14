using DataVisualiser.Shared.Models;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.UI.Charts.Presentation;

internal static class TransformComputedSeriesPresentationAdapter
{
    public static IReadOnlyList<MetricData> ProjectMetricData(ComputedSeriesResult computedSeries)
    {
        ArgumentNullException.ThrowIfNull(computedSeries);

        return computedSeries.Timeline
            .Select((timestamp, index) => new MetricData
            {
                NormalizedTimestamp = timestamp,
                Value = ToDecimalOrNull(computedSeries.RawValues[index])
            })
            .ToArray();
    }

    public static IReadOnlyList<SeriesOperationRequest> CreateOperationRequests(
        ComputedSeriesResult computedSeries,
        SeriesOperationKind? operationKind)
    {
        ArgumentNullException.ThrowIfNull(computedSeries);

        if (operationKind == null)
            return [];

        return
        [
            new SeriesOperationRequest(
                operationKind.Value,
                ResolveInputIndexes(computedSeries, operationKind.Value),
                computedSeries.Id,
                computedSeries.Label)
        ];
    }

    public static string? ResolveOperationType(SeriesOperationKind? operationKind) =>
        operationKind switch
        {
            SeriesOperationKind.Sum => "+",
            SeriesOperationKind.Difference => "-",
            SeriesOperationKind.Ratio => "/",
            _ => null
        };

    private static IReadOnlyList<int> ResolveInputIndexes(ComputedSeriesResult computedSeries, SeriesOperationKind operationKind)
    {
        var sourceCount = computedSeries.SourceSeriesSignatures.Count;
        if (sourceCount > 0)
            return Enumerable.Range(0, sourceCount).ToArray();

        return operationKind switch
        {
            SeriesOperationKind.Difference or SeriesOperationKind.Ratio => [0, 1],
            SeriesOperationKind.Sum => [0, 1],
            _ => [0]
        };
    }

    private static decimal? ToDecimalOrNull(double value) =>
        double.IsNaN(value) || double.IsInfinity(value)
            ? null
            : (decimal)value;
}
