using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Validation.Parity;
using ParityResult = DataVisualiser.Core.Validation.ParityResult;

namespace DataVisualiser.Core.Strategies;

internal sealed class StrategyParityValidationService
{
    public ParityResult ValidateParity(IChartComputationStrategy legacyStrategy, IChartComputationStrategy cmsStrategy)
    {
        if (legacyStrategy == null || cmsStrategy == null)
            return new ParityResult
            {
                    Passed = false,
                    Message = "One or both strategies are null"
            };

        var strategyType = DetermineStrategyType(legacyStrategy, cmsStrategy);
        var harness = GetParityHarness(strategyType);
        if (harness == null)
            return PerformBasicValidation(legacyStrategy, cmsStrategy);

        var context = new StrategyParityContext
        {
                StrategyName = strategyType?.ToString() ?? "Unknown",
                MetricIdentity = "ParityValidation",
                Mode = ParityMode.Diagnostic
        };

        var harnessResult = harness.Validate(
            context,
            () => ParityResultAdapter.ToLegacyExecutionResult(legacyStrategy.Compute()),
            () => ParityResultAdapter.ToCmsExecutionResult(cmsStrategy.Compute()));

        return ConvertParityResult(harnessResult);
    }

    internal IStrategyParityHarness? GetParityHarness(StrategyType? strategyType)
    {
        if (!strategyType.HasValue)
            return null;

        return StrategyTypeMetadata.CreateParityHarness(strategyType.Value);
    }

    private static StrategyType? DetermineStrategyType(IChartComputationStrategy legacyStrategy, IChartComputationStrategy cmsStrategy)
    {
        var legacyTypeName = legacyStrategy.GetType().Name;
        var cmsTypeName = cmsStrategy.GetType().Name;

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

    private static ParityResult PerformBasicValidation(IChartComputationStrategy legacyStrategy, IChartComputationStrategy cmsStrategy)
    {
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

    private static ParityResult ConvertParityResult(Validation.Parity.ParityResult harnessResult)
    {
        if (harnessResult.Passed)
            return new ParityResult
            {
                    Passed = true,
                    Message = "Parity validation passed"
            };

        var failureMessages = harnessResult.Failures.Select(f => $"[{f.Layer}] {f.Message}").ToList();

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
}
