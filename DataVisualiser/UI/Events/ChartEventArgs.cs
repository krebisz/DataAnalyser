namespace DataVisualiser.UI.Events;

public class ChartUpdateRequestedEventArgs : EventArgs
{
    public bool ShowMain { get; set; }
    public bool ShowNormalized { get; set; }
    public bool ShowDiffRatio { get; set; }
    public bool ShowDistribution { get; set; }
    public bool ShowWeeklyTrend { get; init; }
    public bool ShowTransformPanel { get; set; }
    public bool ShowBarPie { get; set; }
    public bool ShowSyncfusionSunburst { get; set; }
    public bool ShouldRenderCharts { get; set; }
    public bool IsVisibilityOnlyToggle { get; set; }
    public string? ToggledChartName { get; set; }
}

public class ChartVisibilityChangedEventArgs : EventArgs
{
    public string ChartName { get; set; } = "";
    public bool IsVisible { get; set; }
}
