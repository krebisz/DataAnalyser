namespace DataVisualiser.Core.Rendering.Distribution;

public sealed record DistributionRenderingCapabilities(
    string PathKey,
    DistributionRenderingQualification Qualification,
    bool SupportsRender,
    bool SupportsUpdate,
    bool SupportsHoverTooltip,
    bool SupportsResetView,
    bool SupportsClear,
    bool SupportsLifecycleSafety);
