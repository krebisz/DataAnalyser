using DataVisualiser.Core.Rendering.BarPie;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.Charts.Presentation.Rendering;
using LiveCharts.Wpf;

namespace DataVisualiser.Tests.UI.Rendering;

public sealed class BarPieRenderingQualificationProbeTests
{
    [Theory]
    [InlineData(BarPieRenderingRoute.Column)]
    [InlineData(BarPieRenderingRoute.PieFacet)]
    public async Task Probe_ForLiveChartsRoute_PassesLifecycleStages(BarPieRenderingRoute route)
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var contract = new BarPieRenderingContract();
            var probe = new BarPieRenderingQualificationProbe();
            var surface = new ProbeSurface();
            var renderer = new ProbeRenderer();
            var host = new BarPieChartRenderHost(surface, renderer, ChartRendererKind.LiveCharts, true);
            var request = new BarPieChartRenderRequest(route, CreateModel(route, isVisible: true));
            var hiddenRequest = new BarPieChartRenderRequest(route, CreateModel(route, isVisible: false));

            var result = await probe.ProbeAsync(contract, host, request, hiddenRequest, request);

            Assert.True(result.Passed, string.Join(Environment.NewLine, result.Failures));
            Assert.True(result.InitialRenderPassed);
            Assert.True(result.RepeatedUpdatePassed);
            Assert.True(result.VisibilityTransitionPassed);
            Assert.True(result.ResetViewPassed);
            Assert.True(result.ClearPassed);
            Assert.True(result.DisposalPassed);
        });
    }

    private static UiChartRenderModel CreateModel(BarPieRenderingRoute route, bool isVisible)
    {
        return route switch
        {
            BarPieRenderingRoute.PieFacet => new UiChartRenderModel
            {
                Title = "Bar / Pie",
                IsVisible = isVisible,
                Facets =
                [
                    new ChartFacetModel
                    {
                        Title = "Bucket 1",
                        Series =
                        [
                            new ChartSeriesModel
                            {
                                Name = "Series A",
                                SeriesType = ChartSeriesType.Pie,
                                Values = [1]
                            }
                        ]
                    }
                ]
            },
            _ => new UiChartRenderModel
            {
                Title = "Bar / Pie",
                IsVisible = isVisible,
                Series =
                [
                    new ChartSeriesModel
                    {
                        Name = "Series A",
                        SeriesType = ChartSeriesType.Column,
                        Values = [1, 2, 3]
                    }
                ],
                AxesX =
                [
                    new ChartAxisModel
                    {
                        Title = "Interval",
                        Labels = ["A", "B", "C"]
                    }
                ],
                AxesY =
                [
                    new ChartAxisModel
                    {
                        Title = "Value"
                    }
                ]
            }
        };
    }

    private sealed class ProbeSurface : IChartSurface, ITrackedChartContentSurface, ITrackedCartesianChartSurface
    {
        public bool HasRenderedContent { get; private set; }
        public CartesianChart? RenderedCartesianChart { get; private set; }

        public void SetRenderedCartesianChart(CartesianChart? chart)
        {
            RenderedCartesianChart = chart;
        }

        public void SetHasRenderedContent(bool hasRenderedContent)
        {
            HasRenderedContent = hasRenderedContent;
        }

        public void SetTitle(string? title) { }
        public void SetIsVisible(bool isVisible) { }
        public void SetHeader(System.Windows.UIElement? header) { }
        public void SetBehavioralControls(System.Windows.UIElement? controls) { }
        public void SetChartContent(System.Windows.UIElement? content) { }
    }

    private sealed class ProbeRenderer : IChartRenderer
    {
        public Task ApplyAsync(IChartSurface surface, UiChartRenderModel model, CancellationToken cancellationToken = default)
        {
            if (surface is ITrackedChartContentSurface trackedContent)
                trackedContent.SetHasRenderedContent(model.HasRenderableContent);

            if (surface is ITrackedCartesianChartSurface trackedCartesian)
            {
                if (model.Series.Count > 0 && model.Facets.Count == 0)
                {
                    var chart = new CartesianChart();
                    chart.AxisX.Add(new Axis());
                    trackedCartesian.SetRenderedCartesianChart(chart);
                }
                else
                {
                    trackedCartesian.SetRenderedCartesianChart(null);
                }
            }

            return Task.CompletedTask;
        }
    }
}
