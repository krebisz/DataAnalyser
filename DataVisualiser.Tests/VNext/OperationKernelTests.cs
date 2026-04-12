using DataVisualiser.Shared.Models;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Kernel;

namespace DataVisualiser.Tests.VNext;

public sealed class OperationKernelTests
{
    [Fact]
    public void BuildSeries_Sum_ShouldAggregateAcrossAllRequestedInputs()
    {
        var kernel = new OperationKernel();
        var bundle = CreateBundle(
            CreateAlignedSeries("Weight", "a", [1d, 2d], [10d, 20d]),
            CreateAlignedSeries("Weight", "b", [2d, double.NaN], [20d, double.NaN]),
            CreateAlignedSeries("Weight", "c", [3d, 4d], [30d, 40d]));

        var program = kernel.BuildSeries(bundle, SeriesOperationRequest.Sum([0, 1, 2], "Total"));

        Assert.Equal("sum", program.Id);
        Assert.Equal("Total", program.Label);
        Assert.Equal([6d, 6d], program.RawValues);
        Assert.Equal([60d, 60d], program.SmoothedValues);
    }

    [Fact]
    public void BuildSeries_Normalize_ShouldPreserveNaNAndScaleRawAndSmoothed()
    {
        var kernel = new OperationKernel();
        var bundle = CreateBundle(CreateAlignedSeries("Weight", "a", [1d, double.NaN, 4d], [2d, 4d, 8d]));

        var program = kernel.BuildSeries(bundle, SeriesOperationRequest.Normalize(0, "n", "Normalized"));

        Assert.Equal([0.25d, double.NaN, 1d], program.RawValues);
        Assert.Equal([0.25d, 0.5d, 1d], program.SmoothedValues);
    }

    [Fact]
    public void BuildSeries_Difference_ShouldUseDistinctRawAndSmoothedResults()
    {
        var kernel = new OperationKernel();
        var bundle = CreateBundle(
            CreateAlignedSeries("Weight", "a", [5d, 7d], [50d, 70d]),
            CreateAlignedSeries("Weight", "b", [2d, 3d], [20d, 10d]));

        var program = kernel.BuildSeries(bundle, SeriesOperationRequest.Difference(0, 1, "A - B"));

        Assert.Equal([3d, 4d], program.RawValues);
        Assert.Equal([30d, 60d], program.SmoothedValues);
    }

    [Fact]
    public void BuildSeries_ShouldThrow_WhenOperationReferencesOutOfRangeSeriesIndex()
    {
        var kernel = new OperationKernel();
        var bundle = CreateBundle(CreateAlignedSeries("Weight", "a", [1d], [1d]));

        Assert.Throws<ArgumentOutOfRangeException>(() => kernel.BuildSeries(bundle, SeriesOperationRequest.Ratio(0, 1, "Invalid")));
    }

    private static AlignedSeriesBundle CreateBundle(params AlignedMetricSeries[] series)
    {
        return new AlignedSeriesBundle(
            new[] { new DateTime(2026, 1, 1), new DateTime(2026, 1, 2), new DateTime(2026, 1, 3) }.Take(series[0].RawValues.Count).ToArray(),
            series);
    }

    private static AlignedMetricSeries CreateAlignedSeries(string metricType, string subtype, IReadOnlyList<double> raw, IReadOnlyList<double> smoothed)
    {
        return new AlignedMetricSeries(
            new MetricSeriesRequest(metricType, subtype, metricType, subtype),
            raw,
            smoothed);
    }
}
