using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.Charts.Rendering;
using DataVisualiser.UI.Charts.Rendering.LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.Tests.UI.Rendering;

public sealed class LiveChartsChartRendererTests
{
    [Fact]
    public async Task ApplyAsync_TracksRenderableContentState_AndCartesianSurface()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var renderer = new LiveChartsChartRenderer();
            var surface = new TestTrackedSurface();

            await renderer.ApplyAsync(surface, new UiChartRenderModel
            {
                Series =
                [
                    new ChartSeriesModel
                    {
                        Name = "Series A",
                        SeriesType = ChartSeriesType.Column,
                        Values = [1d]
                    }
                ]
            });

            Assert.True(surface.HasRenderedContent);
            Assert.NotNull(surface.RenderedCartesianChart);

            await renderer.ApplyAsync(surface, new UiChartRenderModel());

            Assert.False(surface.HasRenderedContent);
            Assert.Null(surface.RenderedCartesianChart);
        });
    }

    private sealed class TestTrackedSurface : IChartSurface, ITrackedCartesianChartSurface, ITrackedChartContentSurface
    {
        public CartesianChart? RenderedCartesianChart { get; private set; }
        public bool HasRenderedContent { get; private set; }

        public void SetRenderedCartesianChart(CartesianChart? chart)
        {
            RenderedCartesianChart = chart;
        }

        public void SetHasRenderedContent(bool hasRenderedContent)
        {
            HasRenderedContent = hasRenderedContent;
        }

        public void SetTitle(string? title)
        {
        }

        public void SetIsVisible(bool isVisible)
        {
        }

        public void SetHeader(System.Windows.UIElement? header)
        {
        }

        public void SetBehavioralControls(System.Windows.UIElement? controls)
        {
        }

        public void SetChartContent(System.Windows.UIElement? content)
        {
        }
    }
}
