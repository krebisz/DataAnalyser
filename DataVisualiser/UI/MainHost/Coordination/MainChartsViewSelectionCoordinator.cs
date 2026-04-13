using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Presentation;

namespace DataVisualiser.UI.MainHost.Coordination;

public sealed class MainChartsViewSelectionCoordinator
{
    public sealed record Actions(
        Action<IReadOnlyList<MetricSeriesSelection>> SetSelectedSeries,
        Action<string> UpdateSubtypeOptions,
        Action UpdateTransformSubtypeOptions,
        Action<int> UpdatePrimaryDataRequiredButtonStates,
        Action<int> UpdateSecondaryDataRequiredButtonStates,
        Action UpdateChartTitlesFromSelections,
        Func<Task> RenderChartsFromLastContextAsync,
        Func<Task> LoadDateRangeForSelectedMetricsAsync);

    public void UpdateSelectedSubtypes(IReadOnlyList<MetricSeriesSelection> selectedSeries, Actions actions)
    {
        ArgumentNullException.ThrowIfNull(selectedSeries);
        ArgumentNullException.ThrowIfNull(actions);

        var selectedSubtypeCount = CountSelectedSubtypes(selectedSeries);

        actions.SetSelectedSeries(selectedSeries);
        actions.UpdateSubtypeOptions(ChartControllerKeys.Normalized);
        actions.UpdateSubtypeOptions(ChartControllerKeys.DiffRatio);
        actions.UpdateSubtypeOptions(ChartControllerKeys.Distribution);
        actions.UpdateSubtypeOptions(ChartControllerKeys.WeeklyTrend);
        actions.UpdateSubtypeOptions(ChartControllerKeys.Main);
        actions.UpdateTransformSubtypeOptions();
        actions.UpdatePrimaryDataRequiredButtonStates(selectedSubtypeCount);
        actions.UpdateSecondaryDataRequiredButtonStates(selectedSubtypeCount);
    }

    public async Task HandleSubtypeSelectionChangedAsync(bool hasLoadedData, bool shouldRefreshDateRangeForCurrentSelection, Actions actions)
    {
        ArgumentNullException.ThrowIfNull(actions);

        if (hasLoadedData)
        {
            actions.UpdateChartTitlesFromSelections();
            await actions.RenderChartsFromLastContextAsync();
            return;
        }

        if (shouldRefreshDateRangeForCurrentSelection)
            await actions.LoadDateRangeForSelectedMetricsAsync();
    }

    private static int CountSelectedSubtypes(IEnumerable<MetricSeriesSelection> selections)
    {
        return selections.Count(selection => selection.QuerySubtype != null);
    }
}
