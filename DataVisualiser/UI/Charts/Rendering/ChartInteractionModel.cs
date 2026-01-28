namespace DataVisualiser.UI.Charts.Rendering;

public sealed class ChartInteractionModel
{
    public bool EnableZoomX { get; init; }
    public bool EnableZoomY { get; init; }
    public bool EnablePanX { get; init; }
    public bool EnablePanY { get; init; }
    public bool Hoverable { get; init; }
}
