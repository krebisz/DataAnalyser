using DataVisualiser.UI.Charts.Helpers;
using DataVisualiser.UI.Charts.Rendering;
using DataVisualiser.UI.Defaults;

namespace DataVisualiser.Core.Rendering.BarPie;

public sealed class BarPieRenderingContract : IBarPieRenderingContract
{
    private static readonly IReadOnlyList<BarPieBackendQualification> QualificationMatrix =
    [
        new BarPieBackendQualification(
            BarPieBackendKey.LiveChartsWpfColumn,
            "BarPie.Column.LiveChartsWpf",
            ChartRendererKind.LiveCharts,
            BarPieRenderingRoute.Column,
            BarPieRenderingQualification.Qualified,
            SupportsRender: true,
            SupportsUpdate: true,
            SupportsResetView: true,
            SupportsClear: true,
            SupportsLifecycleSafety: true),
        new BarPieBackendQualification(
            BarPieBackendKey.LiveChartsWpfPieFacet,
            "BarPie.PieFacet.LiveChartsWpf",
            ChartRendererKind.LiveCharts,
            BarPieRenderingRoute.PieFacet,
            BarPieRenderingQualification.Qualified,
            SupportsRender: true,
            SupportsUpdate: true,
            SupportsResetView: false,
            SupportsClear: true,
            SupportsLifecycleSafety: true),
        new BarPieBackendQualification(
            BarPieBackendKey.EChartsPlaceholderColumn,
            "BarPie.Column.EChartsPlaceholder",
            ChartRendererKind.ECharts,
            BarPieRenderingRoute.Column,
            BarPieRenderingQualification.UnqualifiedDebt,
            SupportsRender: false,
            SupportsUpdate: false,
            SupportsResetView: false,
            SupportsClear: false,
            SupportsLifecycleSafety: false),
        new BarPieBackendQualification(
            BarPieBackendKey.EChartsPlaceholderPieFacet,
            "BarPie.PieFacet.EChartsPlaceholder",
            ChartRendererKind.ECharts,
            BarPieRenderingRoute.PieFacet,
            BarPieRenderingQualification.UnqualifiedDebt,
            SupportsRender: false,
            SupportsUpdate: false,
            SupportsResetView: false,
            SupportsClear: false,
            SupportsLifecycleSafety: false)
    ];

    public IReadOnlyList<BarPieBackendQualification> GetBackendQualificationMatrix()
    {
        return QualificationMatrix;
    }

    public BarPieRenderingCapabilities GetCapabilities(ChartRendererKind rendererKind, BarPieRenderingRoute route)
    {
        var qualification = QualificationMatrix.FirstOrDefault(entry => entry.RendererKind == rendererKind && entry.Route == route);
        if (qualification == null)
            throw new ArgumentOutOfRangeException(nameof(route), route, "Unknown bar/pie rendering route.");

        return new BarPieRenderingCapabilities(
            qualification.PathKey,
            qualification.Qualification,
            qualification.SupportsRender,
            qualification.SupportsUpdate,
            qualification.SupportsResetView,
            qualification.SupportsClear,
            qualification.SupportsLifecycleSafety);
    }

    public Task RenderAsync(BarPieChartRenderRequest request, BarPieChartRenderHost host)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        if (host == null)
            throw new ArgumentNullException(nameof(host));

        return host.Renderer.ApplyAsync(host.Surface, request.Model);
    }

    public Task ClearAsync(BarPieChartRenderHost host)
    {
        if (host == null)
            throw new ArgumentNullException(nameof(host));

        return host.Renderer.ApplyAsync(host.Surface, CreateEmptyModel(host.IsVisible));
    }

    public void ResetView(BarPieRenderingRoute route, BarPieChartRenderHost host)
    {
        if (host == null)
            throw new ArgumentNullException(nameof(host));

        if (route == BarPieRenderingRoute.PieFacet)
            return;

        if (route != BarPieRenderingRoute.Column)
            throw new ArgumentOutOfRangeException(nameof(route), route, "Unknown bar/pie rendering route.");

        var chart = (host.Surface as ITrackedCartesianChartSurface)?.RenderedCartesianChart;
        if (chart != null)
            ChartSurfaceHelper.ResetZoom(chart);
    }

    public bool HasRenderableContent(BarPieRenderingRoute route, BarPieChartRenderHost host)
    {
        if (host == null)
            throw new ArgumentNullException(nameof(host));

        if (host.Surface is ITrackedChartContentSurface trackedSurface)
            return trackedSurface.HasRenderedContent;

        if (route == BarPieRenderingRoute.Column)
        {
            var chart = (host.Surface as ITrackedCartesianChartSurface)?.RenderedCartesianChart;
            return chart != null && ChartSurfaceHelper.HasSeries(chart);
        }

        if (route == BarPieRenderingRoute.PieFacet)
            return false;

        throw new ArgumentOutOfRangeException(nameof(route), route, "Unknown bar/pie rendering route.");
    }

    private static UiChartRenderModel CreateEmptyModel(bool isVisible)
    {
        return new UiChartRenderModel
        {
            Title = ChartUiDefaults.BarPieChartTitle,
            IsVisible = isVisible,
            Series = Array.Empty<ChartSeriesModel>(),
            Facets = Array.Empty<ChartFacetModel>()
        };
    }
}
