using DataVisualiser.Tests.Helpers;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Rendering.Interaction;
using DataVisualiser.UI.Charts.Infrastructure;
using DataVisualiser.UI.Charts.Rendering;
using DataVisualiser.UI.Charts.Rendering.LiveCharts;
using LiveCharts.Wpf;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

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

    [Fact]
    public async Task ApplyAsync_ForBarPieColumnChart_UsesSimpleTooltipAndInteractiveLegend()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var renderer = new LiveChartsChartRenderer();
            var surface = new TestTrackedSurface();

            await renderer.ApplyAsync(surface, new UiChartRenderModel
            {
                ChartName = RenderingDefaults.BarPieChartName,
                Legend = new ChartLegendModel
                {
                    IsVisible = true,
                    Placement = ChartLegendPlacement.Right
                },
                Series =
                [
                    new ChartSeriesModel
                    {
                        Name = "Series A",
                        SeriesType = ChartSeriesType.Column,
                        Values = [10d]
                    },
                    new ChartSeriesModel
                    {
                        Name = "Series B",
                        SeriesType = ChartSeriesType.Column,
                        Values = [30d]
                    }
                ]
            });

            var contentGrid = Assert.IsType<Grid>(surface.ChartContent);
            var chart = Assert.IsType<CartesianChart>(contentGrid.Children[0]);
            var legendContainer = Assert.IsType<Border>(contentGrid.Children[1]);
            var legendItems = Assert.IsType<ItemsControl>(legendContainer.Child);
            var legendEntries = Assert.IsAssignableFrom<IEnumerable>(legendItems.ItemsSource).Cast<object>().ToList();

            Assert.Equal(RenderingDefaults.BarPieChartName, chart.Name);
            Assert.IsType<SimpleChartTooltip>(chart.DataTooltip);
            Assert.Equal(2, legendEntries.Count);
            Assert.All(legendEntries, entry => Assert.IsType<LegendToggleManager.LegendItem>(entry));
        });
    }

    [Fact]
    public async Task ApplyAsync_ForBarPiePieFacet_UsesInteractiveLegend()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var renderer = new LiveChartsChartRenderer();
            var surface = new TestTrackedSurface();

            await renderer.ApplyAsync(surface, new UiChartRenderModel
            {
                ChartName = RenderingDefaults.BarPieChartName,
                Legend = new ChartLegendModel
                {
                    IsVisible = true,
                    Placement = ChartLegendPlacement.Right
                },
                Facets =
                [
                    new ChartFacetModel
                    {
                        Title = "Facet 1",
                        Series =
                        [
                            new ChartSeriesModel { Name = "Series A", SeriesType = ChartSeriesType.Pie, Values = [10d] },
                            new ChartSeriesModel { Name = "Series B", SeriesType = ChartSeriesType.Pie, Values = [30d] }
                        ]
                    },
                    new ChartFacetModel
                    {
                        Title = "Facet 2",
                        Series =
                        [
                            new ChartSeriesModel { Name = "Series A", SeriesType = ChartSeriesType.Pie, Values = [15d] },
                            new ChartSeriesModel { Name = "Series B", SeriesType = ChartSeriesType.Pie, Values = [35d] }
                        ]
                    }
                ]
            });

            var contentGrid = Assert.IsType<Grid>(surface.ChartContent);
            var legendContainer = Assert.IsType<Border>(contentGrid.Children[1]);
            var legendItems = Assert.IsType<ItemsControl>(legendContainer.Child);
            var legendEntries = Assert.IsAssignableFrom<IEnumerable>(legendItems.ItemsSource).Cast<object>().ToList();

            Assert.Equal(2, legendEntries.Count);
            Assert.All(legendEntries, entry => Assert.IsType<PieFacetLegendToggleManager.LegendItem>(entry));
        });
    }

    private sealed class TestTrackedSurface : IChartSurface, ITrackedCartesianChartSurface, ITrackedChartContentSurface
    {
        public CartesianChart? RenderedCartesianChart { get; private set; }
        public bool HasRenderedContent { get; private set; }
        public UIElement? ChartContent { get; private set; }

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
            ChartContent = content;
        }
    }
}
