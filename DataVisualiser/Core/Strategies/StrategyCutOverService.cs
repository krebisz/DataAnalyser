using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Factories;
using DataVisualiser.UI.State;
using DataVisualiser.Validation.Parity;
using ParityResult = DataVisualiser.Validation.ParityResult;

namespace DataVisualiser.Core.Strategies;

/// <summary>
///     Implementation of IStrategyCutOverService.
///     Provides unified cut-over mechanism for all strategies with parity validation.
/// </summary>
public sealed class StrategyCutOverService : IStrategyCutOverService
{
    private readonly Dictionary<StrategyType, IStrategyFactory> _factories;

    public StrategyCutOverService(IDataPreparationService dataPreparation)
    {
        _ = dataPreparation ?? throw new ArgumentNullException(nameof(dataPreparation));

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
                { StrategyType.WeekdayTrend, new WeekdayTrendStrategyFactory() },
                { StrategyType.HourlyDistribution, new HourlyDistributionStrategyFactory() }
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
                StrategyType.SingleMetric   => "SingleMetricStrategy",
                StrategyType.CombinedMetric => "CombinedMetricStrategy",
                StrategyType.MultiMetric    => "MultiMetricStrategy",
                StrategyType.Difference     => "DifferenceStrategy",
                StrategyType.Ratio          => "RatioStrategy",
                StrategyType.Normalized     => "NormalizedStrategy",
                _                           => null
        };

        if (strategyName != null && !CmsConfiguration.ShouldUseCms(strategyName))
            return false;

