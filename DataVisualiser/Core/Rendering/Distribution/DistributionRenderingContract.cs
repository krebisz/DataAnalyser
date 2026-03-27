using System.Windows.Controls;
using DataFileReader.Canonical;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Rendering.Interaction;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Helpers;
using DataVisualiser.UI.State;
using LiveChartsCore.SkiaSharpView.WPF;
using CartesianChart = LiveCharts.Wpf.CartesianChart;

namespace DataVisualiser.Core.Rendering.Distribution;

public sealed class DistributionRenderingContract : IDistributionRenderingContract
{
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
    private readonly IDistributionService _weeklyDistributionService;

    public DistributionRenderingContract(
        Func<ChartRenderingOrchestrator?> getChartRenderingOrchestrator,
        IDistributionService weeklyDistributionService,
        IDistributionService hourlyDistributionService,
        DistributionPolarRenderingService polarRenderingService)
    {
        _getChartRenderingOrchestrator = getChartRenderingOrchestrator ?? throw new ArgumentNullException(nameof(getChartRenderingOrchestrator));
        _weeklyDistributionService = weeklyDistributionService ?? throw new ArgumentNullException(nameof(weeklyDistributionService));
        _hourlyDistributionService = hourlyDistributionService ?? throw new ArgumentNullException(nameof(hourlyDistributionService));
        _polarRenderingService = polarRenderingService ?? throw new ArgumentNullException(nameof(polarRenderingService));
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

    public async Task RenderAsync(DistributionChartRenderRequest request, DistributionChartRenderHost host)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        if (host == null)
            throw new ArgumentNullException(nameof(host));

        DisposeDistributionInteractions(host);

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

        if (route == DistributionRenderingRoute.PolarFallback && host.CartesianChart.Tag is DistributionPolarProjectionTooltip)
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
        host.CartesianChart.Tag = new DistributionPolarProjectionTooltip(host.CartesianChart, definition, rangeResult);
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
    ChartState ChartState);

public sealed record DistributionChartRenderHost(
    CartesianChart CartesianChart,
    PolarChart PolarChart,
    ChartState ChartState,
    Func<ToolTip?> GetPolarTooltip);
