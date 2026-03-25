using DataVisualiser.Core.Strategies;
using DataVisualiser.Core.Strategies.Abstractions;

namespace DataVisualiser.Core.Orchestration.SecondaryCharts;

public sealed class SecondaryMetricChartStrategySelectionStage : ISecondaryMetricChartStrategySelectionStage
{
    private readonly IStrategyCutOverService _strategyCutOverService;

    public SecondaryMetricChartStrategySelectionStage(IStrategyCutOverService strategyCutOverService)
    {
        _strategyCutOverService = strategyCutOverService ?? throw new ArgumentNullException(nameof(strategyCutOverService));
    }

    public SecondaryMetricChartStrategyPlan Select(SecondaryMetricChartRenderRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var context = request.Context;
        if (context.Data1 == null || context.Data2 == null)
            throw new InvalidOperationException("Secondary metric charts require both primary and secondary series.");

        var strategyType = request.Route switch
        {
            SecondaryMetricChartRoute.Normalized => StrategyType.Normalized,
            SecondaryMetricChartRoute.Difference => StrategyType.Difference,
            SecondaryMetricChartRoute.Ratio => StrategyType.Ratio,
            _ => throw new ArgumentOutOfRangeException(nameof(request.Route), request.Route, "Unknown secondary metric chart route.")
        };

        var parameters = new StrategyCreationParameters
        {
            LegacyData1 = context.Data1,
            LegacyData2 = context.Data2,
            Label1 = context.DisplayName1,
            Label2 = context.DisplayName2,
            From = context.From,
            To = context.To,
            NormalizationMode = request.Route == SecondaryMetricChartRoute.Normalized
                ? request.ChartState.SelectedNormalizationMode
                : null
        };

        var strategy = _strategyCutOverService.CreateStrategy(strategyType, context, parameters);
        var operationType = request.Route switch
        {
            SecondaryMetricChartRoute.Normalized => "~",
            SecondaryMetricChartRoute.Difference => "-",
            SecondaryMetricChartRoute.Ratio => "/",
            _ => throw new ArgumentOutOfRangeException(nameof(request.Route), request.Route, "Unknown secondary metric chart route.")
        };

        return new SecondaryMetricChartStrategyPlan(
            strategy,
            context.DisplayName1,
            context.DisplayName2,
            operationType);
    }
}
