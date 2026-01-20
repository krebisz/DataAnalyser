using DataVisualiser.Shared.Models;

namespace DataVisualiser.UI.State;

internal sealed class DistributionPolarTooltipState
{
    public DistributionPolarTooltipState(DistributionModeDefinition definition, DistributionRangeResult rangeResult)
    {
        Definition = definition;
        RangeResult = rangeResult;
    }

    public DistributionModeDefinition Definition { get; }
    public DistributionRangeResult RangeResult { get; }
}
