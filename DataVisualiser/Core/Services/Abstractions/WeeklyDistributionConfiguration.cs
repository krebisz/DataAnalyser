using DataVisualiser.Core.Strategies.Abstractions;

namespace DataVisualiser.Core.Services.Abstractions;

/// <summary>
///     Configuration for weekly distribution charts (days of week)
/// </summary>
public sealed class WeeklyDistributionConfiguration : IDistributionConfiguration
{
    public int BucketCount => 7;

    public string[] BucketLabels => new[]
    {
            "Monday",
            "Tuesday",
            "Wednesday",
            "Thursday",
            "Friday",
            "Saturday",
            "Sunday"
    };

    public string XAxisTitle => "Day of Week";

    public StrategyType StrategyType => StrategyType.WeeklyDistribution;

    public string LogPrefix => "WeeklyDistribution";

    public string BucketName => "Day";

    public string BucketVariableName => "day";
}