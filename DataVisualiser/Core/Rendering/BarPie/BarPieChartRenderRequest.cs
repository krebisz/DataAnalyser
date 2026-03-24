using DataVisualiser.UI.Charts.Rendering;

namespace DataVisualiser.Core.Rendering.BarPie;

public sealed record BarPieChartRenderRequest(
    BarPieRenderingRoute Route,
    UiChartRenderModel Model);
