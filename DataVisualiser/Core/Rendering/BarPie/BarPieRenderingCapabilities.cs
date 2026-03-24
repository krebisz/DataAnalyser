namespace DataVisualiser.Core.Rendering.BarPie;

public sealed record BarPieRenderingCapabilities(
    string PathKey,
    BarPieRenderingQualification Qualification,
    bool SupportsRender,
    bool SupportsUpdate,
    bool SupportsResetView,
    bool SupportsClear,
    bool SupportsLifecycleSafety);
