using DataVisualiser.UI.Charts.Presentation.Rendering;

namespace DataVisualiser.Core.Rendering.BarPie;

public static class BarPieBackendKey
{
    public const string LiveChartsWpfColumn = "LiveChartsWpf.Column";
    public const string LiveChartsWpfPieFacet = "LiveChartsWpf.PieFacet";
    public const string EChartsPlaceholderColumn = "ECharts.Placeholder.Column";
    public const string EChartsPlaceholderPieFacet = "ECharts.Placeholder.PieFacet";
}

public enum BarPieRenderingRoute
{
    Column = 0,
    PieFacet = 1
}

public enum BarPieRenderingQualification
{
    Qualified = 0,
    UnqualifiedDebt = 1
}

public sealed record BarPieChartRenderRequest(
    BarPieRenderingRoute Route,
    UiChartRenderModel Model);

public sealed record BarPieChartRenderHost(
    IChartSurface Surface,
    IChartRenderer Renderer,
    ChartRendererKind RendererKind,
    bool IsVisible);
