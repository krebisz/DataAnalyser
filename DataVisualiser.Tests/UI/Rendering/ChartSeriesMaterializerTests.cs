using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Rendering.Helpers;

namespace DataVisualiser.Tests.UI.Rendering;

public sealed class ChartSeriesMaterializerTests
{
    [Fact]
    public void ResolveStackedSeriesValues_ShouldPreferSmoothed_WhenSmoothedContainsValidValues()
    {
        var timestamps = new List<DateTime> { new(2024, 1, 1), new(2024, 1, 2) };
        var series = new SeriesResult
        {
            Timestamps = timestamps,
            RawValues = [1d, 2d],
            Smoothed = [10d, 20d]
        };

        var values = ChartSeriesMaterializer.ResolveStackedSeriesValues(series, timestamps, out var usedSmoothed);

        Assert.True(usedSmoothed);
        Assert.Equal([10d, 20d], values);
    }

    [Fact]
    public void ResolveStackedSeriesValues_ShouldFallbackToRaw_WhenSmoothedHasNoValidValues()
    {
        var timestamps = new List<DateTime> { new(2024, 1, 1), new(2024, 1, 2) };
        var series = new SeriesResult
        {
            Timestamps = timestamps,
            RawValues = [1d, 2d],
            Smoothed = [double.NaN, double.NaN]
        };

        var values = ChartSeriesMaterializer.ResolveStackedSeriesValues(series, timestamps, out var usedSmoothed);

        Assert.False(usedSmoothed);
        Assert.Equal([1d, 2d], values);
    }

    [Fact]
    public void GetValueStats_ShouldCountValidAndInvalidValues()
    {
        var stats = ChartSeriesMaterializer.GetValueStats([1d, double.NaN, double.PositiveInfinity, 4d]);

        Assert.Equal(2, stats.Valid);
        Assert.Equal(2, stats.NaN);
    }

}
