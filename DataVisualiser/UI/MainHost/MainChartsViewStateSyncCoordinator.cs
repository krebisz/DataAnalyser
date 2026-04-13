using System.Windows.Controls;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.UI.MainHost;

internal sealed class MainChartsViewStateSyncCoordinator
{
    internal sealed record Actions(
        Action<string> SetResolution,
        Action<DateTime?> SetFromDate,
        Action<DateTime?> SetToDate,
        Action<MetricNameOption?> SetMetricType,
        Action<IReadOnlyList<MetricSeriesSelection>, MetricNameOption?> ApplySubtypeSelections,
        Action<int> SelectBarPieBucketCount);

    internal void Apply(MainWindowViewModel viewModel, IEnumerable<MetricNameOption> metricTypeOptions, Actions actions)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(metricTypeOptions);
        ArgumentNullException.ThrowIfNull(actions);

        var targetResolution = ChartUiHelper.GetResolutionFromTableName(viewModel.MetricState.ResolutionTableName);
        actions.SetResolution(targetResolution);

        if (viewModel.MetricState.FromDate.HasValue)
            actions.SetFromDate(viewModel.MetricState.FromDate);

        if (viewModel.MetricState.ToDate.HasValue)
            actions.SetToDate(viewModel.MetricState.ToDate);

        var metricType = ResolveMetricTypeOption(viewModel, metricTypeOptions);
        if (metricType != null)
            actions.SetMetricType(metricType);

        var selections = viewModel.MetricState.SelectedSeries.ToList();
        if (selections.Count > 0)
            actions.ApplySubtypeSelections(selections, metricType);

        actions.SelectBarPieBucketCount(viewModel.ChartState.BarPieBucketCount);
    }

    internal static void ApplyComboSelectionByValue(ComboBox combo, string? value)
    {
        if (combo.Items.Count == 0)
            return;

        if (string.IsNullOrWhiteSpace(value))
        {
            combo.SelectedIndex = 0;
            return;
        }

        var match = combo.Items.OfType<MetricNameOption>()
            .FirstOrDefault(item => string.Equals(item.Value, value, StringComparison.OrdinalIgnoreCase));

        if (match != null)
        {
            combo.SelectedItem = match;
            return;
        }

        combo.SelectedIndex = 0;
    }

    internal static void ApplySubtypeSelections(
        SubtypeSelectorManager selectorManager,
        ComboBox primaryCombo,
        IReadOnlyList<MetricNameOption> subtypeList,
        IReadOnlyList<MetricSeriesSelection> selections,
        MetricNameOption? selectedMetricType)
    {
        using var comboSuppression = selectorManager.SuppressSelectionChanged();
        selectorManager.ClearDynamic();
        selectorManager.SetPrimaryMetricType(selectedMetricType);

        ApplyComboSelectionByValue(primaryCombo, selections[0].QuerySubtype);

        for (var i = 1; i < selections.Count; i++)
        {
            var combo = selectorManager.AddSubtypeCombo(subtypeList, selectedMetricType);
            ApplyComboSelectionByValue(combo, selections[i].QuerySubtype);
        }
    }

    private static MetricNameOption? ResolveMetricTypeOption(MainWindowViewModel viewModel, IEnumerable<MetricNameOption> metricTypeOptions)
    {
        var selectedMetric = viewModel.MetricState.SelectedMetricType;
        if (string.IsNullOrWhiteSpace(selectedMetric))
            return null;

        return metricTypeOptions.FirstOrDefault(item => string.Equals(item.Value, selectedMetric, StringComparison.OrdinalIgnoreCase));
    }
}
