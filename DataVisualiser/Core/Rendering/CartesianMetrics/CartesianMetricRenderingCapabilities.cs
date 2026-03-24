namespace DataVisualiser.Core.Rendering.CartesianMetrics;

public sealed record CartesianMetricRenderingCapabilities(
    string PathKey,
    CartesianMetricRenderingQualification Qualification,
    bool SupportsRender,
    bool SupportsUpdate,
    bool SupportsHoverTooltip,
    bool SupportsResetView,
    bool SupportsClear,
    bool SupportsLifecycleSafety);
