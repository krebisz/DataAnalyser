using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.VNext.State;

public sealed record PresentationState(
    ChartDisplayMode MainChartDisplayMode,
    bool MainVisible,
    bool NormalizedVisible,
    bool DifferenceVisible,
    bool RatioVisible)
{
    public static PresentationState Default { get; } = new(ChartDisplayMode.Regular, true, false, false, false);
}
