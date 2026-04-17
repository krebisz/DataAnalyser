using System.Windows.Controls;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.UI.MainHost;

internal static class MetricSelectionComboReader
{
    public static string? GetSelectedMetricValue(ComboBox combo)
    {
        ArgumentNullException.ThrowIfNull(combo);

        if (combo.SelectedItem is MetricNameOption option)
            return option.Value;

        return combo.SelectedValue?.ToString() ?? combo.SelectedItem?.ToString();
    }

    public static MetricNameOption? GetSelectedMetricOption(ComboBox combo)
    {
        ArgumentNullException.ThrowIfNull(combo);
        return combo.SelectedItem as MetricNameOption;
    }
}
