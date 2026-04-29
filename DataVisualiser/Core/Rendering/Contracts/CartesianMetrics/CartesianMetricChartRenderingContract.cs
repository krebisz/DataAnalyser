using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;
using DataVisualiser.VNext.Contracts;
using LiveCharts.Wpf;

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

public enum CartesianMetricBackendKey
{
    LiveChartsWpfMain = 0,
    LiveChartsWpfNormalized = 1,
    LiveChartsWpfDiffRatio = 2
}

public sealed record CartesianMetricBackendQualification(
    CartesianMetricBackendKey BackendKey,
    string PathKey,
    CartesianMetricRenderingQualification Qualification,
    CartesianMetricChartRoute Route,
    bool SupportsRender,
    bool SupportsUpdate,
    bool SupportsHoverTooltip,
    bool SupportsResetView,
    bool SupportsClear,
    bool SupportsLifecycleSafety);

public sealed record CartesianMetricRenderingCapabilities(
    string PathKey,
    CartesianMetricRenderingQualification Qualification,
    bool SupportsRender,
    bool SupportsUpdate,
    bool SupportsHoverTooltip,
    bool SupportsResetView,
    bool SupportsClear,
    bool SupportsLifecycleSafety);

public enum CartesianMetricRenderingQualification
{
    Qualified = 0,
    TacticalFallback = 1,
    UnqualifiedDebt = 2
}

public enum CartesianMetricChartRoute
{
    Main = 0,
    Normalized = 1,
    DiffRatio = 2
}

public sealed record CartesianMetricChartRenderHost(
    CartesianChart Chart,
    ChartState ChartState);

public sealed record CartesianMetricChartRenderRequest(
    CartesianMetricChartRoute Route,
    ChartDataContext Context,
    IReadOnlyList<MetricSeriesSelection>? SelectedSeries = null,
    string? ResolutionTableName = null,
    bool IsStacked = false,
    bool IsCumulative = false,
    IReadOnlyList<SeriesResult>? OverlaySeries = null,
    CartesianMetricCapabilityContract? CapabilityContract = null);

/// <summary>
///     Binds rendering authority for a CartesianMetric chart kind.
///     Valid kinds: Main, Normalized, Difference, Ratio.
///     Note: display mode is resolved dynamically per render; only Delivery is threaded
///     into the builder to preserve correct stacked/cumulative mode resolution.
/// </summary>
public sealed record CartesianMetricCapabilityContract
{
    private static readonly IReadOnlySet<ChartProgramKind> ValidKinds = new HashSet<ChartProgramKind>
    {
        ChartProgramKind.Main,
        ChartProgramKind.Normalized,
        ChartProgramKind.Difference,
        ChartProgramKind.Ratio
    };

    public CartesianMetricCapabilityContract(
        ChartProgramRequest programRequest,
        CapabilityRequest capability,
        ConsumerDeliveryContract delivery)
    {
        ArgumentNullException.ThrowIfNull(programRequest);
        ArgumentNullException.ThrowIfNull(capability);
        ArgumentNullException.ThrowIfNull(delivery);

        if (!ValidKinds.Contains(programRequest.Kind))
            throw new ArgumentException($"CartesianMetric capability contracts must use a valid CartesianMetric program kind (Main, Normalized, Difference, Ratio). Got: {programRequest.Kind}.", nameof(programRequest));
        if (delivery.ProgramKind != programRequest.Kind)
            throw new ArgumentException("CartesianMetric delivery contract must target the same program kind as the program request.", nameof(delivery));

        ProgramRequest = programRequest;
        Capability = capability;
        Delivery = delivery;
    }

    public ChartProgramRequest ProgramRequest { get; }
    public CapabilityRequest Capability { get; }
    public ConsumerDeliveryContract Delivery { get; }

    public static CartesianMetricCapabilityContract Create(ChartProgramKind kind)
    {
        if (!new HashSet<ChartProgramKind> { ChartProgramKind.Main, ChartProgramKind.Normalized, ChartProgramKind.Difference, ChartProgramKind.Ratio }.Contains(kind))
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "Kind must be a CartesianMetric kind.");

        var programRequest = kind switch
        {
            ChartProgramKind.Main => ChartProgramRequest.MainProgram(),
            ChartProgramKind.Normalized => ChartProgramRequest.Normalized(),
            ChartProgramKind.Difference => ChartProgramRequest.Difference(),
            ChartProgramKind.Ratio => ChartProgramRequest.Ratio(),
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };

        return new CartesianMetricCapabilityContract(
            programRequest,
            CapabilityRequest.FromProgramRequest(programRequest),
            ChartProgramDeliveryTargetResolver.CreateDelivery(kind));
    }
}
