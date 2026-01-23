using System.Diagnostics;
using DataFileReader.Canonical;
using DataVisualiser.Core.Configuration;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Factories;
using DataVisualiser.Core.Strategies.Reachability;
using DataVisualiser.Core.Validation.Parity;
using ParityResult = DataVisualiser.Core.Validation.ParityResult;

namespace DataVisualiser.Core.Strategies;

/// <summary>
///     Implementation of IStrategyCutOverService.
///     Provides unified cut-over mechanism for all strategies with parity validation.
/// </summary>
public sealed class StrategyCutOverService : IStrategyCutOverService
{
    private readonly Dictionary<StrategyType, IStrategyFactory> _factories;

    private readonly IStrategyReachabilityProbe _reachabilityProbe;

    public StrategyCutOverService(IDataPreparationService dataPreparation, IStrategyReachabilityProbe? reachabilityProbe = null)
    {
        _ = dataPreparation ?? throw new ArgumentNullException(nameof(dataPreparation));
        _reachabilityProbe = reachabilityProbe ?? NullStrategyReachabilityProbe.Instance;

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
        {
            Debug.WriteLine($"[ShouldUseCms] Global CMS disabled: UseCmsData={CmsConfiguration.UseCmsData}");
            return false;
        }

        // Check strategy-specific configuration
        var strategyName = StrategyTypeMetadata.GetConfigName(strategyType);

        if (strategyName != null && !CmsConfiguration.ShouldUseCms(strategyName))
        {
            Debug.WriteLine($"[ShouldUseCms] Strategy-specific CMS disabled: {strategyName}={CmsConfiguration.ShouldUseCms(strategyName)}");
            return false;
        }

        // Check if CMS data is available (filtered by date range)
        return strategyType switch
        {
                StrategyType.SingleMetric => HasSufficientCmsSamples(ctx.PrimaryCms, ctx.From, ctx.To),
                StrategyType.CombinedMetric => HasSufficientCmsSamples(ctx.PrimaryCms, ctx.From, ctx.To) && HasSufficientCmsSamples(ctx.SecondaryCms, ctx.From, ctx.To),
                StrategyType.MultiMetric => HasSufficientCmsSamples(ctx.PrimaryCms, ctx.From, ctx.To), // At least primary
                StrategyType.Difference => HasSufficientCmsSamples(ctx.PrimaryCms, ctx.From, ctx.To) && HasSufficientCmsSamples(ctx.SecondaryCms, ctx.From, ctx.To),
                StrategyType.Ratio => HasSufficientCmsSamples(ctx.PrimaryCms, ctx.From, ctx.To) && HasSufficientCmsSamples(ctx.SecondaryCms, ctx.From, ctx.To),
                StrategyType.Normalized => HasSufficientCmsSamples(ctx.PrimaryCms, ctx.From, ctx.To) && HasSufficientCmsSamples(ctx.SecondaryCms, ctx.From, ctx.To),
                StrategyType.WeeklyDistribution => HasSufficientCmsSamples(ctx.PrimaryCms, ctx.From, ctx.To),
                StrategyType.HourlyDistribution => HasSufficientCmsSamples(ctx.PrimaryCms, ctx.From, ctx.To),
                StrategyType.WeekdayTrend => HasSufficientCmsSamples(ctx.PrimaryCms, ctx.From, ctx.To),
                _ => false
        };
    }

