namespace DataVisualiser.Core.Configuration;

public interface ICmsRuntimeConfiguration
{
    bool UseCmsData { get; set; }
    bool UseCmsForSingleMetric { get; set; }
    bool UseCmsForMultiMetric { get; set; }
    bool UseCmsForCombinedMetric { get; set; }
    bool UseCmsForDifference { get; set; }
    bool UseCmsForRatio { get; set; }
    bool UseCmsForNormalized { get; set; }
    bool UseCmsForWeeklyDistribution { get; set; }
    bool UseCmsForWeekdayTrend { get; set; }
    bool UseCmsForHourlyDistribution { get; set; }
    bool UseCmsForBarPie { get; set; }

    bool ShouldUseCms(string strategyType);
}
