using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.Charts.Presentation.Rendering;
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace DataVisualiser.Tests.UI.Rendering;

public sealed class LegendToggleManagerTests
{
    [Fact]
    public void HandleToggle_UpdatesCartesianSeriesVisibility()
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
                    }
                }
            };
            var manager = new LegendToggleManager(chart);
            var item = manager.Items.Single();
            var toggle = new ToggleButton
            {
                DataContext = item,
                IsChecked = false
            };

            LegendToggleManager.HandleToggle(toggle);

            Assert.False(item.IsVisible);
            Assert.Equal(Visibility.Collapsed, chart.Series.OfType<Series>().Single().Visibility);
        });
    }
}
