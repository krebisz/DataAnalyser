using System.Windows.Controls;
using DataFileReader.Canonical;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Rendering.Interaction;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.State;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Rendering;
using LiveChartsCore.SkiaSharpView.WPF;
using CartesianChart = LiveCharts.Wpf.CartesianChart;

namespace DataVisualiser.Core.Rendering.Distribution;

public sealed class DistributionRenderingContract : IDistributionRenderingContract
{
    private readonly ChartRenderPlanAdapterDispatcher<DistributionRenderSurface> _dispatcher;
    private static readonly IReadOnlyList<DistributionBackendQualification> QualificationMatrix =
    [
        new DistributionBackendQualification(
            DistributionBackendKey.LiveChartsWpfCartesian,
            "Distribution.Cartesian.LiveChartsWpf",
            DistributionRenderingQualification.Qualified,
            DistributionRenderingRoute.Cartesian,
            SupportsRender: true,
            SupportsUpdate: true,
            SupportsHoverTooltip: true,
            SupportsResetView: true,
            SupportsClear: true,
            SupportsLifecycleSafety: true),
        new DistributionBackendQualification(
            DistributionBackendKey.LiveChartsWpfPolarFallbackProjection,
            "Distribution.PolarFallback.LiveChartsWpfProjection",
            DistributionRenderingQualification.TacticalFallback,
            DistributionRenderingRoute.PolarFallback,
            SupportsRender: true,
            SupportsUpdate: true,
            SupportsHoverTooltip: true,
            SupportsResetView: true,
            SupportsClear: true,
            SupportsLifecycleSafety: true),
        new DistributionBackendQualification(
            DistributionBackendKey.LiveChartsCorePolar,
            "Distribution.Polar.LiveChartsCore",
            DistributionRenderingQualification.UnqualifiedDebt,
            ActiveRoute: null,
            SupportsRender: false,
            SupportsUpdate: false,
            SupportsHoverTooltip: false,
            SupportsResetView: false,
            SupportsClear: false,
            SupportsLifecycleSafety: false)
    ];

    private readonly Func<ChartRenderingOrchestrator?> _getChartRenderingOrchestrator;
    private readonly IDistributionService _hourlyDistributionService;
    private readonly DistributionPolarRenderingService _polarRenderingService;
    private readonly IDistributionPolarProjectionInteractionFactory? _polarProjectionInteractionFactory;
    private readonly IDistributionService _weeklyDistributionService;

    public DistributionRenderingContract(
        Func<ChartRenderingOrchestrator?> getChartRenderingOrchestrator,
        IDistributionService weeklyDistributionService,
        IDistributionService hourlyDistributionService,
        DistributionPolarRenderingService polarRenderingService,
        IDistributionPolarProjectionInteractionFactory? polarProjectionInteractionFactory = null,
        ChartRenderPlanAdapterDispatcher<DistributionRenderSurface>? dispatcher = null)
    {
        _getChartRenderingOrchestrator = getChartRenderingOrchestrator ?? throw new ArgumentNullException(nameof(getChartRenderingOrchestrator));
        _weeklyDistributionService = weeklyDistributionService ?? throw new ArgumentNullException(nameof(weeklyDistributionService));
        _hourlyDistributionService = hourlyDistributionService ?? throw new ArgumentNullException(nameof(hourlyDistributionService));
        _polarRenderingService = polarRenderingService ?? throw new ArgumentNullException(nameof(polarRenderingService));
        _polarProjectionInteractionFactory = polarProjectionInteractionFactory;
        _dispatcher = dispatcher
            ?? new ChartRenderPlanAdapterDispatcher<DistributionRenderSurface>([new DistributionRenderPlanAdapter(RenderCoreAsync)]);
    }

    public IReadOnlyList<DistributionBackendQualification> GetBackendQualificationMatrix()
    {
        return QualificationMatrix;
    }

    public DistributionRenderingCapabilities GetCapabilities(DistributionRenderingRoute route)
    {
        var qualification = QualificationMatrix.FirstOrDefault(entry => entry.ActiveRoute == route);
        if (qualification == null)
            throw new ArgumentOutOfRangeException(nameof(route), route, "Unknown distribution rendering route.");

        return new DistributionRenderingCapabilities(
            qualification.PathKey,
            qualification.Qualification,
            qualification.SupportsRender,
            qualification.SupportsUpdate,
            qualification.SupportsHoverTooltip,
            qualification.SupportsResetView,
            qualification.SupportsClear,
            qualification.SupportsLifecycleSafety);
    }

