using DataFileReader.Canonical;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Strategies;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Reachability;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;

namespace DataVisualiser.UI.MainHost;

internal sealed class EvidenceParityBuilder
{
    private readonly Func<IStrategyCutOverService?> _getStrategyCutOverService;
    private readonly EvidenceDistributionParityEvaluator _distributionParityEvaluator;
    private readonly EvidenceMultiMetricParityEvaluator _multiMetricParityEvaluator;
    private readonly EvidenceTransformParityEvaluator _transformParityEvaluator;

    internal EvidenceParityBuilder(
        MetricSelectionService metricSelectionService,
        Func<IStrategyCutOverService?> getStrategyCutOverService,
        Func<string?> getSelectedTransformOperation)
    {
        ArgumentNullException.ThrowIfNull(metricSelectionService);
        _getStrategyCutOverService = getStrategyCutOverService ?? throw new ArgumentNullException(nameof(getStrategyCutOverService));
        ArgumentNullException.ThrowIfNull(getSelectedTransformOperation);
        _distributionParityEvaluator = new EvidenceDistributionParityEvaluator(metricSelectionService, getStrategyCutOverService);
        _multiMetricParityEvaluator = new EvidenceMultiMetricParityEvaluator(metricSelectionService, getStrategyCutOverService);
        _transformParityEvaluator = new EvidenceTransformParityEvaluator(metricSelectionService, getSelectedTransformOperation);
    }

    internal async Task<EvidenceParityBundle> BuildAsync(ChartState chartState, MetricState metricState, ChartDataContext? ctx)
    {
        var selectedSeries = metricState.SelectedSeries.ToList();
        var distributionParity = await _distributionParityEvaluator.BuildAsync(chartState, metricState, ctx);
        var combinedParity = BuildCombinedMetricParitySnapshot(ctx);
        var singleParity = BuildSingleMetricParitySnapshot(ctx);
        var multiParity = await _multiMetricParityEvaluator.BuildAsync(metricState, ctx);
        var normalizedParity = BuildNormalizedParitySnapshot(chartState, ctx);
        var weekdayTrendParity = BuildWeekdayTrendParitySnapshot(ctx);
        var transformParity = await _transformParityEvaluator.BuildAsync(chartState, metricState, ctx);
        var paritySummary = EvidenceParitySummaryBuilder.BuildSummary(distributionParity, combinedParity, singleParity, multiParity, normalizedParity, weekdayTrendParity, transformParity);
        var parityWarnings = EvidenceParitySummaryBuilder.BuildWarnings(distributionParity, combinedParity, singleParity, multiParity, normalizedParity, weekdayTrendParity, transformParity, selectedSeries.Count);

        return new EvidenceParityBundle(
            distributionParity,
            combinedParity,
            singleParity,
            multiParity,
            normalizedParity,
            weekdayTrendParity,
            transformParity,
            paritySummary,
            parityWarnings);
    }

    private IStrategyCutOverService ResolveStrategyCutOverService()
    {
        return _getStrategyCutOverService() ?? new StrategyCutOverService(new DataPreparationService(), StrategyReachabilityStoreProbe.Default);
    }

    private CombinedMetricParitySnapshot BuildCombinedMetricParitySnapshot(ChartDataContext? ctx)
    {
        if (ctx == null || ctx.Data1 == null || ctx.Data2 == null)
            return new CombinedMetricParitySnapshot { Status = "Unavailable", Reason = "Combined metric requires primary and secondary data" };

        if (ctx.PrimaryCms is not ICanonicalMetricSeries || ctx.SecondaryCms is not ICanonicalMetricSeries)
            return new CombinedMetricParitySnapshot { Status = "CmsUnavailable", Reason = "Combined metric CMS series missing" };

        var parameters = new StrategyCreationParameters
        {
            LegacyData1 = ctx.Data1,
            LegacyData2 = ctx.Data2,
            Label1 = ctx.DisplayName1,
            Label2 = ctx.DisplayName2,
            From = ctx.From,
            To = ctx.To
        };

        return new CombinedMetricParitySnapshot
        {
            Status = "Completed",
            Result = EvidenceStrategyParityExecutor.ExecuteSafe(ResolveStrategyCutOverService(), StrategyType.CombinedMetric, ctx, parameters)
        };
    }

