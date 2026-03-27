using System.Windows.Controls;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Helpers;

namespace DataVisualiser.UI.Charts.Infrastructure;

internal static class MetricSeriesSelectionAdapterHelper
{
    public static MetricSeriesSelection? PopulateSubtypeCombo(ComboBox combo, IReadOnlyList<MetricSeriesSelection> selectedSeries, MetricSeriesSelection? currentSelection)
    {
        if (selectedSeries == null || selectedSeries.Count == 0)
        {
            ChartSubtypeComboHelper.DisableCombo(combo);
            return null;
        }

        ChartSubtypeComboHelper.PopulateCombo(combo, selectedSeries);
        var resolvedSelection = ChartSubtypeComboHelper.ResolveSelection(selectedSeries, currentSelection) ?? selectedSeries[0];
        ChartSubtypeComboHelper.SelectComboItem(combo, resolvedSelection);
        return resolvedSelection;
    }

    public static MetricSeriesSelection? ResolveSelectedSeries(bool allowComboSelection, ComboBox? combo, MetricSeriesSelection? stateSelection, ChartDataContext ctx)
    {
        return MetricSeriesSelectionCache.ResolveSelection(allowComboSelection,
            combo,
            stateSelection,
            () =>
            {
                var metricType = ctx.PrimaryMetricType ?? ctx.MetricType;
                if (string.IsNullOrWhiteSpace(metricType))
                    return null;

                return new MetricSeriesSelection(metricType, ctx.PrimarySubtype);
            });
    }

    public static string ResolveDisplayName(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
    {
        if (selectedSeries == null)
            return ctx.DisplayName1;

        if (MetricSeriesSelectionCache.IsSameSelection(selectedSeries, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return ctx.DisplayName1;

        if (MetricSeriesSelectionCache.IsSameSelection(selectedSeries, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return ctx.DisplayName2;

        return selectedSeries.DisplayName;
    }
}
