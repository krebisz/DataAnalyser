namespace DataVisualiser.UI.State;

/// <summary>
///     Configuration for CMS (Canonical Metric Series) integration in DataVisualiser.
///     Phase 4: Enables explicit opt-in to CMS-based workflows.
///     When enabled, strategies will use CMS data instead of legacy HealthMetricData.
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

    public static bool UseCmsForMultiMetric { get; set; }
    public static bool UseCmsForCombinedMetric { get; set; }
    public static bool UseCmsForDifference { get; set; }
    public static bool UseCmsForRatio { get; set; }
    public static bool UseCmsForNormalized { get; set; }

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
            _ => false
        };
    }

    /// <summary>
    ///     Resets all CMS flags to default (disabled).
    /// </summary>
    public static void ResetToDefaults()
    {
        UseCmsData = false;
        UseCmsForSingleMetric = false;
        UseCmsForMultiMetric = false;
        UseCmsForCombinedMetric = false;
        UseCmsForDifference = false;
        UseCmsForRatio = false;
        UseCmsForNormalized = false;
    }
}
