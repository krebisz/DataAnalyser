namespace DataVisualiser.Core.Rendering.WeekdayTrend;

public sealed record WeekdayTrendRenderingCapabilities(
    string PathKey,
    WeekdayTrendRenderingQualification Qualification,
    bool SupportsRender,
    bool SupportsUpdate,
    bool SupportsResetView,
    bool SupportsClear,
    bool SupportsLifecycleSafety);
