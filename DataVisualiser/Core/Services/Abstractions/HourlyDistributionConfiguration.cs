using DataVisualiser.Core.Strategies.Abstractions;

namespace DataVisualiser.Core.Services.Abstractions;

/// <summary>
///     Configuration for hourly distribution charts (hours of day)
/// </summary>
public sealed class HourlyDistributionConfiguration : IDistributionConfiguration
{
    public int BucketCount => 24;

    public string[] BucketLabels => new[]
    {
            "12AM",
            "1AM",
            "2AM",
            "3AM",
            "4AM",
            "5AM",
            "6AM",
            "7AM",
            "8AM",
            "9AM",
            "10AM",
            "11AM",
            "12PM",
            "1PM",
            "2PM",
            "3PM",
            "4PM",
            "5PM",
            "6PM",
            "7PM",
            "8PM",
            "9PM",
            "10PM",
            "11PM"
    };

    public string XAxisTitle => "Hours of Day";

    public StrategyType StrategyType => StrategyType.HourlyDistribution;

    public string LogPrefix => "HourlyDistribution";

    public string BucketName => "Hour";

    public string BucketVariableName => "hour";
}