namespace DataVisualiser.Core.Rendering.Distribution;

public sealed record DistributionBackendQualification(
    string BackendKey,
    string PathKey,
    DistributionRenderingQualification Qualification,
    DistributionRenderingRoute? ActiveRoute,
    bool SupportsRender,
    bool SupportsUpdate,
    bool SupportsHoverTooltip,
    bool SupportsResetView,
    bool SupportsClear,
    bool SupportsLifecycleSafety);
