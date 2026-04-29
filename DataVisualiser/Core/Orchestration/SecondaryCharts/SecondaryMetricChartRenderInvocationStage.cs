using DataVisualiser.Core.Rendering.CartesianMetrics;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Orchestration.SecondaryCharts;

public sealed class SecondaryMetricChartRenderInvocationStage : ISecondaryMetricChartRenderInvocationStage
{
    private readonly ChartUpdateCoordinator _chartUpdateCoordinator;

    public SecondaryMetricChartRenderInvocationStage(ChartUpdateCoordinator chartUpdateCoordinator)
    {
        _chartUpdateCoordinator = chartUpdateCoordinator ?? throw new ArgumentNullException(nameof(chartUpdateCoordinator));
    }

    public Task RenderAsync(SecondaryMetricChartStrategyPlan plan, SecondaryMetricChartRenderRequest request, CartesianChart chart)
    {
        if (plan == null)
            throw new ArgumentNullException(nameof(plan));
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        var context = request.Context;
        var programKind = request.Route switch
        {
            SecondaryMetricChartRoute.Normalized => DataVisualiser.VNext.Contracts.ChartProgramKind.Normalized,
            SecondaryMetricChartRoute.Difference => DataVisualiser.VNext.Contracts.ChartProgramKind.Difference,
            SecondaryMetricChartRoute.Ratio => DataVisualiser.VNext.Contracts.ChartProgramKind.Ratio,
            _ => throw new ArgumentOutOfRangeException(nameof(request.Route), request.Route, "Unknown secondary metric chart route.")
        };

        var capabilityContract = CartesianMetricCapabilityContract.Create(programKind);

        return _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(
            chart,
            plan.Strategy,
            plan.PrimaryLabel,
            plan.SecondaryLabel,
            minHeight: 400,
            metricType: context.MetricType,
            primarySubtype: context.PrimarySubtype,
            secondarySubtype: context.SecondarySubtype,
            operationType: plan.OperationType,
            isOperationChart: plan.IsOperationChart,
            secondaryMetricType: context.SecondaryMetricType,
            displayPrimaryMetricType: context.DisplayPrimaryMetricType,
            displaySecondaryMetricType: context.DisplaySecondaryMetricType,
            displayPrimarySubtype: context.DisplayPrimarySubtype,
            displaySecondarySubtype: context.DisplaySecondarySubtype,
            useRenderPlanAdapter: true,
            renderProgramKind: programKind,
            renderDelivery: capabilityContract.Delivery);
    }
}
