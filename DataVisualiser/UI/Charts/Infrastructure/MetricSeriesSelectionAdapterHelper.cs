using System.Windows;
using System.Windows.Controls;
using DataFileReader.Canonical;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services;
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
        return ResolveSelectedSeries(allowComboSelection, combo, stateSelection, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype);
    }

    public static MetricSeriesSelection? ResolveSelectedSeries(bool allowComboSelection, ComboBox? combo, MetricSeriesSelection? stateSelection, string? metricType, string? subtype)
    {
        return MetricSeriesSelectionCache.ResolveSelection(allowComboSelection,
            combo,
            stateSelection,
            () => BuildFallbackSelection(metricType, subtype));
    }

    public static void PopulatePrimarySecondarySubtypeCombos(
        ComboBox primaryCombo,
        ComboBox secondaryCombo,
        FrameworkElement secondaryPanel,
        IReadOnlyList<MetricSeriesSelection> selectedSeries,
        MetricSeriesSelection? currentPrimarySelection,
        MetricSeriesSelection? currentSecondarySelection,
        Action<MetricSeriesSelection?> assignPrimarySelection,
        Action<MetricSeriesSelection?> assignSecondarySelection)
    {
        if (selectedSeries.Count == 0)
        {
            primaryCombo.IsEnabled = false;
            secondaryCombo.IsEnabled = false;
            secondaryPanel.Visibility = Visibility.Collapsed;
            primaryCombo.SelectedItem = null;
            secondaryCombo.SelectedItem = null;
            assignPrimarySelection(null);
            assignSecondarySelection(null);
            return;
        }

        ChartSubtypeComboHelper.PopulateCombo(primaryCombo, selectedSeries);
        ChartSubtypeComboHelper.PopulateCombo(secondaryCombo, selectedSeries);

        var primarySelection = ChartSubtypeComboHelper.ResolveSelection(selectedSeries, currentPrimarySelection) ?? selectedSeries[0];
        ChartSubtypeComboHelper.SelectComboItem(primaryCombo, primarySelection);
        assignPrimarySelection(primarySelection);

        if (selectedSeries.Count > 1)
        {
            secondaryPanel.Visibility = Visibility.Visible;
            secondaryCombo.IsEnabled = true;

            var secondarySelection = currentSecondarySelection != null && selectedSeries.Any(series => string.Equals(series.DisplayKey, currentSecondarySelection.DisplayKey, StringComparison.OrdinalIgnoreCase))
                    ? currentSecondarySelection
                    : selectedSeries[1];

            ChartSubtypeComboHelper.SelectComboItem(secondaryCombo, secondarySelection);
            assignSecondarySelection(secondarySelection);
            return;
        }

        secondaryPanel.Visibility = Visibility.Collapsed;
        secondaryCombo.IsEnabled = false;
        secondaryCombo.SelectedItem = null;
        assignSecondarySelection(null);
    }

    public static async Task<(IReadOnlyList<MetricData>? Data, ICanonicalMetricSeries? Cms)> ResolveSeriesAsync(
        ChartDataContext ctx,
        MetricSeriesSelection? selectedSeries,
        MetricSeriesSelectionCache selectionCache,
        MetricSelectionService metricSelectionService,
        string? resolutionTableName,
        Func<ChartDataContext, (IReadOnlyList<MetricData>? Data, ICanonicalMetricSeries? Cms)> onNoSelection,
        Func<ChartDataContext, MetricSeriesSelection, (bool Matched, IReadOnlyList<MetricData>? Data, ICanonicalMetricSeries? Cms)> tryResolveFromCurrentContext,
        Func<ChartDataContext, (IReadOnlyList<MetricData>? Data, ICanonicalMetricSeries? Cms)> onMissingMetricType)
    {
        if (selectedSeries == null)
            return onNoSelection(ctx);

        var resolvedCurrentContext = tryResolveFromCurrentContext(ctx, selectedSeries);
        if (resolvedCurrentContext.Matched)
            return (resolvedCurrentContext.Data, resolvedCurrentContext.Cms);

        if (string.IsNullOrWhiteSpace(selectedSeries.MetricType))
            return onMissingMetricType(ctx);

        var tableName = resolutionTableName ?? DataAccessDefaults.DefaultTableName;
        var cacheKey = MetricSeriesSelectionCache.BuildCacheKey(selectedSeries, ctx.From, ctx.To, tableName);
        if (selectionCache.TryGetDataWithCms(cacheKey, out var cached, out var cachedCms))
            return (cached, cachedCms);

        var (primaryCms, _, primaryData, _) = await metricSelectionService.LoadMetricDataWithCmsAsync(selectedSeries, null, ctx.From, ctx.To, tableName);
        var data = primaryData.ToList();
        selectionCache.SetDataWithCms(cacheKey, data, primaryCms);
        return (data, primaryCms);
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

    private static MetricSeriesSelection? BuildFallbackSelection(string? metricType, string? subtype)
    {
        if (string.IsNullOrWhiteSpace(metricType))
            return null;

        return new MetricSeriesSelection(metricType, subtype);
    }
}
