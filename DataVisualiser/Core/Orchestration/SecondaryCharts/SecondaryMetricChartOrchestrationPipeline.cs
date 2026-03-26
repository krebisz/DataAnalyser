using System.Diagnostics;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Services.Abstractions;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Orchestration.SecondaryCharts;

public sealed class SecondaryMetricChartOrchestrationPipeline : ISecondaryMetricChartOrchestrationPipeline
{
    private readonly IUserNotificationService _notificationService;
    private readonly ISecondaryMetricChartRenderInvocationStage _renderInvocationStage;
    private readonly ISecondaryMetricChartStrategySelectionStage _strategySelectionStage;

    public SecondaryMetricChartOrchestrationPipeline(
        ISecondaryMetricChartStrategySelectionStage strategySelectionStage,
        ISecondaryMetricChartRenderInvocationStage renderInvocationStage,
        IUserNotificationService notificationService)
    {
        _strategySelectionStage = strategySelectionStage ?? throw new ArgumentNullException(nameof(strategySelectionStage));
        _renderInvocationStage = renderInvocationStage ?? throw new ArgumentNullException(nameof(renderInvocationStage));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    public async Task RenderAsync(SecondaryMetricChartRenderRequest request, CartesianChart chart)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        if (request.Context.Data1 == null || request.Context.Data2 == null)
        {
            ChartHelper.ClearChart(chart, request.ChartState.ChartTimestamps);
            return;
        }

        try
        {
            var plan = _strategySelectionStage.Select(request);
            await _renderInvocationStage.RenderAsync(plan, request, chart);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SecondaryMetricChartPipeline] {request.Route} failed: {ex}");
            _notificationService.ShowError("Chart Error", $"Error rendering {request.Route} chart: {ex.Message}");
            ChartHelper.ClearChart(chart, request.ChartState.ChartTimestamps);
        }
    }
}
