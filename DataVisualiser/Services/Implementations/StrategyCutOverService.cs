using DataFileReader.Canonical;
using DataVisualiser.Charts;
using DataVisualiser.Charts.Strategies;
using DataVisualiser.Models;
using DataVisualiser.Services.Abstractions;
using DataVisualiser.State;

namespace DataVisualiser.Services.Implementations
{
    /// <summary>
    /// Implementation of IStrategyCutOverService.
    /// Provides unified cut-over mechanism for all strategies with parity validation.
    /// </summary>
    public sealed class StrategyCutOverService : IStrategyCutOverService
    {
        private readonly IDataPreparationService _dataPreparation;

        public StrategyCutOverService(IDataPreparationService dataPreparation)
        {
            _dataPreparation = dataPreparation ?? throw new ArgumentNullException(nameof(dataPreparation));
        }

        public bool ShouldUseCms(StrategyType strategyType, ChartDataContext ctx)
        {
            // Check global configuration
            if (!CmsConfiguration.UseCmsData)
                return false;

            // Check strategy-specific configuration
            var strategyName = strategyType switch
            {
                StrategyType.SingleMetric => "SingleMetricStrategy",
                StrategyType.CombinedMetric => "CombinedMetricStrategy",
                StrategyType.MultiMetric => "MultiMetricStrategy",
                StrategyType.Difference => "DifferenceStrategy",
                StrategyType.Ratio => "RatioStrategy",
                StrategyType.Normalized => "NormalizedStrategy",
                _ => null
            };
            
            if (strategyName != null && !CmsConfiguration.ShouldUseCms(strategyName))
                return false;

            // Check if CMS data is available
            return strategyType switch
            {
                StrategyType.SingleMetric => ctx.PrimaryCms != null,
                StrategyType.CombinedMetric => ctx.PrimaryCms != null && ctx.SecondaryCms != null,
                StrategyType.MultiMetric => ctx.PrimaryCms != null, // At least primary
                StrategyType.Difference => ctx.PrimaryCms != null && ctx.SecondaryCms != null,
                StrategyType.Ratio => ctx.PrimaryCms != null && ctx.SecondaryCms != null,
                StrategyType.Normalized => ctx.PrimaryCms != null && ctx.SecondaryCms != null,
                StrategyType.WeeklyDistribution => ctx.PrimaryCms != null,
                StrategyType.WeekdayTrend => ctx.PrimaryCms != null,
                _ => false
            };
        }

        public IChartComputationStrategy CreateStrategy(
            StrategyType strategyType,
            ChartDataContext ctx,
            StrategyCreationParameters parameters)
        {
            if (ShouldUseCms(strategyType, ctx))
            {
                return CreateCmsStrategy(strategyType, ctx, parameters);
            }

            return CreateLegacyStrategy(strategyType, parameters);
        }

        public ParityResult ValidateParity(
            IChartComputationStrategy legacyStrategy,
            IChartComputationStrategy cmsStrategy)
        {
            if (legacyStrategy == null || cmsStrategy == null)
            {
                return new ParityResult
                {
                    Passed = false,
                    Message = "One or both strategies are null"
                };
            }

            // Execute both strategies
            var legacyResult = legacyStrategy.Compute();
            var cmsResult = cmsStrategy.Compute();

            // Basic validation
            if (legacyResult == null && cmsResult == null)
            {
                return new ParityResult { Passed = true, Message = "Both strategies returned null" };
            }

            if (legacyResult == null || cmsResult == null)
            {
                return new ParityResult
                {
                    Passed = false,
                    Message = $"One strategy returned null: legacy={legacyResult == null}, cms={cmsResult == null}"
                };
            }

            // Compare results (simplified - full parity harness should be used)
            var timestampsMatch = legacyResult.Timestamps.Count == cmsResult.Timestamps.Count;
            var valuesMatch = legacyResult.PrimaryRawValues.Count == cmsResult.PrimaryRawValues.Count;

            if (!timestampsMatch || !valuesMatch)
            {
                return new ParityResult
                {
                    Passed = false,
                    Message = $"Result counts differ: timestamps={timestampsMatch}, values={valuesMatch}"
                };
            }

            // TODO: Full numeric comparison using existing parity harness
            return new ParityResult { Passed = true, Message = "Basic validation passed" };
        }

