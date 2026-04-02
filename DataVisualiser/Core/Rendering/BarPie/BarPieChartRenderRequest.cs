using DataVisualiser.UI.Charts.Presentation.Rendering;

namespace DataVisualiser.Core.Rendering.BarPie;

public sealed record BarPieChartRenderRequest(
    BarPieRenderingRoute Route,
    UiChartRenderModel Model);
