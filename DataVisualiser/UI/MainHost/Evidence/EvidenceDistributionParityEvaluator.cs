using System.Diagnostics;
using DataFileReader.Canonical;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Strategies;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Reachability;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;

namespace DataVisualiser.UI.MainHost.Evidence;

internal sealed class EvidenceDistributionParityEvaluator
{
    private readonly Func<IStrategyCutOverService?> _getStrategyCutOverService;
    private readonly MetricSelectionService _metricSelectionService;

    internal EvidenceDistributionParityEvaluator(
        MetricSelectionService metricSelectionService,
        Func<IStrategyCutOverService?> getStrategyCutOverService)
    {
        _metricSelectionService = metricSelectionService ?? throw new ArgumentNullException(nameof(metricSelectionService));
        _getStrategyCutOverService = getStrategyCutOverService ?? throw new ArgumentNullException(nameof(getStrategyCutOverService));
    }

    internal async Task<DistributionParitySnapshot> BuildAsync(ChartState chartState, MetricState metricState, ChartDataContext? ctx)
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
            Weekly = EvidenceStrategyParityExecutor.ExecuteSafe(strategyService, StrategyType.WeeklyDistribution, parityContext, parameters),
            Hourly = EvidenceStrategyParityExecutor.ExecuteSafe(strategyService, StrategyType.HourlyDistribution, parityContext, parameters)
        };
    }

    private IStrategyCutOverService ResolveStrategyCutOverService()
    {
        return EvidenceDataResolutionHelper.ResolveStrategyCutOverService(_getStrategyCutOverService);
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

    private static MetricSeriesSelection? ResolveDistributionSelection(ChartState chartState, ChartDataContext ctx)
    {
        if (chartState.SelectedDistributionSeries != null)
            return chartState.SelectedDistributionSeries;

        var metricType = ctx.PrimaryMetricType ?? ctx.MetricType;
        return string.IsNullOrWhiteSpace(metricType) ? null : new MetricSeriesSelection(metricType, ctx.PrimarySubtype);
    }

    private async Task<(IReadOnlyList<MetricData>? Data, ICanonicalMetricSeries? Cms, string DataSource)> ResolveDistributionParityDataAsync(ChartDataContext ctx, MetricSeriesSelection selection, string tableName)
    {
        return await EvidenceDataResolutionHelper.ResolveWithFallbackAsync(ctx, selection, _metricSelectionService, tableName);
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
