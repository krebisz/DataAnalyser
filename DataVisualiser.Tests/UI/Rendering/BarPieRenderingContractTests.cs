using DataVisualiser.Core.Rendering.BarPie;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.Charts.Rendering;
using LiveCharts.Wpf;

namespace DataVisualiser.Tests.UI.Rendering;

public sealed class BarPieRenderingContractTests
{
    [Fact]
    public void GetBackendQualificationMatrix_IncludesQualifiedLiveChartsAndPlaceholderDebt()
    {
        var contract = new BarPieRenderingContract();

        var entries = contract.GetBackendQualificationMatrix();

        Assert.Contains(entries, entry => entry.BackendKey == BarPieBackendKey.LiveChartsWpfColumn
            && entry.RendererKind == ChartRendererKind.LiveCharts
            && entry.Route == BarPieRenderingRoute.Column
            && entry.Qualification == BarPieRenderingQualification.Qualified);
        Assert.Contains(entries, entry => entry.BackendKey == BarPieBackendKey.LiveChartsWpfPieFacet
            && entry.RendererKind == ChartRendererKind.LiveCharts
            && entry.Route == BarPieRenderingRoute.PieFacet
            && entry.Qualification == BarPieRenderingQualification.Qualified);
        Assert.Contains(entries, entry => entry.BackendKey == BarPieBackendKey.EChartsPlaceholderColumn
            && entry.RendererKind == ChartRendererKind.ECharts
            && entry.Route == BarPieRenderingRoute.Column
            && entry.Qualification == BarPieRenderingQualification.UnqualifiedDebt);
    }

    [Fact]
    public void GetCapabilities_ForLiveChartsPieRoute_ReportsQualifiedWithoutReset()
    {
        var contract = new BarPieRenderingContract();

        var capabilities = contract.GetCapabilities(ChartRendererKind.LiveCharts, BarPieRenderingRoute.PieFacet);

        Assert.Equal("BarPie.PieFacet.LiveChartsWpf", capabilities.PathKey);
        Assert.Equal(BarPieRenderingQualification.Qualified, capabilities.Qualification);
        Assert.True(capabilities.SupportsRender);
        Assert.False(capabilities.SupportsResetView);
    }

    [Theory]
    [InlineData(false, BarPieRenderingRoute.Column)]
    [InlineData(true, BarPieRenderingRoute.PieFacet)]
    public void ResolveRoute_ReturnsExpectedRoute(bool isPieMode, BarPieRenderingRoute expectedRoute)
    {
        var route = BarPieRenderingRouteResolver.Resolve(isPieMode);

        Assert.Equal(expectedRoute, route);
    }

    [Fact]
    public void ResetView_ForPieFacetRoute_IsNoOp()
    {
        var contract = new BarPieRenderingContract();
        var host = new BarPieChartRenderHost(new StubTrackedSurface(), new StubRenderer(), ChartRendererKind.LiveCharts, true);

        contract.ResetView(BarPieRenderingRoute.PieFacet, host);
    }

    [Fact]
    public void HasRenderableContent_UsesTrackedSurfaceState()
    {
        var contract = new BarPieRenderingContract();
        var surface = new StubTrackedSurface
        {
            HasRenderedContentValue = true
        };
        var host = new BarPieChartRenderHost(surface, new StubRenderer(), ChartRendererKind.LiveCharts, true);

        Assert.True(contract.HasRenderableContent(BarPieRenderingRoute.PieFacet, host));

        surface.SetHasRenderedContent(false);

        Assert.False(contract.HasRenderableContent(BarPieRenderingRoute.PieFacet, host));
    }

    private sealed class StubTrackedSurface : IChartSurface, ITrackedChartContentSurface, ITrackedCartesianChartSurface
    {
        public bool HasRenderedContentValue { get; set; }

        public bool HasRenderedContent => HasRenderedContentValue;

        public CartesianChart? RenderedCartesianChart { get; private set; }

        public void SetRenderedCartesianChart(CartesianChart? chart)
        {
            RenderedCartesianChart = chart;
        }

        public void SetHasRenderedContent(bool hasRenderedContent)
        {
            HasRenderedContentValue = hasRenderedContent;
        }

        public void SetTitle(string? title) { }
        public void SetIsVisible(bool isVisible) { }
        public void SetHeader(System.Windows.UIElement? header) { }
        public void SetBehavioralControls(System.Windows.UIElement? controls) { }
        public void SetChartContent(System.Windows.UIElement? content) { }
    }

    private sealed class StubRenderer : IChartRenderer
    {
        public Task ApplyAsync(IChartSurface surface, UiChartRenderModel model, CancellationToken cancellationToken = default)
        {
            if (surface is ITrackedChartContentSurface tracked)
                tracked.SetHasRenderedContent(model.HasRenderableContent);

            return Task.CompletedTask;
        }
    }
}
