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
    private readonly ICmsRuntimeConfiguration _cmsRuntimeConfiguration;
    private readonly Dictionary<StrategyType, IStrategyFactory> _factories;

    private readonly IStrategyReachabilityProbe _reachabilityProbe;

    public StrategyCutOverService(IDataPreparationService dataPreparation, IStrategyReachabilityProbe? reachabilityProbe = null, ICmsRuntimeConfiguration? cmsRuntimeConfiguration = null)
    {
        _ = dataPreparation ?? throw new ArgumentNullException(nameof(dataPreparation));
        _cmsRuntimeConfiguration = cmsRuntimeConfiguration ?? StaticCmsRuntimeConfiguration.Instance;
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
        return EvaluateCmsDecision(strategyType, ctx).UseCms;
    }

    public IChartComputationStrategy CreateStrategy(StrategyType strategyType, ChartDataContext ctx, StrategyCreationParameters parameters)
    {
        var decision = EvaluateCmsDecision(strategyType, ctx);
        Debug.WriteLine($"[CMS] {strategyType}: PrimarySamples={decision.PrimarySamples} (filtered), TotalPrimarySamples={GetSampleCount(ctx.PrimaryCms)}, SecondarySamples={decision.SecondarySamples} (filtered), TotalSecondarySamples={GetSampleCount(ctx.SecondaryCms)}");
        Debug.WriteLine($"[CutOver] Strategy={strategyType}, UseCms={decision.UseCms}, CmsRequested={decision.CmsRequested}, Reason={decision.Reason}, PrimaryCms={(ctx.PrimaryCms == null ? "NULL" : "SET")}, SecondaryCms={(ctx.SecondaryCms == null ? "NULL" : "SET")}, DateRange=[{ctx.From:yyyy-MM-dd} to {ctx.To:yyyy-MM-dd}]");
        _reachabilityProbe.Record(StrategyReachabilityRecord.Create(strategyType, ctx, decision));

        if (decision.UseCms)
            return CreateCmsStrategy(strategyType, ctx, parameters);

        return CreateLegacyStrategy(strategyType, parameters);
    }

    private StrategyCmsDecision EvaluateCmsDecision(StrategyType strategyType, ChartDataContext ctx)
    {
        var primarySamples = GetSampleCount(ctx.PrimaryCms, ctx.From, ctx.To);
        var secondarySamples = GetSampleCount(ctx.SecondaryCms, ctx.From, ctx.To);
        var realCmsSupported = SupportsRealCmsStrategy(strategyType);
        if (!realCmsSupported)
            return new StrategyCmsDecision(false, false, _cmsRuntimeConfiguration.UseCmsData, false, false, primarySamples, secondarySamples, "No real CMS implementation exists");

        if (!_cmsRuntimeConfiguration.UseCmsData)
            return new StrategyCmsDecision(false, false, false, false, true, primarySamples, secondarySamples, "Global CMS disabled");

        var strategyName = StrategyTypeMetadata.GetConfigName(strategyType);
        var strategyCmsEnabled = strategyName == null || _cmsRuntimeConfiguration.ShouldUseCms(strategyName);
        if (!strategyCmsEnabled)
            return new StrategyCmsDecision(false, false, true, false, true, primarySamples, secondarySamples, "Strategy-specific CMS disabled");

        return strategyType switch
        {
                StrategyType.SingleMetric => CreateAvailabilityDecision(HasSufficientCmsSamples(ctx.PrimaryCms, ctx.From, ctx.To), primarySamples, secondarySamples, "Primary CMS data available", "Primary CMS data unavailable in selected range"),
                StrategyType.CombinedMetric => CreateAvailabilityDecision(HasSufficientCmsSamples(ctx.PrimaryCms, ctx.From, ctx.To) && HasSufficientCmsSamples(ctx.SecondaryCms, ctx.From, ctx.To), primarySamples, secondarySamples, "Primary and secondary CMS data available", "Primary or secondary CMS data unavailable in selected range"),
                StrategyType.MultiMetric => CreateMultiMetricDecision(ctx, primarySamples, secondarySamples),
                StrategyType.Difference => CreateAvailabilityDecision(HasSufficientCmsSamples(ctx.PrimaryCms, ctx.From, ctx.To) && HasSufficientCmsSamples(ctx.SecondaryCms, ctx.From, ctx.To), primarySamples, secondarySamples, "Primary and secondary CMS data available", "Primary or secondary CMS data unavailable in selected range"),
                StrategyType.Ratio => CreateAvailabilityDecision(HasSufficientCmsSamples(ctx.PrimaryCms, ctx.From, ctx.To) && HasSufficientCmsSamples(ctx.SecondaryCms, ctx.From, ctx.To), primarySamples, secondarySamples, "Primary and secondary CMS data available", "Primary or secondary CMS data unavailable in selected range"),
                StrategyType.Normalized => CreateAvailabilityDecision(HasSufficientCmsSamples(ctx.PrimaryCms, ctx.From, ctx.To) && HasSufficientCmsSamples(ctx.SecondaryCms, ctx.From, ctx.To), primarySamples, secondarySamples, "Primary and secondary CMS data available", "Primary or secondary CMS data unavailable in selected range"),
                StrategyType.WeeklyDistribution => CreateAvailabilityDecision(HasSufficientCmsSamples(ctx.PrimaryCms, ctx.From, ctx.To), primarySamples, secondarySamples, "Primary CMS data available", "Primary CMS data unavailable in selected range"),
                StrategyType.HourlyDistribution => CreateAvailabilityDecision(HasSufficientCmsSamples(ctx.PrimaryCms, ctx.From, ctx.To), primarySamples, secondarySamples, "Primary CMS data available", "Primary CMS data unavailable in selected range"),
                StrategyType.WeekdayTrend => CreateAvailabilityDecision(HasSufficientCmsSamples(ctx.PrimaryCms, ctx.From, ctx.To), primarySamples, secondarySamples, "Primary CMS data available", "Primary CMS data unavailable in selected range"),
                _ => new StrategyCmsDecision(false, true, true, true, true, primarySamples, secondarySamples, "Unsupported strategy type")
        };
    }

    private static StrategyCmsDecision CreateAvailabilityDecision(bool available, int primarySamples, int secondarySamples, string successReason, string failureReason)
    {
        return new StrategyCmsDecision(available, true, true, true, true, primarySamples, secondarySamples, available ? successReason : failureReason);
    }

    private static StrategyCmsDecision CreateMultiMetricDecision(ChartDataContext ctx, int primarySamples, int secondarySamples)
    {
        if (ctx.CmsSeries == null || ctx.CmsSeries.Count == 0)
            return new StrategyCmsDecision(false, true, true, true, true, primarySamples, secondarySamples, "CMS multi-series data unavailable");

        if (ctx.CmsSeries.Any(item => item == null || item.MetricId == null))
            return new StrategyCmsDecision(false, true, true, true, true, primarySamples, secondarySamples, "CMS multi-series identities are incomplete");

        if (!MetricCompatibilityHelper.ValidateCompatibility(ctx.CmsSeries))
            return new StrategyCmsDecision(false, true, true, true, true, primarySamples, secondarySamples, MetricCompatibilityHelper.GetIncompatibilityReason(ctx.CmsSeries) ?? "CMS multi-series metrics are incompatible");

        if (!ctx.CmsSeries.All(item => HasSufficientCmsSamples(item, ctx.From, ctx.To)))
            return new StrategyCmsDecision(false, true, true, true, true, primarySamples, secondarySamples, "One or more CMS series lacks samples in the selected range");

        return new StrategyCmsDecision(true, true, true, true, true, primarySamples, secondarySamples, "Compatible CMS multi-series data available");
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

    private static bool HasSufficientCmsSeries(IReadOnlyList<ICanonicalMetricSeries>? series, DateTime from, DateTime to)
    {
        if (series == null || series.Count == 0)
            return false;

        if (series.Any(item => item == null || item.MetricId == null))
            return false;

        if (!MetricCompatibilityHelper.ValidateCompatibility(series))
            return false;

        return series.All(item => HasSufficientCmsSamples(item, from, to));
    }

    private static bool SupportsRealCmsStrategy(StrategyType strategyType)
    {
        return strategyType switch
        {
            StrategyType.SingleMetric => true,
            StrategyType.CombinedMetric => true,
            StrategyType.MultiMetric => true,
            StrategyType.WeeklyDistribution => true,
            StrategyType.HourlyDistribution => true,
            StrategyType.WeekdayTrend => true,
            StrategyType.Difference => false,
            StrategyType.Ratio => false,
            StrategyType.Normalized => false,
            _ => false
        };
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
