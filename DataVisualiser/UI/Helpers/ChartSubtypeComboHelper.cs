using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using DataVisualiser.UI.Controls;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.UI.Helpers;

public static class ChartSubtypeComboHelper
{
    public static void PopulateCombo(ComboBox combo, IReadOnlyList<MetricSeriesSelection> selectedSeries)
    {
        if (combo == null)
            throw new ArgumentNullException(nameof(combo));

        combo.Items.Clear();

        foreach (var selection in selectedSeries)
            combo.Items.Add(MetricSeriesSelectionCache.BuildSeriesComboItem(selection));

        combo.IsEnabled = selectedSeries.Count > 0;
    }

    public static MetricSeriesSelection? ResolveSelection(IReadOnlyList<MetricSeriesSelection> selectedSeries, MetricSeriesSelection? currentSelection)
    {
        if (selectedSeries == null || selectedSeries.Count == 0)
            return null;

        if (currentSelection != null && selectedSeries.Any(series => string.Equals(series.DisplayKey, currentSelection.DisplayKey, StringComparison.OrdinalIgnoreCase)))
            return currentSelection;

        return selectedSeries[0];
    }

    public static void SelectComboItem(ComboBox combo, MetricSeriesSelection selection)
    {
        if (combo == null)
            throw new ArgumentNullException(nameof(combo));
        if (selection == null)
            throw new ArgumentNullException(nameof(selection));

        var item = MetricSeriesSelectionCache.FindSeriesComboItem(combo, selection) ?? combo.Items.OfType<ComboBoxItem>().FirstOrDefault();
        combo.SelectedItem = item;
    }

    public static void DisableCombo(ComboBox combo)
    {
        if (combo == null)
            throw new ArgumentNullException(nameof(combo));

        combo.IsEnabled = false;
        combo.SelectedItem = null;
    }
}
