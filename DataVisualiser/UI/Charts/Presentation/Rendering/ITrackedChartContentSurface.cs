namespace DataVisualiser.UI.Charts.Presentation.Rendering;

/// <summary>
///     Optional surface capability for tracking whether a renderer has produced
///     meaningful chart content on the surface.
/// </summary>
public interface ITrackedChartContentSurface
{
    bool HasRenderedContent { get; }

    void SetHasRenderedContent(bool hasRenderedContent);
}
