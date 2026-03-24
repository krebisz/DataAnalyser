using DataVisualiser.UI.Charts.Helpers;

namespace DataVisualiser.Core.Rendering.CartesianMetrics;

public sealed class CartesianMetricChartRenderingContract : ICartesianMetricChartRenderingContract
{
    private static readonly IReadOnlyList<CartesianMetricBackendQualification> QualificationMatrix =
    [
        new CartesianMetricBackendQualification(
            CartesianMetricBackendKey.LiveChartsWpfMain,
            "CartesianMetric.Main.LiveChartsWpf",
            CartesianMetricRenderingQualification.Qualified,
            CartesianMetricChartRoute.Main,
            SupportsRender: true,
            SupportsUpdate: true,
            SupportsHoverTooltip: true,
            SupportsResetView: true,
            SupportsClear: true,
            SupportsLifecycleSafety: true),
        new CartesianMetricBackendQualification(
            CartesianMetricBackendKey.LiveChartsWpfNormalized,
            "CartesianMetric.Normalized.LiveChartsWpf",
            CartesianMetricRenderingQualification.Qualified,
            CartesianMetricChartRoute.Normalized,
            SupportsRender: true,
            SupportsUpdate: true,
            SupportsHoverTooltip: true,
            SupportsResetView: true,
            SupportsClear: true,
            SupportsLifecycleSafety: true),
        new CartesianMetricBackendQualification(
            CartesianMetricBackendKey.LiveChartsWpfDiffRatio,
            "CartesianMetric.DiffRatio.LiveChartsWpf",
            CartesianMetricRenderingQualification.Qualified,
            CartesianMetricChartRoute.DiffRatio,
            SupportsRender: true,
            SupportsUpdate: true,
            SupportsHoverTooltip: true,
            SupportsResetView: true,
            SupportsClear: true,
            SupportsLifecycleSafety: true)
    ];

    private readonly ICartesianMetricChartRenderInvoker _renderInvoker;

    public CartesianMetricChartRenderingContract(ICartesianMetricChartRenderInvoker renderInvoker)
    {
        _renderInvoker = renderInvoker ?? throw new ArgumentNullException(nameof(renderInvoker));
    }

    public IReadOnlyList<CartesianMetricBackendQualification> GetBackendQualificationMatrix()
    {
        return QualificationMatrix;
    }

    public CartesianMetricRenderingCapabilities GetCapabilities(CartesianMetricChartRoute route)
    {
        var qualification = QualificationMatrix.FirstOrDefault(entry => entry.Route == route);
        if (qualification == null)
            throw new ArgumentOutOfRangeException(nameof(route), route, "Unknown cartesian metric chart rendering route.");

        return new CartesianMetricRenderingCapabilities(
            qualification.PathKey,
            qualification.Qualification,
            qualification.SupportsRender,
            qualification.SupportsUpdate,
            qualification.SupportsHoverTooltip,
            qualification.SupportsResetView,
            qualification.SupportsClear,
            qualification.SupportsLifecycleSafety);
    }

    public Task RenderAsync(CartesianMetricChartRenderRequest request, CartesianMetricChartRenderHost host)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        if (host == null)
            throw new ArgumentNullException(nameof(host));

        return _renderInvoker.RenderAsync(request, host);
    }

    public void Clear(CartesianMetricChartRoute route, CartesianMetricChartRenderHost host)
    {
        if (host == null)
            throw new ArgumentNullException(nameof(host));

        ChartSurfaceHelper.ClearCartesian(host.Chart, host.ChartState);
    }

    public void ResetView(CartesianMetricChartRoute route, CartesianMetricChartRenderHost host)
    {
        if (host == null)
            throw new ArgumentNullException(nameof(host));

        ChartSurfaceHelper.ResetZoom(host.Chart);
    }

    public bool HasRenderableContent(CartesianMetricChartRoute route, CartesianMetricChartRenderHost host)
    {
        if (host == null)
            throw new ArgumentNullException(nameof(host));

        return ChartSurfaceHelper.HasSeries(host.Chart);
    }
}
