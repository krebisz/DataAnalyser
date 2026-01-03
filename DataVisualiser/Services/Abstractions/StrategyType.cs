namespace DataVisualiser.Services.Abstractions;

/// <summary>
///     Strategy type enumeration.
/// </summary>
public enum StrategyType
{
    SingleMetric,
    CombinedMetric,
    MultiMetric,
    Difference,
    Ratio,
    Normalized,
    WeeklyDistribution,
    WeekdayTrend
}