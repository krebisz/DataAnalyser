namespace DataVisualiser.Core.Rendering.CartesianMetrics;

public sealed record CartesianMetricBackendQualification(
    CartesianMetricBackendKey BackendKey,
    string PathKey,
    CartesianMetricRenderingQualification Qualification,
    CartesianMetricChartRoute Route,
    bool SupportsRender,
    bool SupportsUpdate,
    bool SupportsHoverTooltip,
    bool SupportsResetView,
    bool SupportsClear,
    bool SupportsLifecycleSafety);
