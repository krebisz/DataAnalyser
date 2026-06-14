using System.Windows;
using System.Windows.Media;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Tests.Helpers.Infrastructure;
using LiveCharts.Wpf;

namespace DataVisualiser.Tests.Core.Rendering;

public sealed class ChartThemeStylingHelperTests
{
    [Fact]
    public void ApplyAxisTheme_ShouldUseConfiguredAxisAndGridBrushes()
    {
        StaTestHelper.Run(() =>
        {
            var app = Application.Current ?? new Application();
            app.Resources["ThemeChartAxisBrush"] = Brushes.Black;
            app.Resources["ThemeChartGridLineBrush"] = Brushes.Black;

            var chart = new CartesianChart();
            chart.AxisX.Add(new Axis());
            chart.AxisY.Add(new Axis());

            ChartThemeStylingHelper.ApplyCartesianChartTheme(chart);

            Assert.Same(Brushes.Black, chart.AxisX[0].Foreground);
            Assert.Same(Brushes.Black, chart.AxisX[0].Separator.Stroke);
            Assert.Same(Brushes.Black, chart.AxisY[0].Foreground);
            Assert.Same(Brushes.Black, chart.AxisY[0].Separator.Stroke);
        });
    }

    [Fact]
    public void ApplyAxisTheme_ShouldReplacePreviouslyAppliedThemeBrushes()
    {
        StaTestHelper.Run(() =>
        {
            var app = Application.Current ?? new Application();
            app.Resources["ThemeChartAxisBrush"] = Brushes.Black;
            app.Resources["ThemeChartGridLineBrush"] = Brushes.Black;

            var chart = new CartesianChart();
            chart.AxisX.Add(new Axis());

            ChartThemeStylingHelper.ApplyCartesianChartTheme(chart);

            app.Resources["ThemeChartAxisBrush"] = Brushes.White;
            app.Resources["ThemeChartGridLineBrush"] = Brushes.DimGray;

            ChartThemeStylingHelper.ApplyCartesianChartTheme(chart);

            Assert.Same(Brushes.White, chart.AxisX[0].Foreground);
            Assert.Same(Brushes.DimGray, chart.AxisX[0].Separator.Stroke);
        });
    }
}
