using DataFileReader.Canonical;
using DataVisualiser.Core.Configuration;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Strategies.Abstractions;

namespace DataVisualiser.Core.Strategies.Reachability;

public sealed class StrategyCmsDecisionEvaluator
{
    private readonly ICmsRuntimeConfiguration _cmsRuntimeConfiguration;

    public StrategyCmsDecisionEvaluator(ICmsRuntimeConfiguration cmsRuntimeConfiguration)
    {
        _cmsRuntimeConfiguration = cmsRuntimeConfiguration ?? throw new ArgumentNullException(nameof(cmsRuntimeConfiguration));
    }

    public StrategyCmsDecision Evaluate(StrategyType strategyType, ChartDataContext ctx)
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

    public int GetSampleCount(object? series, DateTime? from = null, DateTime? to = null)
    {
        if (series is not ICanonicalMetricSeries cmsSeries)
            return 0;

        if (cmsSeries.Samples == null)
            return 0;

        if (from.HasValue && to.HasValue)
        {
            var toEndOfDay = to.Value.Date.AddDays(1).AddTicks(-1);
            var fromStartOfDay = from.Value.Date;
            return cmsSeries.Samples.Count(s => s.Value.HasValue && s.Timestamp.LocalDateTime >= fromStartOfDay && s.Timestamp.LocalDateTime <= toEndOfDay);
        }

        return cmsSeries.Samples.Count;
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

    private static bool HasSufficientCmsSamples(object? series, DateTime from, DateTime to, int minSamples = 1)
    {
        if (series is not ICanonicalMetricSeries cmsSeries)
            return false;

        if (cmsSeries.Samples == null)
            return false;

        var toEndOfDay = to.Date.AddDays(1).AddTicks(-1);
        var fromStartOfDay = from.Date;
        var filteredCount = cmsSeries.Samples.Count(s => s.Value.HasValue && s.Timestamp.LocalDateTime >= fromStartOfDay && s.Timestamp.LocalDateTime <= toEndOfDay);

        return filteredCount >= minSamples;
    }

    private static bool SupportsRealCmsStrategy(StrategyType strategyType)
    {
        return strategyType switch
        {
            StrategyType.SingleMetric => true,
            StrategyType.CombinedMetric => true,
            StrategyType.MultiMetric => true,
            StrategyType.Difference => true,
            StrategyType.Ratio => true,
            StrategyType.Normalized => true,
            StrategyType.WeeklyDistribution => true,
            StrategyType.HourlyDistribution => true,
            StrategyType.WeekdayTrend => true,
            _ => false
        };
    }
}
