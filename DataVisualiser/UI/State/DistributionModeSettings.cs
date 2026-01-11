namespace DataVisualiser.UI.State;

public sealed class DistributionModeSettings
{
    public DistributionModeSettings(bool useFrequencyShading, int intervalCount)
    {
        UseFrequencyShading = useFrequencyShading;
        IntervalCount = intervalCount;
    }

    public bool UseFrequencyShading { get; set; }
    public int IntervalCount { get; set; }
}