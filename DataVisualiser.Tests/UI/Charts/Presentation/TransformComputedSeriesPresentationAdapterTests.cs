using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.Tests.UI.Charts.Presentation;

public sealed class TransformComputedSeriesPresentationAdapterTests
{
    [Fact]
    public void ProjectMetricData_ShouldConvertComputedRawValuesToMetricData()
    {
        var result = new ComputedSeriesResult(
            "sum",
            "Total",
            [new DateTime(2026, 1, 1), new DateTime(2026, 1, 2)],
            [1.5d, double.NaN],
            [1.5d, 1.5d]);

        var data = TransformComputedSeriesPresentationAdapter.ProjectMetricData(result);

        Assert.Equal(2, data.Count);
        Assert.Equal(new DateTime(2026, 1, 1), data[0].NormalizedTimestamp);
        Assert.Equal(1.5m, data[0].Value);
        Assert.Null(data[1].Value);
    }

    [Fact]
    public void CreateOperationRequests_ShouldUseComputedSeriesSourceSignaturesWhenAvailable()
    {
        var result = new ComputedSeriesResult(
            "sum",
            "Total",
            [new DateTime(2026, 1, 1)],
            [3d],
            [3d],
            ["a", "b", "c"]);

        var requests = TransformComputedSeriesPresentationAdapter.CreateOperationRequests(result, SeriesOperationKind.Sum);

        var request = Assert.Single(requests);
        Assert.Equal(SeriesOperationKind.Sum, request.Kind);
        Assert.Equal([0, 1, 2], request.InputIndexes);
        Assert.Equal("Total", request.Label);
    }
}
