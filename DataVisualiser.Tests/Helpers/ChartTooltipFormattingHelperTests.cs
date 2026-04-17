using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Rendering.Interaction;
using LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.Tests.Helpers;

public sealed class ChartTooltipFormattingHelperTests
{
    [Theory]
    [InlineData("Weight (Raw)", "Weight", true, false)]
    [InlineData("Weight (raw)", "Weight", true, false)]
    [InlineData("Weight (smooth)", "Weight", false, true)]
    [InlineData("Weight", "Weight", false, false)]
    public void ParseSeriesTitle_ShouldPreserveExistingTitleSemantics(string title, string expectedBaseName, bool expectedRaw, bool expectedSmoothed)
    {
        var (baseName, isRaw, isSmoothed) = ChartTooltipFormattingHelper.ParseSeriesTitle(title);

        Assert.Equal(expectedBaseName, baseName);
        Assert.Equal(expectedRaw, isRaw);
        Assert.Equal(expectedSmoothed, isSmoothed);
    }

    [Fact]
    public void GetChartValuesAtIndex_ShouldReturnPipeSeparatedSeriesValues()
    {
        StaTestHelper.Run(() =>
        {
            var chart = new CartesianChart
            {
                Series = new SeriesCollection
                {
                    new LineSeries { Title = "Weight", Values = new ChartValues<double> { 10, 20 } },
                    new LineSeries { Title = "Fat", Values = new ChartValues<double> { 2, 3 } }
                }
            };

            var result = ChartTooltipFormattingHelper.GetChartValuesAtIndex(chart, 1);

            Assert.Equal("Weight: 20 | Fat: 3", result);
        });
    }

    [Fact]
    public void GetChartValuesFormattedAtIndex_ShouldGroupRawAndSmoothedPairs()
    {
        StaTestHelper.Run(() =>
        {
            var chart = new CartesianChart
            {
                Series = new SeriesCollection
                {
                    new LineSeries { Title = "Weight (smooth)", Values = new ChartValues<double> { 10, 20 } },
                    new LineSeries { Title = "Fat (smooth)", Values = new ChartValues<double> { 2, 3 } },
                    new LineSeries { Title = "Weight (Raw)", Values = new ChartValues<double> { 11, 21 } },
                    new LineSeries { Title = "Fat (Raw)", Values = new ChartValues<double> { 4, 5 } }
                }
            };

            var result = ChartTooltipFormattingHelper.GetChartValuesFormattedAtIndex(chart, 1);

            Assert.Equal("Weight smooth: 20; Fat smooth: 3; Weight Raw: 21; Fat Raw: 5", result);
        });
    }

    [Fact]
    public void GetChartValuesFormattedAtIndex_ShouldAddStackedTotalAndIgnoreOverlaySeries()
    {
        StaTestHelper.Run(() =>
        {
            var chart = new CartesianChart
            {
                Tag = new ChartStackingTooltipState(includeTotal: true, isCumulative: false, overlaySeriesNames: ["Target"]),
                Series = new SeriesCollection
                {
                    new StackedAreaSeries { Title = "Weight (smooth)", Values = new ChartValues<double> { 10, 20 } },
                    new StackedAreaSeries { Title = "Fat (smooth)", Values = new ChartValues<double> { 2, 3 } },
                    new LineSeries { Title = "Target", Values = new ChartValues<double> { 50, 60 } }
                }
            };

            var result = ChartTooltipFormattingHelper.GetChartValuesFormattedAtIndex(chart, 1);

            Assert.Equal("Weight smooth: 20; Fat smooth: 3; Target value: 60; Total: 23", result);
        });
    }

    [Fact]
    public void GetChartValuesFormattedAtIndex_ShouldReconstructCumulativeOriginalValues()
    {
        StaTestHelper.Run(() =>
        {
            var chart = new CartesianChart
            {
                Tag = new ChartStackingTooltipState(includeTotal: true, isCumulative: true),
                Series = new SeriesCollection
                {
                    new StackedAreaSeries { Title = "Weight (Raw)", Values = new ChartValues<double> { 10, 20 } },
                    new StackedAreaSeries { Title = "Fat (Raw)", Values = new ChartValues<double> { 13, 25 } }
                }
            };

            var result = ChartTooltipFormattingHelper.GetChartValuesFormattedAtIndex(chart, 1);

            Assert.Equal("Weight Raw: 20; Fat Raw: 5; Total: 25", result);
        });
    }

    [Fact]
    public void GetChartValuesFormattedAtIndex_ShouldUseOriginalSeriesForCumulativeTotalsWhenAvailable()
    {
        StaTestHelper.Run(() =>
        {
            var originalSeries = new[]
            {
                new SeriesResult { DisplayName = "Weight", RawValues = [10, 20] },
                new SeriesResult { DisplayName = "Fat", RawValues = [2, 3] }
            };
            var chart = new CartesianChart
            {
                Tag = new ChartStackingTooltipState(includeTotal: true, isCumulative: true, originalSeries: originalSeries),
                Series = new SeriesCollection
                {
                    new StackedAreaSeries { Title = "Weight (Raw)", Values = new ChartValues<double> { 10, 20 } },
                    new StackedAreaSeries { Title = "Fat (Raw)", Values = new ChartValues<double> { 12, 23 } }
                }
            };

            var result = ChartTooltipFormattingHelper.GetChartValuesFormattedAtIndex(chart, 1);

            Assert.Equal("Weight Raw: 20; Fat Raw: 3; Total: 23", result);
        });
    }
}
