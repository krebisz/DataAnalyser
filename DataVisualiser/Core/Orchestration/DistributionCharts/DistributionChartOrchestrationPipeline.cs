using System.Diagnostics;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Services.Abstractions;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Orchestration.DistributionCharts;

public sealed class DistributionChartOrchestrationPipeline : IDistributionChartOrchestrationPipeline
{
    private readonly IUserNotificationService _notificationService;
    private readonly IDistributionChartPreparationStage _preparationStage;
    private readonly IDistributionChartRenderInvocationStage _renderInvocationStage;

    public DistributionChartOrchestrationPipeline(
        IDistributionChartPreparationStage preparationStage,
        IDistributionChartRenderInvocationStage renderInvocationStage,
        IUserNotificationService? notificationService = null)
    {
        _preparationStage = preparationStage ?? throw new ArgumentNullException(nameof(preparationStage));
        _renderInvocationStage = renderInvocationStage ?? throw new ArgumentNullException(nameof(renderInvocationStage));
        _notificationService = notificationService ?? MessageBoxUserNotificationService.Instance;
    }

    public async Task RenderAsync(DistributionChartOrchestrationRequest request, CartesianChart chart)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        if (request.Context.Data1 == null || request.Context.Data1.Count == 0)
        {
            ChartHelper.ClearChart(chart, request.ChartState.ChartTimestamps);
            return;
        }

        try
        {
            var preparedData = _preparationStage.Prepare(request);
            await _renderInvocationStage.RenderAsync(preparedData, chart);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DistributionChartPipeline] {request.Mode} failed: {ex}");
            _notificationService.ShowError("Chart Error", $"Error rendering distribution chart: {ex.Message}");
            ChartHelper.ClearChart(chart, request.ChartState.ChartTimestamps);
        }
    }
}
