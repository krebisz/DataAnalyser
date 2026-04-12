using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering;

namespace DataVisualiser.Tests.Orchestration;

public sealed class ChartYAxisDataBuilderTests
{
    [Fact]
    public void BuildSyntheticRawData_ShouldSumStackedSeriesValues()
    {
        var timestamps = new List<DateTime> { new(2024, 1, 1), new(2024, 1, 2) };
        var model = new ChartRenderModel
        {
            IsStacked = true,
            Timestamps = timestamps,
            Series =
            [
                new SeriesResult { Timestamps = timestamps, RawValues = [1d, 2d] },
                new SeriesResult { Timestamps = timestamps, RawValues = [3d, 4d] }
            ]
        };

        var raw = ChartYAxisDataBuilder.BuildSyntheticRawData(model);

        Assert.Equal(2, raw.Count);
        Assert.Equal(4m, raw[0].Value);
        Assert.Equal(6m, raw[1].Value);
    }

    [Fact]
    public void CollectSmoothedValues_ShouldIncludeOverlayRawWhenOverlaySmoothedMissing()
    {
        var model = new ChartRenderModel
        {
            PrimarySmoothed = [1d, 2d],
            OverlaySeries =
            [
                new SeriesResult { RawValues = [5d, 6d] }
            ]
        };

        var values = ChartYAxisDataBuilder.CollectSmoothedValues(model);

        Assert.Equal([1d, 2d, 5d, 6d], values);
    }
}
