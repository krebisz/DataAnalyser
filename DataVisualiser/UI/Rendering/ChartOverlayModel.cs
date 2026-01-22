namespace DataVisualiser.UI.Rendering;

public sealed class ChartOverlayModel
{
    public string? Kind { get; init; }
    public string? Label { get; init; }
    public IReadOnlyDictionary<string, object?>? Metadata { get; init; }
}