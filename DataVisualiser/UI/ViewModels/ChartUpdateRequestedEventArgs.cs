namespace DataVisualiser.UI.ViewModels.Events;

public class ChartUpdateRequestedEventArgs : EventArgs
{
    public bool ShowMain { get; set; }
    public bool ShowNormalized { get; set; }
    public bool ShowDiffRatio { get; set; }
    public bool ShowWeekly { get; set; }
    public bool ShowWeeklyTrend { get; init; }
    public bool ShowTransformPanel { get; set; }

    // NEW: whether charts should be rendered
    public bool ShouldRenderCharts { get; set; }

    // NEW: indicates if this is just a visibility toggle (no data reload needed)
    public bool IsVisibilityOnlyToggle { get; set; }

    // NEW: which chart was toggled (null if not a single-chart toggle)
    public string? ToggledChartName { get; set; }
}
