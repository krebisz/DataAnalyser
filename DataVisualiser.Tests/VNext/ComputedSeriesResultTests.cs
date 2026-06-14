using DataVisualiser.Core.Computation.Results;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.Tests.VNext;

public sealed class ComputedSeriesResultTests
{
    [Fact]
    public void FromDerivedDataset_ShouldPreserveNeutralComputationShape()
    {
        var dataset = new DerivedDataset(
            "sum",
            "Total",
            [new DateTime(2026, 1, 1), new DateTime(2026, 1, 2)],
            [3d, 6d],
            [3d, 4.5d],
            ["Weight:morning", "Weight:evening"],
            "Sum:sum:0,1:",
            new Dictionary<string, string> { ["Source"] = "Test" });

        var result = ComputedSeriesResult.FromDerivedDataset(dataset);

        Assert.Equal("sum", result.Id);
        Assert.Equal("Total", result.Label);
        Assert.Equal(dataset.Timeline, result.Timeline);
        Assert.Equal(dataset.RawValues, result.RawValues);
        Assert.Equal(dataset.SmoothedValues, result.SmoothedValues);
        Assert.Equal(dataset.SourceSeriesSignatures, result.SourceSeriesSignatures);
        Assert.Equal(dataset.OperationSignature, result.OperationSignature);
        Assert.Equal("Test", result.Metadata["Source"]);
    }

    [Fact]
    public void FromChartSeriesProgram_ShouldRejectCardinalityDrift()
    {
        var program = new ChartSeriesProgram("sum", "Total", [1d], [1d, 2d]);

        Assert.Throws<ArgumentException>(() => ComputedSeriesResult.FromChartSeriesProgram(
            program,
            [new DateTime(2026, 1, 1)]));
    }

    [Fact]
    public void Adapter_ShouldProjectComputedSeriesToLegacySeriesResult()
    {
        var computed = new ComputedSeriesResult(
            "sum",
            "Total",
            [new DateTime(2026, 1, 1)],
            [3d],
            [2.5d],
            ["Weight:morning"],
            "Sum:sum:0:");

        var series = ComputedSeriesResultAdapters.ToSeriesResult(computed);
        var chartResult = ComputedSeriesResultAdapters.ToChartComputationResult(computed);

        Assert.Equal("sum", series.SeriesId);
        Assert.Equal("Total", series.DisplayName);
        Assert.Equal([3d], series.RawValues);
        Assert.Equal([2.5d], series.Smoothed);
        Assert.Equal([3d], chartResult.PrimaryRawValues);
        Assert.Equal([2.5d], chartResult.PrimarySmoothed);
        Assert.Single(chartResult.Series!);
    }
}
