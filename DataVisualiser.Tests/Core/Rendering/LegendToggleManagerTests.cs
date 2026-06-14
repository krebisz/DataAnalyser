using DataVisualiser.Tests.Helpers;
using DataVisualiser.Tests.Helpers.Infrastructure;
using DataVisualiser.UI.Charts.Presentation;
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace DataVisualiser.Tests.Core.Rendering;

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

    [Fact]
    public void LegendItemStroke_ShouldFollowSeriesStrokeChanges()
    {
        StaTestHelper.Run(() =>
        {
            var series = new LineSeries
            {
                Title = "Ave",
                Stroke = Brushes.Black,
                Values = new ChartValues<double> { 10d }
            };
            var chart = new CartesianChart
            {
                Series = new SeriesCollection { series }
            };
            var manager = new LegendToggleManager(chart);
            var item = manager.Items.Single();
            var changed = false;
            item.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(LegendToggleManager.LegendItem.Stroke))
                    changed = true;
            };

            series.Stroke = Brushes.White;

            Assert.True(changed);
            Assert.Same(Brushes.White, item.Stroke);
        });
    }
}
