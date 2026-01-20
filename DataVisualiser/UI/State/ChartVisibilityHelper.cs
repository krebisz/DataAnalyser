using DataVisualiser.UI.Controls;

namespace DataVisualiser.UI.State;

public static class ChartVisibilityHelper
{
    public static IReadOnlyList<string> GetHiddenChartKeys(ChartState state)
    {
        if (state == null)
            throw new ArgumentNullException(nameof(state));

        var hidden = new List<string>();

        if (!state.IsMainVisible)
            hidden.Add(ChartControllerKeys.Main);

        if (!state.IsNormalizedVisible)
            hidden.Add(ChartControllerKeys.Normalized);

        if (!state.IsDiffRatioVisible)
            hidden.Add(ChartControllerKeys.DiffRatio);

        if (!state.IsDistributionVisible)
            hidden.Add(ChartControllerKeys.Distribution);

        if (!state.IsWeeklyTrendVisible)
            hidden.Add(ChartControllerKeys.WeeklyTrend);

        if (!state.IsTransformPanelVisible)
            hidden.Add(ChartControllerKeys.Transform);

        if (!state.IsBarPieVisible)
            hidden.Add(ChartControllerKeys.BarPie);

        return hidden;
    }
}
