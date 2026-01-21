namespace DataVisualiser.Core.Configuration;

/// <summary>
///     Configuration for CMS (Canonical Metric Series) integration in DataVisualiser.
///     Phase 4: Enables explicit opt-in to CMS-based workflows.
///     When enabled, strategies will use CMS data instead of legacy MetricData.
/// </summary>
public static class CmsConfiguration
{
    /// <summary>
    ///     Global flag to enable CMS-based data workflows.
    ///     Default: false (legacy mode)
    /// </summary>
    public static bool UseCmsData { get; set; } = true; // ENABLED for SingleMetricStrategy testing

    /// <summary>
    ///     Per-strategy CMS enablement.
    ///     Allows gradual migration strategy by strategy.
    /// </summary>
    public static bool UseCmsForSingleMetric { get; set; } = true; // ENABLED for testing

    public static bool UseCmsForMultiMetric { get; set; } = true;
    public static bool UseCmsForCombinedMetric { get; set; } = true;
    public static bool UseCmsForDifference { get; set; } = true;
    public static bool UseCmsForRatio { get; set; } = true;
    public static bool UseCmsForNormalized { get; set; } = true;
    public static bool UseCmsForWeeklyDistribution { get; set; } = true;
    public static bool UseCmsForWeekdayTrend { get; set; } = true;
    public static bool UseCmsForHourlyDistribution { get; set; } = true;
    public static bool UseCmsForBarPie { get; set; } = true;

    /// <summary>
    ///     Checks if CMS should be used for a specific strategy type.
    /// </summary>
    public static bool ShouldUseCms(string strategyType)
    {
        if (!UseCmsData)
            return false;

        return strategyType switch
        {
                "SingleMetricStrategy" => UseCmsForSingleMetric,
                "MultiMetricStrategy" => UseCmsForMultiMetric,
                "CombinedMetricStrategy" => UseCmsForCombinedMetric,
                "DifferenceStrategy" => UseCmsForDifference,
                "RatioStrategy" => UseCmsForRatio,
                "NormalizedStrategy" => UseCmsForNormalized,
                "WeeklyDistributionStrategy" => UseCmsForWeeklyDistribution,
                "WeekdayTrendStrategy" => UseCmsForWeekdayTrend,
                "HourlyDistributionStrategy" => UseCmsForHourlyDistribution,
                "BarPieStrategy" => UseCmsForBarPie,
                _ => false
        };
    }
}
