using DataVisualiser.Core.Rendering.CartesianMetrics;
using DataVisualiser.VNext.Contracts;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Orchestration.MainChart;

public sealed class MainChartRenderInvocationStage : IMainChartRenderInvocationStage
{
    private readonly ChartUpdateCoordinator _chartUpdateCoordinator;

    public MainChartRenderInvocationStage(ChartUpdateCoordinator chartUpdateCoordinator)
    {
        _chartUpdateCoordinator = chartUpdateCoordinator ?? throw new ArgumentNullException(nameof(chartUpdateCoordinator));
    }

    public Task RenderAsync(MainChartStrategyPlan plan, CartesianChart chart, CartesianMetricCapabilityContract? capabilityContract = null)
    {
        if (plan == null)
            throw new ArgumentNullException(nameof(plan));
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        var context = plan.WorkingContext;
        return _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(
            chart,
            plan.Strategy,
            new ChartUpdateRequest
            {
                PrimaryLabel = plan.PrimaryLabel,
                SecondaryLabel = plan.SecondaryLabel,
                MetricType = context.MetricType,
                PrimarySubtype = context.PrimarySubtype,
                SecondarySubtype = plan.SecondaryLabel != null ? context.SecondarySubtype : null,
                SecondaryMetricType = context.SecondaryMetricType,
                DisplayPrimaryMetricType = context.DisplayPrimaryMetricType,
                DisplaySecondaryMetricType = context.DisplaySecondaryMetricType,
                DisplayPrimarySubtype = context.DisplayPrimarySubtype,
                DisplaySecondarySubtype = context.DisplaySecondarySubtype,
                IsStacked = plan.IsStacked,
                IsCumulative = plan.IsCumulative,
                OverlaySeries = plan.OverlaySeries,
                RenderProgramKind = ChartProgramKind.Main,
                RenderDelivery = capabilityContract?.Delivery,
                RenderConsumptionContractFactory = renderPlan => CartesianMetricVNextConsumptionContractBuilder.Build(
                    renderPlan,
                    capabilityContract?.Delivery,
                    new Dictionary<string, string>
                    {
                        ["CartesianMetric.Route"] = "Main",
                        ["CartesianMetric.IsStacked"] = plan.IsStacked.ToString(),
                        ["CartesianMetric.IsCumulative"] = plan.IsCumulative.ToString(),
                        ["CartesianMetric.HasOverlaySeries"] = (plan.OverlaySeries != null && plan.OverlaySeries.Count > 0).ToString()
                    })
            });
    }
}