    public async Task<ChartRenderAdapterResult> RenderAsync(DistributionChartRenderRequest request, DistributionChartRenderHost host)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        if (host == null)
            throw new ArgumentNullException(nameof(host));

        DisposeDistributionInteractions(host);
        var plan = DistributionRenderPlanBuilder.Build(request);
        return await _dispatcher.ApplyAsync(new DistributionRenderSurface(request, host), plan);
    }

    private async Task RenderCoreAsync(DistributionChartRenderRequest request, DistributionChartRenderHost host)
    {
        switch (request.Route)
        {
            case DistributionRenderingRoute.PolarFallback:
                await RenderPolarFallbackAsync(request, host);
                break;
            case DistributionRenderingRoute.Cartesian:
                await RenderCartesianAsync(request, host);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(request.Route), request.Route, "Unknown distribution rendering route.");
        }
    }

    public void Clear(DistributionChartRenderHost host)
    {
        if (host == null)
            throw new ArgumentNullException(nameof(host));

        DisposeDistributionInteractions(host);
        ChartSurfaceHelper.ClearCartesian(host.CartesianChart, host.ChartState);
        ChartSurfaceHelper.ClearPolar(host.PolarChart, host.GetPolarTooltip);
    }

    public void ResetView(DistributionRenderingRoute route, DistributionChartRenderHost host)
    {
        if (host == null)
            throw new ArgumentNullException(nameof(host));

        if (route == DistributionRenderingRoute.PolarFallback && host.CartesianChart.Tag is IDistributionPolarProjectionInteraction)
        {
            _polarRenderingService.RefitPolarProjection(host.CartesianChart);
            return;
        }

        if (route == DistributionRenderingRoute.Cartesian)
        {
            ChartSurfaceHelper.ResetZoom(host.CartesianChart);
            return;
        }

        throw new ArgumentOutOfRangeException(nameof(route), route, "Unknown distribution rendering route.");
    }

    public bool HasRenderableContent(DistributionRenderingRoute route, DistributionChartRenderHost host)
    {
        if (host == null)
            throw new ArgumentNullException(nameof(host));

        return route switch
        {
            DistributionRenderingRoute.PolarFallback => ChartSurfaceHelper.HasSeries(host.CartesianChart) || ChartSurfaceHelper.HasSeries(host.PolarChart),
            DistributionRenderingRoute.Cartesian => ChartSurfaceHelper.HasSeries(host.CartesianChart),
            _ => throw new ArgumentOutOfRangeException(nameof(route), route, "Unknown distribution rendering route.")
        };
    }

    private async Task RenderCartesianAsync(DistributionChartRenderRequest request, DistributionChartRenderHost host)
    {
        var orchestrator = _getChartRenderingOrchestrator();
        if (orchestrator != null)
        {
            await orchestrator.RenderDistributionChartAsync(request.RenderingContext, host.CartesianChart, request.ChartState, request.Mode);
            return;
        }

        var service = GetDistributionService(request.Mode);
        await service.UpdateDistributionChartAsync(
            host.CartesianChart,
            request.Data,
            request.DisplayName,
            request.From,
            request.To,
            400,
            request.Settings.UseFrequencyShading,
            request.Settings.IntervalCount,
            request.CmsSeries);
    }

    private async Task RenderPolarFallbackAsync(DistributionChartRenderRequest request, DistributionChartRenderHost host)
    {
        var service = GetDistributionService(request.Mode);
        var rangeResult = await service.ComputeSimpleRangeAsync(request.Data, request.DisplayName, request.From, request.To, request.CmsSeries);
        if (rangeResult == null)
            return;

        var definition = DistributionModeCatalog.Get(request.Mode);
        _polarRenderingService.RenderPolarChart(rangeResult, definition, host.CartesianChart);
        host.CartesianChart.Tag = _polarProjectionInteractionFactory?.Create(host.CartesianChart, definition, rangeResult);
        host.PolarChart.Tag = null;
    }

    private void DisposeDistributionInteractions(DistributionChartRenderHost host)
    {
        if (host.CartesianChart.Tag is IDisposable disposable)
            disposable.Dispose();

        host.CartesianChart.Tag = null;
        host.CartesianChart.DataTooltip = null;

        var tooltip = host.GetPolarTooltip();
        if (tooltip != null)
            tooltip.IsOpen = false;
    }

    private IDistributionService GetDistributionService(DistributionMode mode)
    {
        return mode switch
        {
            DistributionMode.Weekly => _weeklyDistributionService,
            DistributionMode.Hourly => _hourlyDistributionService,
            _ => _weeklyDistributionService
        };
    }
}