        private IChartComputationStrategy CreateCmsStrategy(
            StrategyType strategyType,
            ChartDataContext ctx,
            StrategyCreationParameters parameters)
        {
            return strategyType switch
            {
                StrategyType.SingleMetric => new SingleMetricCmsStrategy(
                    ctx.PrimaryCms as ICanonicalMetricSeries ?? throw new InvalidOperationException("PrimaryCms is null"),
                    parameters.Label1,
                    parameters.From,
                    parameters.To),

                StrategyType.CombinedMetric => new CombinedMetricCmsStrategy(
                    ctx.PrimaryCms as ICanonicalMetricSeries ?? throw new InvalidOperationException("PrimaryCms is null"),
                    ctx.SecondaryCms as ICanonicalMetricSeries ?? throw new InvalidOperationException("SecondaryCms is null"),
                    parameters.Label1,
                    parameters.Label2,
                    parameters.From,
                    parameters.To),

                StrategyType.WeeklyDistribution => new CmsWeeklyDistributionStrategy(
                    ctx.PrimaryCms as ICanonicalMetricSeries ?? throw new InvalidOperationException("PrimaryCms is null"),
                    parameters.From,
                    parameters.To,
                    parameters.Label1),

                // TODO: Implement other CMS strategies
                _ => CreateLegacyStrategy(strategyType, parameters)
            };
        }

        private IChartComputationStrategy CreateLegacyStrategy(
            StrategyType strategyType,
            StrategyCreationParameters parameters)
        {
            return strategyType switch
            {
                StrategyType.SingleMetric => new SingleMetricLegacyStrategy(
                    parameters.LegacyData1 ?? Array.Empty<HealthMetricData>(),
                    parameters.Label1,
                    parameters.From,
                    parameters.To),

                StrategyType.CombinedMetric => new CombinedMetricStrategy(
                    parameters.LegacyData1 ?? Array.Empty<HealthMetricData>(),
                    parameters.LegacyData2 ?? Array.Empty<HealthMetricData>(),
                    parameters.Label1,
                    parameters.Label2,
                    parameters.From,
                    parameters.To),

                StrategyType.Difference => new DifferenceStrategy(
                    parameters.LegacyData1 ?? Array.Empty<HealthMetricData>(),
                    parameters.LegacyData2 ?? Array.Empty<HealthMetricData>(),
                    parameters.Label1,
                    parameters.Label2,
                    parameters.From,
                    parameters.To),

                StrategyType.Ratio => new RatioStrategy(
                    parameters.LegacyData1 ?? Array.Empty<HealthMetricData>(),
                    parameters.LegacyData2 ?? Array.Empty<HealthMetricData>(),
                    parameters.Label1,
                    parameters.Label2,
                    parameters.From,
                    parameters.To),

                StrategyType.Normalized => new NormalizedStrategy(
                    parameters.LegacyData1 ?? Array.Empty<HealthMetricData>(),
                    parameters.LegacyData2 ?? Array.Empty<HealthMetricData>(),
                    parameters.Label1,
                    parameters.Label2,
                    parameters.From,
                    parameters.To,
                    parameters.NormalizationMode ?? NormalizationMode.PercentageOfMax),

                StrategyType.MultiMetric => new MultiMetricStrategy(
                    parameters.LegacySeries ?? Array.Empty<IEnumerable<HealthMetricData>>(),
                    parameters.Labels ?? Array.Empty<string>(),
                    parameters.From,
                    parameters.To,
                    parameters.Unit),

                StrategyType.WeeklyDistribution => new WeeklyDistributionStrategy(
                    parameters.LegacyData1 ?? Array.Empty<HealthMetricData>(),
                    parameters.Label1,
                    parameters.From,
                    parameters.To),

                _ => throw new NotSupportedException($"Strategy type {strategyType} is not supported")
            };
        }
    }
}

