using DataVisualiser.Core.Orchestration;
using DataVisualiser.Shared.Models;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.State;

public class ChartState
{
    private readonly Dictionary<DistributionMode, DistributionModeSettings> _distributionSettings = new();

    public ChartState()
    {
        foreach (var definition in DistributionModeCatalog.All)
            _distributionSettings[definition.Mode] = new DistributionModeSettings(true, definition.DefaultIntervalCount);
    }

    // Track which charts are visible
    public bool IsMainVisible { get; set; } = true; // Default to visible (Show on startup)
    public bool IsNormalizedVisible { get; set; }
    public bool IsDiffRatioVisible { get; set; }                // Unified Diff/Ratio chart
    public bool IsDiffRatioDifferenceMode { get; set; } = true; // true = Difference (-), false = Ratio (/)
    public bool IsDistributionVisible { get; set; }
    public bool IsWeeklyTrendVisible { get; set; }
    public bool IsTransformPanelVisible { get; set; }
    public bool IsWeekdayTrendPolarMode { get; set; } = false; // Default to Cartesian
    public bool IsDistributionPolarMode { get; set; } = false; // Default to Cartesian

    // Weekly Trend (weekday series toggles)
    public bool ShowMonday { get; set; } = true;
    public bool ShowTuesday { get; set; } = true;
    public bool ShowWednesday { get; set; } = true;
    public bool ShowThursday { get; set; } = true;
    public bool ShowFriday { get; set; } = true;
    public bool ShowSaturday { get; set; } = true;
    public bool ShowSunday { get; set; } = true;


    // Normalization mode
    public NormalizationMode SelectedNormalizationMode { get; set; }

    // Distribution chart options
    public DistributionMode SelectedDistributionMode { get; set; } = DistributionMode.Weekly;
    public MetricSeriesSelection? SelectedDistributionSeries { get; set; }
    public MetricSeriesSelection? SelectedWeekdayTrendSeries { get; set; }
    public MetricSeriesSelection? SelectedTransformPrimarySeries { get; set; }
    public MetricSeriesSelection? SelectedTransformSecondarySeries { get; set; }

    // Chart data from last load
    public ChartDataContext? LastContext { get; set; }

    // Current chart titles (left + right)
    public string LeftTitle { get; set; } = string.Empty;
    public string RightTitle { get; set; } = string.Empty;

    // Timestamps linked to each chart
    public Dictionary<CartesianChart, List<DateTime>> ChartTimestamps { get; } = new();

    public DistributionModeSettings GetDistributionSettings(DistributionMode mode)
    {
        if (_distributionSettings.TryGetValue(mode, out var settings))
            return settings;

        var definition = DistributionModeCatalog.Get(mode);
        settings = new DistributionModeSettings(true, definition.DefaultIntervalCount);
        _distributionSettings[mode] = settings;
        return settings;
    }
}