public static class DistributionBackendKey
{
    public const string LiveChartsWpfCartesian = "LiveChartsWpf.Cartesian";
    public const string LiveChartsWpfPolarFallbackProjection = "LiveChartsWpf.PolarFallbackProjection";
    public const string LiveChartsCorePolar = "LiveChartsCore.Polar";
}

public sealed record DistributionBackendQualification(
    string BackendKey,
    string PathKey,
    DistributionRenderingQualification Qualification,
    DistributionRenderingRoute? ActiveRoute,
    bool SupportsRender,
    bool SupportsUpdate,
    bool SupportsHoverTooltip,
    bool SupportsResetView,
    bool SupportsClear,
    bool SupportsLifecycleSafety);

public sealed record DistributionRenderingCapabilities(
    string PathKey,
    DistributionRenderingQualification Qualification,
    bool SupportsRender,
    bool SupportsUpdate,
    bool SupportsHoverTooltip,
    bool SupportsResetView,
    bool SupportsClear,
    bool SupportsLifecycleSafety);

public enum DistributionRenderingQualification
{
    Qualified = 0,
    TacticalFallback = 1,
    UnqualifiedDebt = 2
}

public enum DistributionRenderingRoute
{
    Cartesian = 0,
    PolarFallback = 1
}

public static class DistributionRenderingRouteResolver
{
    public static DistributionRenderingRoute Resolve(bool isPolarMode)
    {
        return isPolarMode
            ? DistributionRenderingRoute.PolarFallback
            : DistributionRenderingRoute.Cartesian;
    }
}

public sealed record DistributionChartRenderRequest(
    DistributionRenderingRoute Route,
    DistributionMode Mode,
    DistributionModeSettings Settings,
    IReadOnlyList<MetricData> Data,
    string DisplayName,
    DateTime From,
    DateTime To,
    ICanonicalMetricSeries? CmsSeries,
    ChartDataContext RenderingContext,
    ChartState ChartState,
    string SelectionDisplayKey = "<none>");

public sealed record DistributionChartRenderHost(
    CartesianChart CartesianChart,
    PolarChart PolarChart,
    ChartState ChartState,
    Func<ToolTip?> GetPolarTooltip);

public sealed record DistributionRenderSurface(
    DistributionChartRenderRequest Request,
    DistributionChartRenderHost Host);

public static class DistributionRenderPlanBuilder
{
    public static ChartRenderPlan Build(DistributionChartRenderRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var backendKey = ResolveBackendKey(request.Route);
        var sourceSignature = $"{request.Mode}:{request.DisplayName}:{request.From:O}:{request.To:O}:{request.Settings.IntervalCount}:{request.Data.Count}";
        var metadata = new Dictionary<string, string>
        {
            ["Adapter"] = nameof(DistributionRenderPlanAdapter),
            [ChartRenderPlanMetadataKeys.BackendKey] = backendKey,
            ["ProgramKind"] = ChartProgramKind.Distribution.ToString(),
            ["Route"] = request.Route.ToString(),
            ["Mode"] = request.Mode.ToString(),
            ["Selection"] = request.SelectionDisplayKey
        };
        ChartRenderPlanVocabularyMetadata.AddTo(
            metadata,
            ChartProgramKind.Distribution,
            sourceSignature,
            deliveryTarget: "DistributionChart");

        return new ChartRenderPlan(
            $"{backendKey}:{request.Mode}:{request.DisplayName}:{request.From:O}:{request.To:O}:{request.Settings.IntervalCount}",
            ChartProgramKind.Distribution,
            ChartRenderPlanKind.Cartesian,
            ChartDisplayMode.Regular,
            request.DisplayName,
            request.From,
            request.To,
            sourceSignature,
            Array.Empty<ChartSeriesPlan>(),
            Array.Empty<ChartHierarchyNodePlan>(),
            new RenderDensityPlan(
                ChartRenderDensityMode.FullFidelity,
                request.Data.Count,
                request.Data.Count,
                request.Settings.IntervalCount),
            new ChartInteractionPlan(
                SupportsZoom: true,
                SupportsPan: true,
                SupportsTooltips: true,
                SupportsSelection: true,
                SupportsViewportRefinement: false),
            metadata);
    }

    private static string ResolveBackendKey(DistributionRenderingRoute route)
    {
        return route == DistributionRenderingRoute.PolarFallback
            ? DistributionBackendKey.LiveChartsWpfPolarFallbackProjection
            : DistributionBackendKey.LiveChartsWpfCartesian;
    }
}
