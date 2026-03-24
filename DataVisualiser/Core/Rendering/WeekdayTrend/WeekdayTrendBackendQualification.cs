namespace DataVisualiser.Core.Rendering.WeekdayTrend;

public sealed record WeekdayTrendBackendQualification(
    string BackendKey,
    string PathKey,
    WeekdayTrendRenderingQualification Qualification,
    WeekdayTrendRenderingRoute ActiveRoute,
    bool SupportsRender,
    bool SupportsUpdate,
    bool SupportsResetView,
    bool SupportsClear,
    bool SupportsLifecycleSafety);
