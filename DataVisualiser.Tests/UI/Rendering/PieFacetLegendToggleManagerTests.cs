using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.Charts.Infrastructure;
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows.Controls.Primitives;
using System.Windows;

namespace DataVisualiser.Tests.UI.Rendering;

public sealed class PieFacetLegendToggleManagerTests
{
    [Fact]
    public void HandleToggle_UpdatesMatchingPieSeriesAcrossAllFacets()
    {
        StaTestHelper.Run(() =>
        {
            var firstChart = CreateChart(10d, 30d);
            var secondChart = CreateChart(15d, 35d);
            var manager = new PieFacetLegendToggleManager([firstChart, secondChart]);
            var firstItem = manager.Items.Single(item => item.Title == "Series A");
            var toggle = new ToggleButton
            {
                DataContext = firstItem,
                IsChecked = false
            };

            PieFacetLegendToggleManager.HandleToggle(toggle);

            Assert.False(firstItem.IsVisible);
            Assert.All(new[] { firstChart, secondChart }, chart =>
            {
                var series = chart.Series.OfType<PieSeries>().Single(item => item.Title == "Series A");
                Assert.Equal(Visibility.Collapsed, series.Visibility);
            });
        });
    }

    private static PieChart CreateChart(double seriesAValue, double seriesBValue)
    {
        return new PieChart
        {
            Series = new SeriesCollection
            {
                new PieSeries
                {
                    Title = "Series A",
                    Values = new ChartValues<double> { seriesAValue }
                },
                new PieSeries
                {
                    Title = "Series B",
                    Values = new ChartValues<double> { seriesBValue }
                }
            }
        };
    }
}
