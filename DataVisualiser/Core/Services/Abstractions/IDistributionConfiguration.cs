using DataVisualiser.Core.Strategies.Abstractions;

namespace DataVisualiser.Core.Services.Abstractions;

/// <summary>
///     Configuration interface for distribution charts (weekly, hourly, etc.)
///     Defines the differences between distribution types (bucket count, labels, titles, etc.)
/// </summary>
public interface IDistributionConfiguration
{
    /// <summary>
    ///     Number of buckets/intervals for the x-axis (e.g., 7 for days of week, 24 for hours of day)
    /// </summary>
    int BucketCount { get; }

    /// <summary>
    ///     Labels for each bucket on the x-axis
    /// </summary>
    string[] BucketLabels { get; }

    /// <summary>
    ///     Title for the x-axis
    /// </summary>
    string XAxisTitle { get; }

    /// <summary>
    ///     Strategy type to use for computation
    /// </summary>
    StrategyType StrategyType { get; }

    /// <summary>
    ///     Prefix for log messages (e.g., "WeeklyDistribution" or "HourlyDistribution")
    /// </summary>
    string LogPrefix { get; }

    /// <summary>
    ///     Name for bucket values in logs (e.g., "Day" or "Hour")
    /// </summary>
    string BucketName { get; }

    /// <summary>
    ///     Name for bucket values in variable names (e.g., "day" or "hour")
    /// </summary>
    string BucketVariableName { get; }
}