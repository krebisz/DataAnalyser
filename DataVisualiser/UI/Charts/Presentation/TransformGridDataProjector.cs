using DataVisualiser.Core.Transforms;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.UI.Charts.Presentation;

internal static class TransformGridDataProjector
{
    public static IReadOnlyList<TransformResultDataRow> ProjectMetricRows(IEnumerable<MetricData>? data)
    {
        var rows = MetricDataSeriesHelper.FilterValuedAndOrder(data);

        var values = rows
            .Select(point => point.Value.HasValue ? (double)point.Value.Value : double.NaN)
            .ToList();

        return TransformExpressionEvaluator.CreateTransformResultRows(rows, values);
    }

    public static IReadOnlyList<TransformResultDataRow> ProjectResultRows(List<MetricData> dataList, List<double> results) =>
        TransformExpressionEvaluator.CreateTransformResultRows(dataList, results);

    public static IReadOnlyList<TransformComputedSeriesGridRow> ProjectComputedRows(ComputedSeriesResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var dataList = TransformComputedSeriesPresentationAdapter.ProjectMetricData(result).ToList();
        var rawRows = ProjectResultRows(dataList, result.RawValues.ToList());
        var smoothedRows = ProjectResultRows(dataList, result.SmoothedValues.ToList());

        return rawRows
            .Select((row, index) => new TransformComputedSeriesGridRow(
                row.Timestamp,
                row.Value,
                smoothedRows[index].Value))
            .ToArray();
    }
}

internal sealed record TransformComputedSeriesGridRow(
    string Timestamp,
    string Raw,
    string Smoothed);
