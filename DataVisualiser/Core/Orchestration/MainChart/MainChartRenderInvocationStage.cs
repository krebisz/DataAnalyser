using LiveCharts.Wpf;

namespace DataVisualiser.Core.Orchestration.MainChart;

public sealed class MainChartRenderInvocationStage : IMainChartRenderInvocationStage
{
    private readonly ChartUpdateCoordinator _chartUpdateCoordinator;

    public MainChartRenderInvocationStage(ChartUpdateCoordinator chartUpdateCoordinator)
    {
        _chartUpdateCoordinator = chartUpdateCoordinator ?? throw new ArgumentNullException(nameof(chartUpdateCoordinator));
    }

    public Task RenderAsync(MainChartStrategyPlan plan, CartesianChart chart)
    {
        if (plan == null)
            throw new ArgumentNullException(nameof(plan));
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        var context = plan.WorkingContext;
        return _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(
            chart,
            plan.Strategy,
            plan.PrimaryLabel,
            plan.SecondaryLabel,
            400,
            context.MetricType,
            context.PrimarySubtype,
            plan.SecondaryLabel != null ? context.SecondarySubtype : null,
            isOperationChart: false,
            secondaryMetricType: context.SecondaryMetricType,
            displayPrimaryMetricType: context.DisplayPrimaryMetricType,
            displaySecondaryMetricType: context.DisplaySecondaryMetricType,
            displayPrimarySubtype: context.DisplayPrimarySubtype,
            displaySecondarySubtype: context.DisplaySecondarySubtype,
            isStacked: plan.IsStacked,
            isCumulative: plan.IsCumulative,
            overlaySeries: plan.OverlaySeries);
    }
}
