using System.Windows.Controls;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Shared.Models;
using LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Charts.Helpers;

public static class ChartUiHelper
{
    public static string GetTableNameFromResolution(ComboBox resolutionCombo)
    {
        var selectedResolution = resolutionCombo.SelectedItem?.ToString() ?? "All";
        return selectedResolution switch
        {
                "Hourly" => DataAccessDefaults.HealthMetricsHourTable,
                "Daily" => DataAccessDefaults.HealthMetricsDayTable,
                "Weekly" => DataAccessDefaults.HealthMetricsWeekTable,
                "Monthly" => DataAccessDefaults.HealthMetricsMonthTable,
                "Yearly" => DataAccessDefaults.HealthMetricsYearTable,
                _ => DataAccessDefaults.DefaultTableName
        };
    }

    public static string GetResolutionFromTableName(string? tableName)
    {
        return tableName switch
        {
                DataAccessDefaults.HealthMetricsHourTable => "Hourly",
                DataAccessDefaults.HealthMetricsDayTable => "Daily",
                DataAccessDefaults.HealthMetricsWeekTable => "Weekly",
                DataAccessDefaults.HealthMetricsMonthTable => "Monthly",
                DataAccessDefaults.HealthMetricsYearTable => "Yearly",
                _ => "All"
        };
    }

    public static string[] GetChartTitlesFromCombos(ComboBox tablesCombo, ComboBox subtypeCombo, ComboBox? subtypeCombo2)
    {
        var baseMetric = GetDisplayValueFromCombo(tablesCombo);
        var display1 = GetDisplayNameFromCombo(subtypeCombo, baseMetric);
        var display2 = subtypeCombo2 != null ? GetDisplayNameFromCombo(subtypeCombo2, baseMetric) : baseMetric;
        return new[]
        {
                display1,
                display2
        };
    }

    public static string? GetSubMetricType(ComboBox subMetricTypeCombo)
    {
        if (!subMetricTypeCombo.IsEnabled || subMetricTypeCombo.SelectedItem == null)
            return null;

        var subtypeValue = GetValueFromCombo(subMetricTypeCombo);
        if (string.IsNullOrEmpty(subtypeValue) || subtypeValue == "(All)")
            return null;

        return subtypeValue;
    }

    public static string GetDisplayNameFromCombo(ComboBox combo, string baseType)
    {
        if (combo.IsEnabled && combo.SelectedItem != null)
        {
            var selected = GetDisplayValueFromCombo(combo);
            if (!string.IsNullOrEmpty(selected) && selected != "(All)")
                return selected;
        }

        return baseType;
    }

    public static void InitializeChartBehavior(CartesianChart chart)
    {
        if (chart == null)
            return;

        chart.Zoom = ZoomingOptions.X;
        chart.Pan = PanningOptions.X;
    }

    public static void ResetZoom(CartesianChart? chart)
    {
        if (chart == null)
            return;

        ResetZoom(ref chart);
    }

    public static void ResetZoom(ref CartesianChart chart)
    {
        if (chart != null && chart.AxisX != null && chart.AxisX.Count > 0)
        {
            var axis = chart.AxisX[0];
            if (axis != null)
            {
                axis.MinValue = double.NaN;
                axis.MaxValue = double.NaN;
            }
        }
    }

    private static string GetDisplayValueFromCombo(ComboBox combo)
    {
        if (combo.SelectedItem is MetricNameOption option)
            return option.Display;

        return combo.SelectedItem?.ToString() ?? string.Empty;
    }

    private static string? GetValueFromCombo(ComboBox combo)
    {
        if (combo.SelectedItem is MetricNameOption option)
            return option.Value;

        return combo.SelectedValue?.ToString() ?? combo.SelectedItem?.ToString();
    }
}
