using DataVisualiser.Charts;
using DataVisualiser.Models;

namespace DataVisualiser.Services.Abstractions
{
    /// <summary>
    /// Unified cut-over mechanism for all strategies.
    /// Single decision point for legacy vs CMS strategy selection.
    /// </summary>
    public interface IStrategyCutOverService
    {
        /// <summary>
        /// Creates a strategy with unified cut-over logic and parity validation.
        /// </summary>
        IChartComputationStrategy CreateStrategy(
            StrategyType strategyType,
            ChartDataContext ctx,
            StrategyCreationParameters parameters);

        /// <summary>
        /// Determines if CMS should be used for a strategy.
        /// </summary>
        bool ShouldUseCms(StrategyType strategyType, ChartDataContext ctx);

        /// <summary>
        /// Executes parity validation between legacy and CMS strategies.
        /// </summary>
        ParityResult ValidateParity(
            IChartComputationStrategy legacyStrategy,
            IChartComputationStrategy cmsStrategy);
    }

    /// <summary>
    /// Strategy type enumeration.
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

    /// <summary>
    /// Parameters for strategy creation.
    /// </summary>
    public class StrategyCreationParameters
    {
        public IEnumerable<HealthMetricData>? LegacyData1 { get; init; }
        public IEnumerable<HealthMetricData>? LegacyData2 { get; init; }
        public IReadOnlyList<IEnumerable<HealthMetricData>>? LegacySeries { get; init; }
        public string Label1 { get; init; } = string.Empty;
        public string Label2 { get; init; } = string.Empty;
        public IReadOnlyList<string>? Labels { get; init; }
        public DateTime From { get; init; }
        public DateTime To { get; init; }
        public string? Unit { get; init; }
    }

    /// <summary>
    /// Result of parity validation.
    /// </summary>
    public class ParityResult
    {
        public bool Passed { get; init; }
        public string? Message { get; init; }
        public Dictionary<string, object>? Details { get; init; }
    }
}

