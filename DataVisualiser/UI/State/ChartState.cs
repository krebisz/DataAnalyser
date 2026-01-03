using DataVisualiser.Core.Orchestration;
using DataVisualiser.Shared.Models;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.State;

public class ChartState
{
    // Track which charts are visible
    public bool IsMainVisible { get; set; } = true; // Default to visible (Show on startup)
    public bool IsNormalizedVisible { get; set; }
    public bool IsDiffRatioVisible { get; set; } // Unified Diff/Ratio chart
    public bool IsDiffRatioDifferenceMode { get; set; } = true; // true = Difference (-), false = Ratio (/)
    public bool IsWeeklyVisible { get; set; }
    public bool IsWeeklyTrendVisible { get; set; }
    public bool IsTransformPanelVisible { get; set; }
    public bool IsWeekdayTrendPolarMode { get; set; } = false; // Default to Cartesian

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

    // Weekly distribution chart options
    public bool UseFrequencyShading { get; set; } = true; // Default to frequency shading enabled
    public int WeeklyIntervalCount { get; set; } = 25; // Default interval count for frequency shading

    // Chart data from last load
    public ChartDataContext? LastContext { get; set; }

    // Current chart titles (left + right)
    public string LeftTitle { get; set; } = string.Empty;
    public string RightTitle { get; set; } = string.Empty;

    // Timestamps linked to each chart
    public Dictionary<CartesianChart, List<DateTime>> ChartTimestamps { get; } = new();
}
