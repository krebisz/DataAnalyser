namespace DataVisualiser.Core.Rendering.Transform;

public sealed record TransformBackendQualification(
    TransformBackendKey BackendKey,
    string PathKey,
    TransformRenderingQualification Qualification,
    TransformRenderingRoute Route,
    bool SupportsRender,
    bool SupportsUpdate,
    bool SupportsHoverTooltip,
    bool SupportsResetView,
    bool SupportsClear,
    bool SupportsLifecycleSafety);