    public IChartComputationStrategy CreateStrategy(StrategyType strategyType, ChartDataContext ctx, StrategyCreationParameters parameters)
    {
        var useCms = ShouldUseCms(strategyType, ctx);
        Debug.WriteLine($"[CMS] {strategyType}: PrimarySamples={GetSampleCount(ctx.PrimaryCms, ctx.From, ctx.To)} (filtered), TotalPrimarySamples={GetSampleCount(ctx.PrimaryCms)}, SecondarySamples={GetSampleCount(ctx.SecondaryCms, ctx.From, ctx.To)} (filtered), TotalSecondarySamples={GetSampleCount(ctx.SecondaryCms)}");
        Debug.WriteLine($"[CutOver] Strategy={strategyType}, UseCms={useCms}, PrimaryCms={(ctx.PrimaryCms == null ? "NULL" : "SET")}, SecondaryCms={(ctx.SecondaryCms == null ? "NULL" : "SET")}, DateRange=[{ctx.From:yyyy-MM-dd} to {ctx.To:yyyy-MM-dd}]");
        _reachabilityProbe.Record(StrategyReachabilityRecord.Create(strategyType, useCms, ctx, GetSampleCount(ctx.PrimaryCms, ctx.From, ctx.To), GetSampleCount(ctx.SecondaryCms, ctx.From, ctx.To)));

        if (useCms)
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

    public IChartComputationStrategy CreateCmsStrategy(StrategyType strategyType, ChartDataContext ctx, StrategyCreationParameters parameters)
    {
        if (!_factories.TryGetValue(strategyType, out var factory))
            throw new NotSupportedException($"Strategy type {strategyType} is not supported");

        return factory.CreateCmsStrategy(ctx, parameters);
    }

    public IChartComputationStrategy CreateLegacyStrategy(StrategyType strategyType, StrategyCreationParameters parameters)
    {
        if (!_factories.TryGetValue(strategyType, out var factory))
            throw new NotSupportedException($"Strategy type {strategyType} is not supported");

        return factory.CreateLegacyStrategy(parameters);
    }

    private static bool HasSufficientCmsSamples(object? series, DateTime from, DateTime to, int minSamples = 1)
    {
        if (series is not ICanonicalMetricSeries cmsSeries)
        {
            Debug.WriteLine("[HasSufficientCmsSamples] series is not ICanonicalMetricSeries");
            return false;
        }

        if (cmsSeries.Samples == null)
        {
            Debug.WriteLine("[HasSufficientCmsSamples] Samples is null");
            return false;
        }

        var totalSamples = cmsSeries.Samples.Count;
        var validValueSamples = cmsSeries.Samples.Count(s => s.Value.HasValue);

        // Extend 'to' to end of day to include all samples on that date
        var toEndOfDay = to.Date.AddDays(1).AddTicks(-1);
        var fromStartOfDay = from.Date;

        // Count samples within the date range (compare dates, not times)
        var filteredCount = cmsSeries.Samples.Count(s => s.Value.HasValue && s.Timestamp.LocalDateTime >= fromStartOfDay && s.Timestamp.LocalDateTime <= toEndOfDay);

        // Debug: Show sample range if available
        if (totalSamples > 0)
        {
            var firstSample = cmsSeries.Samples.First();
            var lastSample = cmsSeries.Samples.Last();
            var samplesInRange = cmsSeries.Samples.Where(s => s.Value.HasValue && s.Timestamp.LocalDateTime >= fromStartOfDay && s.Timestamp.LocalDateTime <= toEndOfDay).Take(5).ToList();
            Debug.WriteLine($"[HasSufficientCmsSamples] Total={totalSamples}, ValidValues={validValueSamples}, Filtered={filteredCount}, Range=[{fromStartOfDay:yyyy-MM-dd} to {toEndOfDay:yyyy-MM-dd HH:mm:ss}], FirstSample={firstSample.Timestamp.LocalDateTime:yyyy-MM-dd HH:mm:ss}, LastSample={lastSample.Timestamp.LocalDateTime:yyyy-MM-dd HH:mm:ss}");
            if (samplesInRange.Any())
                Debug.WriteLine($"[HasSufficientCmsSamples] Sample timestamps in range: {string.Join(", ", samplesInRange.Select(s => s.Timestamp.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss")))}");
        }

        return filteredCount >= minSamples;
    }

    private static int GetSampleCount(object? series, DateTime? from = null, DateTime? to = null)
    {
        if (series is not ICanonicalMetricSeries cmsSeries)
            return 0;

        if (cmsSeries.Samples == null)
            return 0;

        // If date range is provided, count filtered samples; otherwise count all
        // Extend 'to' to end of day to include all samples on that date
        if (from.HasValue && to.HasValue)
        {
            var toEndOfDay = to.Value.Date.AddDays(1).AddTicks(-1);
            var fromStartOfDay = from.Value.Date;
            return cmsSeries.Samples.Count(s => s.Value.HasValue && s.Timestamp.LocalDateTime >= fromStartOfDay && s.Timestamp.LocalDateTime <= toEndOfDay);
        }

        return cmsSeries.Samples.Count;
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
        if (!strategyType.HasValue)
            return null;

        return StrategyTypeMetadata.CreateParityHarness(strategyType.Value);
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