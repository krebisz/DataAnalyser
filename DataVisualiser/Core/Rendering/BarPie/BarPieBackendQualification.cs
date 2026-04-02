using DataVisualiser.UI.Charts.Presentation.Rendering;

namespace DataVisualiser.Core.Rendering.BarPie;

public sealed record BarPieBackendQualification(
    string BackendKey,
    string PathKey,
    ChartRendererKind RendererKind,
    BarPieRenderingRoute Route,
    BarPieRenderingQualification Qualification,
    bool SupportsRender,
    bool SupportsUpdate,
    bool SupportsResetView,
    bool SupportsClear,
    bool SupportsLifecycleSafety);