        // Check if CMS data is available
        return strategyType switch
        {
                StrategyType.SingleMetric       => ctx.PrimaryCms != null,
                StrategyType.CombinedMetric     => ctx.PrimaryCms != null && ctx.SecondaryCms != null,
                StrategyType.MultiMetric        => ctx.PrimaryCms != null, // At least primary
                StrategyType.Difference         => ctx.PrimaryCms != null && ctx.SecondaryCms != null,
                StrategyType.Ratio              => ctx.PrimaryCms != null && ctx.SecondaryCms != null,
                StrategyType.Normalized         => ctx.PrimaryCms != null && ctx.SecondaryCms != null,
                StrategyType.WeeklyDistribution => ctx.PrimaryCms != null,
                StrategyType.HourlyDistribution => ctx.PrimaryCms != null,
                StrategyType.WeekdayTrend           => ctx.PrimaryCms != null,
                _                               => false
        };
    }

    public IChartComputationStrategy CreateStrategy(StrategyType strategyType, ChartDataContext ctx, StrategyCreationParameters parameters)
    {
        if (ShouldUseCms(strategyType, ctx))
            return CreateCmsStrategy(strategyType, ctx, parameters);

        return CreateLegacyStrategy(strategyType, parameters);
    }

    public ParityResult ValidateParity(IChartComputationStrategy legacyStrategy, IChartComputationStrategy cmsStrategy)
    {
        if (legacyStrategy == null || cmsStrategy == null)
            return new ParityResult
            {
                    Passed = false,
                    Message = "One or both strategies are null"
            };

        // Determine strategy type from strategy instances
        var strategyType = DetermineStrategyType(legacyStrategy, cmsStrategy);

        // Get appropriate parity harness
        var harness = GetParityHarness(strategyType);
        if (harness == null)
                // Fallback to basic validation if no harness available
            return PerformBasicValidation(legacyStrategy, cmsStrategy);

        // Use parity harness for full validation
        var context = new StrategyParityContext
        {
                StrategyName = strategyType?.ToString() ?? "Unknown",
                MetricIdentity = "ParityValidation",
                Mode = ParityMode.Diagnostic
        };

        var harnessResult = harness.Validate(context, () => ParityResultAdapter.ToLegacyExecutionResult(legacyStrategy.Compute()), () => ParityResultAdapter.ToCmsExecutionResult(cmsStrategy.Compute()));

        // Convert Validation.Parity.ParityResult to Validation.ParityResult
        return ConvertParityResult(harnessResult);
    }

    private StrategyType? DetermineStrategyType(IChartComputationStrategy legacyStrategy, IChartComputationStrategy cmsStrategy)
    {
        // Determine strategy type from strategy class names
        var legacyTypeName = legacyStrategy.GetType().Name;
        var cmsTypeName = cmsStrategy.GetType().Name;

        // Map strategy class names to StrategyType enum
        if (legacyTypeName.Contains("SingleMetric") || cmsTypeName.Contains("SingleMetric"))
            return StrategyType.SingleMetric;
        if (legacyTypeName.Contains("CombinedMetric") || cmsTypeName.Contains("CombinedMetric"))
            return StrategyType.CombinedMetric;
        if (legacyTypeName.Contains("MultiMetric") || cmsTypeName.Contains("MultiMetric"))
            return StrategyType.MultiMetric;
        if (legacyTypeName.Contains("Difference") || cmsTypeName.Contains("Difference"))
            return StrategyType.Difference;
        if (legacyTypeName.Contains("Ratio") || cmsTypeName.Contains("Ratio"))
            return StrategyType.Ratio;
        if (legacyTypeName.Contains("Normalized") || cmsTypeName.Contains("Normalized"))
            return StrategyType.Normalized;
        if (legacyTypeName.Contains("WeeklyDistribution") || cmsTypeName.Contains("WeeklyDistribution"))
            return StrategyType.WeeklyDistribution;
        if (legacyTypeName.Contains("HourlyDistribution") || cmsTypeName.Contains("HourlyDistribution"))
            return StrategyType.HourlyDistribution;
        if (legacyTypeName.Contains("WeekdayTrend") || cmsTypeName.Contains("WeekdayTrend"))
            return StrategyType.WeekdayTrend;

        return null;
    }

    private IStrategyParityHarness? GetParityHarness(StrategyType? strategyType)
    {
        return strategyType switch
        {
                StrategyType.CombinedMetric     => new CombinedMetricParityHarness(),
                StrategyType.WeeklyDistribution => new WeeklyDistributionParityHarness(),
                StrategyType.HourlyDistribution => new HourlyDistributionParityHarness(),
            // TODO: Add harnesses for other strategy types as they become available
            _ => null
        };
    }

    private ParityResult PerformBasicValidation(IChartComputationStrategy legacyStrategy, IChartComputationStrategy cmsStrategy)
    {
        // Fallback basic validation when no harness is available
        var legacyResult = legacyStrategy.Compute();
        var cmsResult = cmsStrategy.Compute();

        if (legacyResult == null && cmsResult == null)
            return new ParityResult
            {
                    Passed = true,
                    Message = "Both strategies returned null"
            };

        if (legacyResult == null || cmsResult == null)
            return new ParityResult
            {
                    Passed = false,
                    Message = $"One strategy returned null: legacy={legacyResult == null}, cms={cmsResult == null}"
            };

        var timestampsMatch = legacyResult.Timestamps.Count == cmsResult.Timestamps.Count;
        var valuesMatch = legacyResult.PrimaryRawValues.Count == cmsResult.PrimaryRawValues.Count;

        if (!timestampsMatch || !valuesMatch)
            return new ParityResult
            {
                    Passed = false,
                    Message = $"Result counts differ: timestamps={timestampsMatch}, values={valuesMatch}"
            };

        return new ParityResult
        {
                Passed = true,
                Message = "Basic validation passed"
        };
    }

    private ParityResult ConvertParityResult(Validation.Parity.ParityResult harnessResult)
    {
        if (harnessResult.Passed)
            return new ParityResult
            {
                    Passed = true,
                    Message = "Parity validation passed"
            };

        var failureMessages = harnessResult.Failures.Select(f => $"[{f.Layer}] {f.Message}").
                                            ToList();

        return new ParityResult
        {
                Passed = false,
                Message = string.Join("; ", failureMessages),
                Details = new Dictionary<string, object>
                {
                        { "FailureCount", harnessResult.Failures.Count },
                        {
                                "Failures", harnessResult.Failures.Select(f => new
                                {
                                        f.Layer,
                                        f.Message
                                })
                        }
                }
        };
    }

    private IChartComputationStrategy CreateCmsStrategy(StrategyType strategyType, ChartDataContext ctx, StrategyCreationParameters parameters)
    {
        if (!_factories.TryGetValue(strategyType, out var factory))
            throw new NotSupportedException($"Strategy type {strategyType} is not supported");

        return factory.CreateCmsStrategy(ctx, parameters);
    }

    private IChartComputationStrategy CreateLegacyStrategy(StrategyType strategyType, StrategyCreationParameters parameters)
    {
        if (!_factories.TryGetValue(strategyType, out var factory))
            throw new NotSupportedException($"Strategy type {strategyType} is not supported");

        return factory.CreateLegacyStrategy(parameters);
    }
}