using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.OperationChain;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.Tests.UI.Charts.Presentation;

public sealed class OperationChainTransformProjectionTests
{
    [Fact]
    public void BuildInputRows_ShouldMirrorTransformMetricProjection()
    {
        var data = CreateData([2m, 1m], [new DateTime(2026, 1, 2), new DateTime(2026, 1, 1)]);

        var operationRows = TransformGridRowProjector.BuildInputRows(data);
        var transformRows = TransformGridDataProjector.ProjectMetricRows(data);

        Assert.Equal(transformRows.Count, operationRows.Count);
        for (var index = 0; index < transformRows.Count; index++)
        {
            Assert.Equal(transformRows[index].Timestamp, operationRows[index].Timestamp);
            Assert.Equal(transformRows[index].Value, operationRows[index].Value);
        }
    }

    [Fact]
    public void BuildComputedRows_ShouldMirrorTransformComputedProjection()
    {
        var result = new ComputedSeriesResult(
            "sum",
            "Total",
            [new DateTime(2026, 1, 1), new DateTime(2026, 1, 2)],
            [3d, 6d],
            [3d, 4.5d]);

        var operationRows = TransformGridRowProjector.BuildComputedRows(result, "Total");
        var transformRows = TransformGridDataProjector.ProjectComputedRows(result);

        Assert.Equal(transformRows.Count, operationRows.Count);
        for (var index = 0; index < transformRows.Count; index++)
        {
            Assert.Equal(transformRows[index].Timestamp, operationRows[index].Timestamp);
            Assert.Equal(transformRows[index].Raw, operationRows[index].Raw);
            Assert.Equal(transformRows[index].Smoothed, operationRows[index].Smoothed);
            Assert.Equal("Total", operationRows[index].Series);
        }
    }

    private static IReadOnlyList<MetricData> CreateData(IReadOnlyList<decimal> values, IReadOnlyList<DateTime> timestamps) =>
        values
            .Select((value, index) => new MetricData
            {
                NormalizedTimestamp = timestamps[index],
                Value = value
            })
            .ToArray();
}
