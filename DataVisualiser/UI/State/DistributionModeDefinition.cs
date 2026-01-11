using DataVisualiser.Shared.Models;

namespace DataVisualiser.UI.State;

public sealed class DistributionModeDefinition
{
    public DistributionModeDefinition(DistributionMode mode, string displayName, string title, string xAxisTitle, IReadOnlyList<string> xAxisLabels, int defaultIntervalCount)
    {
        Mode = mode;
        DisplayName = displayName;
        Title = title;
        XAxisTitle = xAxisTitle;
        XAxisLabels = xAxisLabels;
        DefaultIntervalCount = defaultIntervalCount;
    }

    public DistributionMode Mode { get; }
    public string DisplayName { get; }
    public string Title { get; }
    public string XAxisTitle { get; }
    public IReadOnlyList<string> XAxisLabels { get; }
    public int DefaultIntervalCount { get; }
}