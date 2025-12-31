using DataFileReader.Canonical;
using DataVisualiser.Charts;
using DataVisualiser.Charts.Strategies;
using DataVisualiser.Models;
using DataVisualiser.Services.Abstractions;
using DataVisualiser.Services.Implementations.Factories;
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
        private readonly Dictionary<StrategyType, IStrategyFactory> _factories;

        public StrategyCutOverService(IDataPreparationService dataPreparation)
        {
            _dataPreparation = dataPreparation ?? throw new ArgumentNullException(nameof(dataPreparation));
            
            // Initialize factories
            _factories = new Dictionary<StrategyType, IStrategyFactory>
            {
                { StrategyType.SingleMetric, new SingleMetricStrategyFactory() },
                { StrategyType.CombinedMetric, new CombinedMetricStrategyFactory() },
                { StrategyType.MultiMetric, new MultiMetricStrategyFactory() },
                { StrategyType.Difference, new DifferenceStrategyFactory() },
                { StrategyType.Ratio, new RatioStrategyFactory() },
                { StrategyType.Normalized, new NormalizedStrategyFactory() },
                { StrategyType.WeeklyDistribution, new WeeklyDistributionStrategyFactory() },
                { StrategyType.WeekdayTrend, new WeekdayTrendStrategyFactory() }
            };
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
            if (!_factories.TryGetValue(strategyType, out var factory))
            {
                throw new NotSupportedException($"Strategy type {strategyType} is not supported");
            }

            return factory.CreateCmsStrategy(ctx, parameters);
        }

        private IChartComputationStrategy CreateLegacyStrategy(
            StrategyType strategyType,
            StrategyCreationParameters parameters)
        {
            if (!_factories.TryGetValue(strategyType, out var factory))
            {
                throw new NotSupportedException($"Strategy type {strategyType} is not supported");
            }

            return factory.CreateLegacyStrategy(parameters);
        }
    }
}

