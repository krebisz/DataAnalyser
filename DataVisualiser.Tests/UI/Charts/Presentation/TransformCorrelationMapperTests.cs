using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Kernel;

namespace DataVisualiser.Tests.UI.Charts.Presentation;

public sealed class TransformCorrelationMapperTests
{
    [Fact]
    public void Compute_ShouldCalculatePairCorrelation()
    {
        var aligned = new AlignedSeriesBundle(
            [new DateTime(2026, 1, 1), new DateTime(2026, 1, 2), new DateTime(2026, 1, 3)],
            [
                new AlignedMetricSeries(new MetricSeriesRequest("Weight", "fat", "Weight", "Fat"), [1d, 2d, 3d], [1d, 2d, 3d]),
                new AlignedMetricSeries(new MetricSeriesRequest("Weight", "muscle", "Weight", "Muscle"), [2d, 4d, 6d], [2d, 4d, 6d])
            ]);

        var summary = TransformCorrelationMapper.Compute(aligned, "Correlation");

        Assert.Equal("Weight - Fat ~ Weight - Muscle", summary.Label);
        Assert.Equal(1d, summary.Correlation, precision: 8);
        Assert.Equal(3, summary.SampleCount);
    }

    [Fact]
    public void OperationChainProvider_ShouldExposeNoneAndExtendedTransformOperations()
    {
        var tags = new OperationChainTransformOperationProvider()
            .GetOperations()
            .Select(item => item.Tag)
            .ToArray();

        Assert.Equal(["None", "Log", "Sqrt", "Add", "Subtract", "Divide", "Sum3", "Correlation", "Sum3Correlation"], tags);
    }
}
