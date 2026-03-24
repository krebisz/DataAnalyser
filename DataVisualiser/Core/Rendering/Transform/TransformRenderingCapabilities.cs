namespace DataVisualiser.Core.Rendering.Transform;

public sealed record TransformRenderingCapabilities(
    string PathKey,
    TransformRenderingQualification Qualification,
    bool SupportsRender,
    bool SupportsUpdate,
    bool SupportsHoverTooltip,
    bool SupportsResetView,
    bool SupportsClear,
    bool SupportsLifecycleSafety);
