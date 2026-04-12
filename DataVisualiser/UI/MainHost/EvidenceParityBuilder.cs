using System.Diagnostics;
using DataFileReader.Canonical;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Strategies;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Reachability;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;

namespace DataVisualiser.UI.MainHost;

internal sealed class EvidenceParityBuilder
{
    private readonly Func<string?> _getSelectedTransformOperation;
    private readonly Func<IStrategyCutOverService?> _getStrategyCutOverService;
    private readonly EvidenceTransformParityEvaluator _transformParityEvaluator;
    private readonly MetricSelectionService _metricSelectionService;

    internal EvidenceParityBuilder(
        MetricSelectionService metricSelectionService,
        Func<IStrategyCutOverService?> getStrategyCutOverService,
        Func<string?> getSelectedTransformOperation)
    {
        _metricSelectionService = metricSelectionService ?? throw new ArgumentNullException(nameof(metricSelectionService));
        _getStrategyCutOverService = getStrategyCutOverService ?? throw new ArgumentNullException(nameof(getStrategyCutOverService));
        _getSelectedTransformOperation = getSelectedTransformOperation ?? throw new ArgumentNullException(nameof(getSelectedTransformOperation));
        _transformParityEvaluator = new EvidenceTransformParityEvaluator(metricSelectionService, getSelectedTransformOperation);
    }

    internal async Task<EvidenceParityBundle> BuildAsync(ChartState chartState, MetricState metricState, ChartDataContext? ctx)
    {
        var selectedSeries = metricState.SelectedSeries.ToList();
        var distributionParity = await BuildDistributionParitySnapshotAsync(chartState, metricState, ctx);
        var combinedParity = BuildCombinedMetricParitySnapshot(ctx);
        var singleParity = BuildSingleMetricParitySnapshot(ctx);
        var multiParity = await BuildMultiMetricParitySnapshotAsync(metricState, ctx);
        var normalizedParity = BuildNormalizedParitySnapshot(chartState, ctx);
        var weekdayTrendParity = BuildWeekdayTrendParitySnapshot(ctx);
        var transformParity = await _transformParityEvaluator.BuildAsync(chartState, metricState, ctx);
        var paritySummary = BuildParitySummary(distributionParity, combinedParity, singleParity, multiParity, normalizedParity, weekdayTrendParity, transformParity);
        var parityWarnings = BuildParityWarnings(distributionParity, combinedParity, singleParity, multiParity, normalizedParity, weekdayTrendParity, transformParity, selectedSeries.Count);

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

    private static ParitySummarySnapshot BuildParitySummary(DistributionParitySnapshot distributionSnapshot, CombinedMetricParitySnapshot combinedSnapshot, SimpleParitySnapshot singleSnapshot, SimpleParitySnapshot multiSnapshot, SimpleParitySnapshot normalizedSnapshot, SimpleParitySnapshot weekdayTrendSnapshot, TransformParitySnapshot transformSnapshot)
    {
        var weeklyPassed = distributionSnapshot.Weekly?.Passed;
        var hourlyPassed = distributionSnapshot.Hourly?.Passed;
        var combinedPassed = combinedSnapshot.Result?.Passed;
        var singlePassed = singleSnapshot.Result?.Passed;
        var multiPassed = multiSnapshot.Result?.Passed;
        var normalizedPassed = normalizedSnapshot.Result?.Passed;
        var weekdayTrendPassed = weekdayTrendSnapshot.Result?.Passed;
        var transformPassed = transformSnapshot.Result?.Passed;
        var completed = string.Equals(distributionSnapshot.Status, "Completed", StringComparison.OrdinalIgnoreCase);

        return new ParitySummarySnapshot
        {
            Status = distributionSnapshot.Status,
            WeeklyPassed = weeklyPassed,
            HourlyPassed = hourlyPassed,
            CombinedMetricPassed = combinedPassed,
            SingleMetricPassed = singlePassed,
            MultiMetricPassed = multiPassed,
            NormalizedPassed = normalizedPassed,
            WeekdayTrendPassed = weekdayTrendPassed,
            TransformPassed = transformPassed,
            OverallPassed = completed && weeklyPassed == true && hourlyPassed == true && combinedPassed != false && singlePassed != false && multiPassed != false && normalizedPassed != false && weekdayTrendPassed != false && transformPassed != false,
            StrategiesEvaluated =
            [
                "WeeklyDistribution",
                "HourlyDistribution",
                "CombinedMetric",
                "SingleMetric",
                "MultiMetric",
                "Normalized",
                "WeekdayTrend",
                "Transform"
            ]
        };
    }

    private static IReadOnlyList<string> BuildParityWarnings(DistributionParitySnapshot distributionSnapshot, CombinedMetricParitySnapshot combinedSnapshot, SimpleParitySnapshot singleSnapshot, SimpleParitySnapshot multiSnapshot, SimpleParitySnapshot normalizedSnapshot, SimpleParitySnapshot weekdayTrendSnapshot, TransformParitySnapshot transformSnapshot, int selectedSeriesCount)
    {
        var warnings = new List<string>();
        AddWarningIfUnavailable(warnings, "WeeklyDistribution", distributionSnapshot.Status, distributionSnapshot.Reason);
        AddWarningIfUnavailable(warnings, "CombinedMetric", combinedSnapshot.Status, combinedSnapshot.Reason);
        AddWarningIfUnavailable(warnings, "SingleMetric", singleSnapshot.Status, singleSnapshot.Reason);
        AddWarningIfUnavailable(warnings, "MultiMetric", multiSnapshot.Status, multiSnapshot.Reason);
        AddWarningIfUnavailable(warnings, "Normalized", normalizedSnapshot.Status, normalizedSnapshot.Reason);
        AddWarningIfUnavailable(warnings, "WeekdayTrend", weekdayTrendSnapshot.Status, weekdayTrendSnapshot.Reason);
        AddWarningIfUnavailable(warnings, "Transform", transformSnapshot.Status, transformSnapshot.Reason);

        if (selectedSeriesCount < 2)
            warnings.Add("Multiple series required for CombinedMetric/Normalized/Transform parity; select at least two series.");

        return warnings;
    }

    private static void AddWarningIfUnavailable(List<string> warnings, string label, string status, string? reason)
    {
        if (string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase))
            return;

        var detail = string.IsNullOrWhiteSpace(reason) ? "Unavailable" : reason;
        warnings.Add($"{label} parity not completed: {detail}");
    }

