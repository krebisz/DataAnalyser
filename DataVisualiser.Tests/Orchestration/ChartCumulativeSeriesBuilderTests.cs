using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Strategies.Abstractions;

namespace DataVisualiser.Tests.Orchestration;

public sealed class ChartCumulativeSeriesBuilderTests
{
    [Fact]
    public void Build_ShouldAccumulateMultiSeriesInTimelineOrder()
    {
        var timestamps = new List<DateTime> { new(2024, 1, 1), new(2024, 1, 2) };
        var result = new ChartComputationResult
        {
            Series =
            [
                new SeriesResult { SeriesId = "a", DisplayName = "A", Timestamps = timestamps, RawValues = [1d, 2d], Smoothed = [1d, 2d] },
                new SeriesResult { SeriesId = "b", DisplayName = "B", Timestamps = timestamps, RawValues = [3d, 4d], Smoothed = [3d, 4d] }
            ]
        };

        var (renderSeries, originalSeries) = ChartCumulativeSeriesBuilder.Build(result, new StubStrategy(), "Primary", null);

        Assert.NotNull(renderSeries);
        Assert.NotNull(originalSeries);
        Assert.Equal([1d, 2d], renderSeries![0].RawValues);
        Assert.Equal([4d, 6d], renderSeries[1].RawValues);
        Assert.Equal([1d, 2d], originalSeries![0].RawValues);
        Assert.Equal([3d, 4d], originalSeries[1].RawValues);
    }

    [Fact]
    public void Build_ShouldAccumulateLegacyPrimaryAndSecondary()
    {
        var result = new ChartComputationResult
        {
            Timestamps = [new DateTime(2024, 1, 1), new DateTime(2024, 1, 2)],
            PrimaryRawValues = [1d, 2d],
            PrimarySmoothed = [1d, 2d],
            SecondaryRawValues = [3d, 4d],
            SecondarySmoothed = [3d, 4d]
        };

        var (renderSeries, originalSeries) = ChartCumulativeSeriesBuilder.Build(result, new StubStrategy(), "Primary", "Secondary");

        Assert.NotNull(renderSeries);
        Assert.NotNull(originalSeries);
        Assert.Equal([1d, 2d], renderSeries![0].RawValues);
        Assert.Equal([4d, 6d], renderSeries[1].RawValues);
        Assert.Equal("Primary", renderSeries[0].DisplayName);
        Assert.Equal("Secondary", renderSeries[1].DisplayName);
    }

    private sealed class StubStrategy : IChartComputationStrategy
    {
        public string PrimaryLabel => "Primary";
        public string SecondaryLabel => "Secondary";
        public string? Unit => "kg";
        public ChartComputationResult? Compute() => throw new NotSupportedException();
    }
}
