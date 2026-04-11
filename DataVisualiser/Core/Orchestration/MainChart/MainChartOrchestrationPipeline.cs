using LiveCharts.Wpf;

namespace DataVisualiser.Core.Orchestration.MainChart;

public sealed class MainChartOrchestrationPipeline : IMainChartOrchestrationPipeline
{
    private readonly IMainChartPreparationStage _preparationStage;
    private readonly IMainChartRenderInvocationStage _renderInvocationStage;
    private readonly IMainChartStrategySelectionStage _strategySelectionStage;

    public MainChartOrchestrationPipeline(
        IMainChartPreparationStage preparationStage,
        IMainChartStrategySelectionStage strategySelectionStage,
        IMainChartRenderInvocationStage renderInvocationStage)
    {
        _preparationStage = preparationStage ?? throw new ArgumentNullException(nameof(preparationStage));
        _strategySelectionStage = strategySelectionStage ?? throw new ArgumentNullException(nameof(strategySelectionStage));
        _renderInvocationStage = renderInvocationStage ?? throw new ArgumentNullException(nameof(renderInvocationStage));
    }

    public async Task<MainChartPreparedData> RenderAsync(MainChartRenderRequest request, CartesianChart chart)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        var preparedData = await _preparationStage.PrepareAsync(request);
        var strategyPlan = _strategySelectionStage.Select(preparedData);
        await _renderInvocationStage.RenderAsync(strategyPlan, chart);
        return preparedData;
    }
}
