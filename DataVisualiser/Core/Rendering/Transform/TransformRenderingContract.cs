using DataVisualiser.UI.Charts.Helpers;

namespace DataVisualiser.Core.Rendering.Transform;

public sealed class TransformRenderingContract : ITransformRenderingContract
{
    private static readonly IReadOnlyList<TransformBackendQualification> QualificationMatrix =
    [
        new TransformBackendQualification(
            TransformBackendKey.LiveChartsWpfResultCartesian,
            "Transform.Result.LiveChartsWpf",
            TransformRenderingQualification.Qualified,
            TransformRenderingRoute.ResultCartesian,
            SupportsRender: true,
            SupportsUpdate: true,
            SupportsHoverTooltip: true,
            SupportsResetView: true,
            SupportsClear: true,
            SupportsLifecycleSafety: true)
    ];

    private readonly ITransformChartRenderInvoker _renderInvoker;

    public TransformRenderingContract(ITransformChartRenderInvoker renderInvoker)
    {
        _renderInvoker = renderInvoker ?? throw new ArgumentNullException(nameof(renderInvoker));
    }

    public IReadOnlyList<TransformBackendQualification> GetBackendQualificationMatrix()
    {
        return QualificationMatrix;
    }

    public TransformRenderingCapabilities GetCapabilities(TransformRenderingRoute route)
    {
        var qualification = QualificationMatrix.FirstOrDefault(entry => entry.Route == route);
        if (qualification == null)
            throw new ArgumentOutOfRangeException(nameof(route), route, "Unknown transform rendering route.");

        return new TransformRenderingCapabilities(
            qualification.PathKey,
            qualification.Qualification,
            qualification.SupportsRender,
            qualification.SupportsUpdate,
            qualification.SupportsHoverTooltip,
            qualification.SupportsResetView,
            qualification.SupportsClear,
            qualification.SupportsLifecycleSafety);
    }

    public Task RenderAsync(TransformChartRenderRequest request, TransformChartRenderHost host)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        if (host == null)
            throw new ArgumentNullException(nameof(host));

        return _renderInvoker.RenderAsync(request, host);
    }

    public void Clear(TransformRenderingRoute route, TransformChartRenderHost host)
    {
        if (host == null)
            throw new ArgumentNullException(nameof(host));

        ChartSurfaceHelper.ClearCartesian(host.Chart, host.ChartState);
        host.ResetAuxiliaryVisuals?.Invoke();
    }

    public void ResetView(TransformRenderingRoute route, TransformChartRenderHost host)
    {
        if (host == null)
            throw new ArgumentNullException(nameof(host));

        ChartSurfaceHelper.ResetZoom(host.Chart);
    }

    public bool HasRenderableContent(TransformRenderingRoute route, TransformChartRenderHost host)
    {
        if (host == null)
            throw new ArgumentNullException(nameof(host));

        return ChartSurfaceHelper.HasSeries(host.Chart);
    }
}