    private SimpleParitySnapshot BuildSingleMetricParitySnapshot(ChartDataContext? ctx)
    {
        if (ctx == null || ctx.Data1 == null)
            return new SimpleParitySnapshot { Status = "Unavailable", Reason = "Primary series required" };

        if (ctx.PrimaryCms is not ICanonicalMetricSeries)
            return new SimpleParitySnapshot { Status = "CmsUnavailable", Reason = "Primary CMS series missing" };

        var parameters = new StrategyCreationParameters
        {
            LegacyData1 = ctx.Data1,
            Label1 = ctx.DisplayName1,
            From = ctx.From,
            To = ctx.To
        };

        return new SimpleParitySnapshot
        {
            Status = "Completed",
            Result = EvidenceStrategyParityExecutor.ExecuteSafe(ResolveStrategyCutOverService(), StrategyType.SingleMetric, ctx, parameters)
        };
    }

    private SimpleParitySnapshot BuildNormalizedParitySnapshot(ChartState chartState, ChartDataContext? ctx)
    {
        if (ctx == null || ctx.Data1 == null || ctx.Data2 == null)
            return new SimpleParitySnapshot { Status = "Unavailable", Reason = "Primary and secondary series required" };

        if (ctx.PrimaryCms is not ICanonicalMetricSeries || ctx.SecondaryCms is not ICanonicalMetricSeries)
            return new SimpleParitySnapshot { Status = "CmsUnavailable", Reason = "CMS series missing" };

        var parameters = new StrategyCreationParameters
        {
            LegacyData1 = ctx.Data1,
            LegacyData2 = ctx.Data2,
            Label1 = ctx.DisplayName1,
            Label2 = ctx.DisplayName2,
            From = ctx.From,
            To = ctx.To,
            NormalizationMode = chartState.SelectedNormalizationMode
        };

        return new SimpleParitySnapshot
        {
            Status = "Completed",
            Result = EvidenceStrategyParityExecutor.ExecuteSafe(ResolveStrategyCutOverService(), StrategyType.Normalized, ctx, parameters)
        };
    }

    private SimpleParitySnapshot BuildWeekdayTrendParitySnapshot(ChartDataContext? ctx)
    {
        if (ctx == null || ctx.Data1 == null)
            return new SimpleParitySnapshot { Status = "Unavailable", Reason = "Primary series required" };

        if (ctx.PrimaryCms is not ICanonicalMetricSeries)
            return new SimpleParitySnapshot { Status = "CmsUnavailable", Reason = "Primary CMS series missing" };

        var parameters = new StrategyCreationParameters
        {
            LegacyData1 = ctx.Data1,
            Label1 = ctx.DisplayName1,
            From = ctx.From,
            To = ctx.To
        };

        return new SimpleParitySnapshot
        {
            Status = "Completed",
            Result = EvidenceStrategyParityExecutor.ExecuteSafe(ResolveStrategyCutOverService(), StrategyType.WeekdayTrend, ctx, parameters)
        };
    }


}

internal sealed record EvidenceParityBundle(
    DistributionParitySnapshot DistributionParity,
    CombinedMetricParitySnapshot CombinedMetricParity,
    SimpleParitySnapshot SingleMetricParity,
    SimpleParitySnapshot MultiMetricParity,
    SimpleParitySnapshot NormalizedParity,
    SimpleParitySnapshot WeekdayTrendParity,
    TransformParitySnapshot TransformParity,
    ParitySummarySnapshot ParitySummary,
    IReadOnlyList<string> ParityWarnings);
