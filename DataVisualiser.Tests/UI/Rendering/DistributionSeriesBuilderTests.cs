using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Tests.Helpers;
using LiveCharts.Wpf;

namespace DataVisualiser.Tests.UI.Rendering;

public sealed class DistributionSeriesBuilderTests
{
    [Fact]
    public async Task AddBaselineAndRangeSeries_ShouldAppendTwoSeriesWithExpectedValues()
    {
        await StaTestHelper.RunAsync(() =>
        {
            var chart = new CartesianChart();

            DistributionSeriesBuilder.AddBaselineAndRangeSeries(
                chart,
                [1.0, double.NaN],
                [0.5, -1.0],
                10.0,
                "Weight",
                useFrequencyShading: false,
                bucketCount: 2);

            Assert.Equal(2, chart.Series.Count);
            var baseline = chart.Series[0];
            var range = chart.Series[1];
            Assert.Equal([1.0, 0.0], baseline.Values.Cast<double>().ToList());
            Assert.Equal([0.5, 0.0], range.Values.Cast<double>().ToList());

            return Task.CompletedTask;
        });
    }

    [Fact]
    public async Task AddAverageSeries_ShouldAddAverageLineUsingValidBucketValuesOnly()
    {
        await StaTestHelper.RunAsync(() =>
        {
            var chart = new CartesianChart();
            var bucketValues = new Dictionary<int, List<double>>
            {
                [0] = [1.0, 3.0],
                [1] = [double.NaN, 4.0]
            };

            DistributionSeriesBuilder.AddAverageSeries(chart, bucketValues, "Weight", 2);

            Assert.Single(chart.Series);
            var series = chart.Series[0];
            Assert.Equal([2.0, 4.0], series.Values.Cast<double>().ToList());

            return Task.CompletedTask;
        });
    }
}
