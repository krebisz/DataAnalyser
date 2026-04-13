using DataVisualiser.Core.Orchestration;
using DataVisualiser.UI.Charts.Presentation;

namespace DataVisualiser.UI.MainHost.Coordination;

public sealed class MainChartsViewDataLoadedCoordinator
{
    public sealed class Actions(
        Action completeTransformSelectionsPendingLoad,
        Action<string> updateSubtypeOptions,
        Action updateTransformSubtypeOptions,
        Action updateTransformComputeButtonState,
        Action<int> updatePrimaryDataRequiredButtonStates,
        Action<int> updateSecondaryDataRequiredButtonStates,
        Func<string, ChartDataContext, Task> renderChartAsync,
        Func<Task> renderChartsFromLastContextAsync)
    {
        public Action CompleteTransformSelectionsPendingLoad { get; } = completeTransformSelectionsPendingLoad ?? throw new ArgumentNullException(nameof(completeTransformSelectionsPendingLoad));
        public Action<string> UpdateSubtypeOptions { get; } = updateSubtypeOptions ?? throw new ArgumentNullException(nameof(updateSubtypeOptions));
        public Action UpdateTransformSubtypeOptions { get; } = updateTransformSubtypeOptions ?? throw new ArgumentNullException(nameof(updateTransformSubtypeOptions));
        public Action UpdateTransformComputeButtonState { get; } = updateTransformComputeButtonState ?? throw new ArgumentNullException(nameof(updateTransformComputeButtonState));
        public Action<int> UpdatePrimaryDataRequiredButtonStates { get; } = updatePrimaryDataRequiredButtonStates ?? throw new ArgumentNullException(nameof(updatePrimaryDataRequiredButtonStates));
        public Action<int> UpdateSecondaryDataRequiredButtonStates { get; } = updateSecondaryDataRequiredButtonStates ?? throw new ArgumentNullException(nameof(updateSecondaryDataRequiredButtonStates));
        public Func<string, ChartDataContext, Task> RenderChartAsync { get; } = renderChartAsync ?? throw new ArgumentNullException(nameof(renderChartAsync));
        public Func<Task> RenderChartsFromLastContextAsync { get; } = renderChartsFromLastContextAsync ?? throw new ArgumentNullException(nameof(renderChartsFromLastContextAsync));
    }

    public async Task HandleAsync(ChartDataContext? context, int selectedSubtypeCount, Actions actions)
    {
        ArgumentNullException.ThrowIfNull(actions);

        if (!MainChartsViewChartUpdateCoordinator.ShouldRenderCharts(context))
            return;

        var safeContext = context!;
        actions.CompleteTransformSelectionsPendingLoad();
        actions.UpdateSubtypeOptions(ChartControllerKeys.Normalized);
        actions.UpdateSubtypeOptions(ChartControllerKeys.DiffRatio);
        actions.UpdateSubtypeOptions(ChartControllerKeys.Main);
        actions.UpdateTransformSubtypeOptions();
        actions.UpdateTransformComputeButtonState();
        actions.UpdatePrimaryDataRequiredButtonStates(selectedSubtypeCount);
        actions.UpdateSecondaryDataRequiredButtonStates(selectedSubtypeCount);

        await actions.RenderChartAsync(ChartControllerKeys.BarPie, safeContext);
        await actions.RenderChartsFromLastContextAsync();
    }
}