    private IStrategyCutOverService ResolveStrategyCutOverService()
    {
        return _getStrategyCutOverService() ?? new StrategyCutOverService(new DataPreparationService(), StrategyReachabilityStoreProbe.Default);
    }

    private static int CountCmsSamples(ICanonicalMetricSeries? series, DateTime? from = null, DateTime? to = null)
    {
        if (series?.Samples == null)
            return 0;

        if (!from.HasValue || !to.HasValue)
            return series.Samples.Count(s => s.Value.HasValue);

        var toEndOfDay = to.Value.Date.AddDays(1).AddTicks(-1);
        var fromStartOfDay = from.Value.Date;
        return series.Samples.Count(s => s.Value.HasValue && s.Timestamp.LocalDateTime >= fromStartOfDay && s.Timestamp.LocalDateTime <= toEndOfDay);
    }

    private async Task<DistributionParitySnapshot> BuildDistributionParitySnapshotAsync(ChartState chartState, MetricState metricState, ChartDataContext? ctx)
    {
        if (ctx == null || ctx.From == default || ctx.To == default)
            return UnavailableDistribution("No chart context available");

        var selection = ResolveDistributionSelection(chartState, ctx);
        if (selection == null)
            return UnavailableDistribution("No distribution series selected");

        var tableName = metricState.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
        var (legacyData, cmsSeries, dataSource) = await ResolveDistributionParityDataAsync(ctx, selection, tableName);
        if (legacyData == null || legacyData.Count == 0)
            return UnavailableDistribution("No legacy distribution data available", dataSource);

        var cmsSampleTotal = CountCmsSamples(cmsSeries);
        var cmsSampleInRange = CountCmsSamples(cmsSeries, ctx.From, ctx.To);
        if (cmsSeries == null)
        {
            return new DistributionParitySnapshot
            {
                Status = "CmsUnavailable",
                DataSource = dataSource,
                CmsSamplesTotal = cmsSampleTotal,
                CmsSamplesInRange = cmsSampleInRange
            };
        }

        var parameters = new StrategyCreationParameters
        {
            LegacyData1 = legacyData,
            Label1 = selection.DisplayName ?? ctx.DisplayName1 ?? string.Empty,
            From = ctx.From,
            To = ctx.To
        };
        var parityContext = new ChartDataContext
        {
            PrimaryCms = cmsSeries,
            Data1 = legacyData,
            DisplayName1 = selection.DisplayName ?? ctx.DisplayName1 ?? string.Empty,
            MetricType = selection.MetricType ?? ctx.MetricType,
            PrimaryMetricType = selection.MetricType ?? ctx.PrimaryMetricType,
            PrimarySubtype = selection.Subtype,
            DisplayPrimaryMetricType = selection.DisplayMetricType ?? ctx.DisplayPrimaryMetricType,
            DisplayPrimarySubtype = selection.DisplaySubtype ?? ctx.DisplayPrimarySubtype,
            From = ctx.From,
            To = ctx.To
        };

        var strategyService = ResolveStrategyCutOverService();
        return new DistributionParitySnapshot
        {
            Status = "Completed",
            Selection = new
            {
                selection.MetricType,
                selection.Subtype,
                selection.DisplayMetricType,
                selection.DisplaySubtype,
                selection.DisplayName,
                selection.DisplayKey
            },
            DataSource = dataSource,
            LegacySamples = legacyData.Count,
            CmsSamplesTotal = cmsSampleTotal,
            CmsSamplesInRange = cmsSampleInRange,
            Weekly = ExecuteParitySafe(strategyService, StrategyType.WeeklyDistribution, parityContext, parameters),
            Hourly = ExecuteParitySafe(strategyService, StrategyType.HourlyDistribution, parityContext, parameters)
        };
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
            Result = ExecuteParitySafe(ResolveStrategyCutOverService(), StrategyType.CombinedMetric, ctx, parameters)
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
            Result = ExecuteParitySafe(ResolveStrategyCutOverService(), StrategyType.SingleMetric, ctx, parameters)
        };
    }

    private async Task<SimpleParitySnapshot> BuildMultiMetricParitySnapshotAsync(MetricState metricState, ChartDataContext? ctx)
    {
        if (ctx == null || ctx.Data1 == null)
            return new SimpleParitySnapshot { Status = "Unavailable", Reason = "Primary series required" };

        var selectedSeries = metricState.SelectedSeries
            .GroupBy(series => series.DisplayKey, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();

        if (selectedSeries.Count < 3)
            return new SimpleParitySnapshot { Status = "Unavailable", Reason = "At least three series required" };

        var tableName = metricState.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
        var resolved = await ResolveMultiMetricParityInputsAsync(ctx, selectedSeries, tableName);
        if (resolved.LegacySeries.Count < 3)
            return new SimpleParitySnapshot { Status = "Unavailable", Reason = "Insufficient multi-series legacy data available" };

        if (resolved.CmsSeries.Count != resolved.LegacySeries.Count)
            return new SimpleParitySnapshot { Status = "CmsUnavailable", Reason = "CMS multi-series data missing" };

        var parityContext = new ChartDataContext
        {
            PrimaryCms = resolved.CmsSeries.FirstOrDefault(),
            SecondaryCms = resolved.CmsSeries.Count > 1 ? resolved.CmsSeries[1] : null,
            CmsSeries = resolved.CmsSeries,
            Data1 = resolved.LegacySeries[0].ToList(),
            Data2 = resolved.LegacySeries.Count > 1 ? resolved.LegacySeries[1].ToList() : Array.Empty<MetricData>(),
            DisplayName1 = resolved.Labels[0],
            DisplayName2 = resolved.Labels.Count > 1 ? resolved.Labels[1] : string.Empty,
            ActualSeriesCount = resolved.LegacySeries.Count,
            From = ctx.From,
            To = ctx.To
        };
        var parameters = new StrategyCreationParameters
        {
            LegacySeries = resolved.LegacySeries,
            CmsSeries = resolved.CmsSeries,
            Labels = resolved.Labels,
            From = ctx.From,
            To = ctx.To
        };

        return new SimpleParitySnapshot
        {
            Status = "Completed",
            Result = ExecuteParitySafe(ResolveStrategyCutOverService(), StrategyType.MultiMetric, parityContext, parameters)
        };
    }

    private async Task<(List<IEnumerable<MetricData>> LegacySeries, List<ICanonicalMetricSeries> CmsSeries, List<string> Labels)> ResolveMultiMetricParityInputsAsync(ChartDataContext ctx, IReadOnlyList<MetricSeriesSelection> selectedSeries, string tableName)
    {
        var legacySeries = new List<IEnumerable<MetricData>>();
        var cmsSeries = new List<ICanonicalMetricSeries>();
        var labels = new List<string>();

        foreach (var selection in selectedSeries)
        {
            var label = string.IsNullOrWhiteSpace(selection.DisplayName) ? selection.DisplayKey : selection.DisplayName;
            var (legacyData, cmsData) = await ResolveMultiMetricParitySeriesAsync(ctx, selection, tableName);
            if (legacyData == null || legacyData.Count == 0)
                continue;

            legacySeries.Add(legacyData);
            labels.Add(label);
            if (cmsData != null)
                cmsSeries.Add(cmsData);
        }

        return (legacySeries, cmsSeries, labels);
    }

    private async Task<(IReadOnlyList<MetricData>? Data, ICanonicalMetricSeries? Cms)> ResolveMultiMetricParitySeriesAsync(ChartDataContext ctx, MetricSeriesSelection selection, string tableName)
    {
        if (ctx.Data1 != null && EvidenceDiagnosticsBuilder.IsSameSelection(selection, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return (ctx.Data1, ctx.PrimaryCms as ICanonicalMetricSeries);

        if (ctx.Data2 != null && EvidenceDiagnosticsBuilder.IsSameSelection(selection, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return (ctx.Data2, ctx.SecondaryCms as ICanonicalMetricSeries);

        if (ctx.CmsSeries != null)
        {
            var targetMetricId = CanonicalMetricMapping.FromLegacyFields(selection.MetricType, selection.QuerySubtype);
            var matchingCms = ctx.CmsSeries.FirstOrDefault(series => string.Equals(series.MetricId?.Value, targetMetricId, StringComparison.OrdinalIgnoreCase));
            if (matchingCms != null)
            {
                var legacyData = await ResolveLegacyDataForSelectionAsync(ctx, selection, tableName);
                return (legacyData, matchingCms);
            }
        }

        var (primaryCms, _, primaryData, _) = await _metricSelectionService.LoadMetricDataWithCmsAsync(selection, null, ctx.From, ctx.To, tableName);
        return (primaryData.ToList(), primaryCms);
    }

    private async Task<IReadOnlyList<MetricData>?> ResolveLegacyDataForSelectionAsync(ChartDataContext ctx, MetricSeriesSelection selection, string tableName)
    {
        if (ctx.Data1 != null && EvidenceDiagnosticsBuilder.IsSameSelection(selection, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return ctx.Data1;

        if (ctx.Data2 != null && EvidenceDiagnosticsBuilder.IsSameSelection(selection, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return ctx.Data2;

        if (string.IsNullOrWhiteSpace(selection.MetricType))
            return ctx.Data1;

        var (primaryData, _) = await _metricSelectionService.LoadMetricDataAsync(selection.MetricType, selection.QuerySubtype, null, ctx.From, ctx.To, tableName);
        return primaryData.ToList();
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
            Result = ExecuteParitySafe(ResolveStrategyCutOverService(), StrategyType.Normalized, ctx, parameters)
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
            Result = ExecuteParitySafe(ResolveStrategyCutOverService(), StrategyType.WeekdayTrend, ctx, parameters)
        };
    }


    private static MetricSeriesSelection? ResolveDistributionSelection(ChartState chartState, ChartDataContext ctx)
    {
        if (chartState.SelectedDistributionSeries != null)
            return chartState.SelectedDistributionSeries;

        var metricType = ctx.PrimaryMetricType ?? ctx.MetricType;
        return string.IsNullOrWhiteSpace(metricType) ? null : new MetricSeriesSelection(metricType, ctx.PrimarySubtype);
    }

    private async Task<(IReadOnlyList<MetricData>? Data, ICanonicalMetricSeries? Cms, string DataSource)> ResolveDistributionParityDataAsync(ChartDataContext ctx, MetricSeriesSelection selection, string tableName)
    {
        if (ctx.Data1 != null && EvidenceDiagnosticsBuilder.IsSameSelection(selection, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return (ctx.Data1, ctx.PrimaryCms as ICanonicalMetricSeries, "ChartContext.Primary");

        if (ctx.Data2 != null && EvidenceDiagnosticsBuilder.IsSameSelection(selection, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return (ctx.Data2, ctx.SecondaryCms as ICanonicalMetricSeries, "ChartContext.Secondary");

        if (string.IsNullOrWhiteSpace(selection.MetricType))
            return (ctx.Data1, ctx.PrimaryCms as ICanonicalMetricSeries, "ChartContext.Fallback");

        var (primaryCms, _, primaryData, _) = await _metricSelectionService.LoadMetricDataWithCmsAsync(selection, null, ctx.From, ctx.To, tableName);
        return (primaryData.ToList(), primaryCms, "MetricSelectionService");
    }

    private ParityResultSnapshot ExecuteParitySafe(IStrategyCutOverService strategyCutOverService, StrategyType strategyType, ChartDataContext ctx, StrategyCreationParameters parameters)
    {
        try
        {
            var legacy = strategyCutOverService.CreateLegacyStrategy(strategyType, parameters);
            var cms = strategyCutOverService.CreateCmsStrategy(strategyType, ctx, parameters);
            var result = strategyCutOverService.ValidateParity(legacy, cms);
            return new ParityResultSnapshot { Passed = result.Passed, Message = result.Message, Details = result.Details };
        }
        catch (Exception ex)
        {
            return new ParityResultSnapshot { Passed = false, Error = ex.Message };
        }
    }

    private static DistributionParitySnapshot UnavailableDistribution(string reason, string? dataSource = null)
    {
        Debug.WriteLine($"[ParityExport] Unavailable: {reason}");
        return new DistributionParitySnapshot
        {
            Status = "Unavailable",
            Reason = reason,
            DataSource = dataSource
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
