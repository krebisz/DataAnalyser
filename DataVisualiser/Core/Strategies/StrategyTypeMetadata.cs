using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Validation.Parity;

namespace DataVisualiser.Core.Strategies;

public static class StrategyTypeMetadata
{
    private static readonly IReadOnlyDictionary<StrategyType, StrategyTypeInfo> Infos = new Dictionary<StrategyType, StrategyTypeInfo>
    {
        { StrategyType.SingleMetric, new StrategyTypeInfo("SingleMetricStrategy", () => new ChartComputationParityHarness()) },
        { StrategyType.CombinedMetric, new StrategyTypeInfo("CombinedMetricStrategy", () => new CombinedMetricParityHarness()) },
        { StrategyType.MultiMetric, new StrategyTypeInfo("MultiMetricStrategy", () => new ChartComputationParityHarness()) },
        { StrategyType.Difference, new StrategyTypeInfo("DifferenceStrategy", null) },
        { StrategyType.Ratio, new StrategyTypeInfo("RatioStrategy", null) },
        { StrategyType.Normalized, new StrategyTypeInfo("NormalizedStrategy", () => new ChartComputationParityHarness()) },
        { StrategyType.WeeklyDistribution, new StrategyTypeInfo("WeeklyDistributionStrategy", () => new WeeklyDistributionParityHarness()) },
        { StrategyType.HourlyDistribution, new StrategyTypeInfo("HourlyDistributionStrategy", () => new HourlyDistributionParityHarness()) },
        { StrategyType.WeekdayTrend, new StrategyTypeInfo("WeekdayTrendStrategy", () => new ChartComputationParityHarness()) }
    };

    public static string? GetConfigName(StrategyType strategyType)
    {
        return Infos.TryGetValue(strategyType, out var info) ? info.ConfigName : null;
    }

    public static IStrategyParityHarness? CreateParityHarness(StrategyType strategyType)
    {
        return Infos.TryGetValue(strategyType, out var info) ? info.ParityHarnessFactory?.Invoke() : null;
    }

    private sealed record StrategyTypeInfo(string ConfigName, Func<IStrategyParityHarness?>? ParityHarnessFactory);
}
