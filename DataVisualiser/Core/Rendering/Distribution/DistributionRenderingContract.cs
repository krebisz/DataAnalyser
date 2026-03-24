using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Rendering.Interaction;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Helpers;
using DataVisualiser.UI.State;

namespace DataVisualiser.Core.Rendering.Distribution;

public sealed class DistributionRenderingContract : IDistributionRenderingContract
{
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

    public DistributionRenderingCapabilities GetCapabilities(DistributionRenderingRoute route)
    {
        return route switch
        {
            DistributionRenderingRoute.PolarFallback => new DistributionRenderingCapabilities(
                "Distribution.PolarFallback.LiveChartsWpfProjection",
                DistributionRenderingQualification.TacticalFallback,
                SupportsRender: true,
                SupportsUpdate: true,
                SupportsHoverTooltip: true,
                SupportsResetView: true,
                SupportsClear: true,
                SupportsLifecycleSafety: true),
            _ => new DistributionRenderingCapabilities(
                "Distribution.Cartesian.LiveChartsWpf",
                DistributionRenderingQualification.Qualified,
                SupportsRender: true,
                SupportsUpdate: true,
                SupportsHoverTooltip: true,
                SupportsResetView: true,
                SupportsClear: true,
                SupportsLifecycleSafety: true)
        };
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
            default:
                await RenderCartesianAsync(request, host);
                break;
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

        ChartSurfaceHelper.ResetZoom(host.CartesianChart);
    }

    public bool HasRenderableContent(DistributionRenderingRoute route, DistributionChartRenderHost host)
    {
        if (host == null)
            throw new ArgumentNullException(nameof(host));

        return route switch
        {
            DistributionRenderingRoute.PolarFallback => ChartSurfaceHelper.HasSeries(host.CartesianChart) || ChartSurfaceHelper.HasSeries(host.PolarChart),
            _ => ChartSurfaceHelper.HasSeries(host.CartesianChart)
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
