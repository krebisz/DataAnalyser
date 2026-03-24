using DataVisualiser.Core.Rendering.Interaction;
using LiveCharts;
using LiveCharts.Wpf;
using DataVisualiser.Tests.Helpers;
using System.Windows;

namespace DataVisualiser.Tests.Helpers;

public sealed class ChartTooltipParticipationCalculatorTests
{
    [Fact]
    public void BuildColumnSeriesParticipationLookup_ReturnsPercentagesForVisibleColumnSeries()
    {
        StaTestHelper.Run(() =>
        {
            var chart = new CartesianChart
            {
                Series = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Series A",
                        Values = new ChartValues<double> { 10d }
                    },
                    new ColumnSeries
                    {
                        Title = "Series B",
                        Values = new ChartValues<double> { 30d }
                    },
                    new ColumnSeries
                    {
                        Title = "Series Hidden",
                        Values = new ChartValues<double> { 100d },
                        Visibility = Visibility.Collapsed
                    }
                }
            };

            var lookup = ChartTooltipParticipationCalculator.BuildColumnSeriesParticipationLookup(chart, 0);

            Assert.Equal(2, lookup.Count);
            Assert.Equal(0.25d, lookup["Series A"], 3);
            Assert.Equal(0.75d, lookup["Series B"], 3);
            Assert.DoesNotContain("Series Hidden", lookup.Keys);
        });
    }
}
