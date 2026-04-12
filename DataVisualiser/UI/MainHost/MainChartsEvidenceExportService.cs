using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Reachability;
using DataVisualiser.UI.State;

namespace DataVisualiser.UI.MainHost;

public sealed class MainChartsEvidenceExportService
{
    private readonly EvidenceDiagnosticsBuilder _diagnosticsBuilder;
    private readonly EvidenceParityBuilder _parityBuilder;
    private readonly ReachabilityExportWriter _exportWriter;
    private readonly Func<string> _getBarPieMode;
    private readonly IReachabilityEvidenceStore _reachabilityStore;
    private readonly IReachabilityExportPathResolver _targetPathResolver;

    public MainChartsEvidenceExportService(
        ReachabilityExportWriter exportWriter,
        IReachabilityExportPathResolver targetPathResolver,
        IReachabilityEvidenceStore reachabilityStore,
        MetricSelectionService metricSelectionService,
        Func<IStrategyCutOverService?> getStrategyCutOverService,
        Func<string?> getSelectedTransformOperation,
        Func<string> getBarPieMode,
        Func<UiSurfaceDiagnosticsSnapshot>? getUiSurfaceDiagnostics = null)
    {
        _exportWriter = exportWriter ?? throw new ArgumentNullException(nameof(exportWriter));
        _targetPathResolver = targetPathResolver ?? throw new ArgumentNullException(nameof(targetPathResolver));
        _reachabilityStore = reachabilityStore ?? throw new ArgumentNullException(nameof(reachabilityStore));
        _getBarPieMode = getBarPieMode ?? throw new ArgumentNullException(nameof(getBarPieMode));
        _diagnosticsBuilder = new EvidenceDiagnosticsBuilder(metricSelectionService, getUiSurfaceDiagnostics);
        _parityBuilder = new EvidenceParityBuilder(metricSelectionService, getStrategyCutOverService, getSelectedTransformOperation);
    }

    public void ClearEvidence()
    {
        _reachabilityStore.Clear();
    }

    public async Task<ReachabilityEvidenceExportResult> ExportAsync(ChartState chartState, MetricState metricState, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(chartState);
        ArgumentNullException.ThrowIfNull(metricState);

        var reachabilityRecords = _reachabilityStore.Snapshot();
        var selectedSeries = metricState.SelectedSeries.ToList();
        var parity = await _parityBuilder.BuildAsync(chartState, metricState, chartState.LastContext);
        var selectedDistributionSettings = chartState.GetDistributionSettings(chartState.SelectedDistributionMode);
        var diagnostics = await _diagnosticsBuilder.BuildDiagnosticsAsync(chartState, metricState, reachabilityRecords);

        var payload = new
        {
            ExportedAtUtc = utcNow,
            MetricState = new
            {
                metricState.SelectedMetricType,
                metricState.FromDate,
                metricState.ToDate,
                metricState.ResolutionTableName,
                SelectedSeries = selectedSeries.Select(series => new
                {
                    series.MetricType,
                    series.Subtype,
                    series.DisplayMetricType,
                    series.DisplaySubtype,
                    series.DisplayName,
                    series.DisplayKey
                })
            },
            ChartState = new
            {
                chartState.IsMainVisible,
                chartState.IsNormalizedVisible,
                chartState.IsDiffRatioVisible,
                chartState.IsDistributionVisible,
                chartState.IsWeeklyTrendVisible,
                chartState.IsTransformPanelVisible,
                chartState.IsBarPieVisible,
                chartState.MainChartDisplayMode,
                chartState.IsDiffRatioDifferenceMode,
                chartState.SelectedDistributionMode,
                SelectedDistributionSettings = new
                {
                    selectedDistributionSettings.UseFrequencyShading,
                    selectedDistributionSettings.IntervalCount
                },
                chartState.IsDistributionPolarMode,
                chartState.WeekdayTrendChartMode,
                chartState.WeekdayTrendAverageWindow,
                chartState.ShowMonday,
                chartState.ShowTuesday,
                chartState.ShowWednesday,
                chartState.ShowThursday,
                chartState.ShowFriday,
                chartState.ShowSaturday,
                chartState.ShowSunday,
                chartState.ShowAverage,
                chartState.BarPieBucketCount,
                BarPieMode = _getBarPieMode()
            },
            CmsConfiguration = new
            {
                Core.Configuration.CmsConfiguration.UseCmsData,
                Core.Configuration.CmsConfiguration.UseCmsForSingleMetric,
                Core.Configuration.CmsConfiguration.UseCmsForMultiMetric,
                Core.Configuration.CmsConfiguration.UseCmsForCombinedMetric,
                Core.Configuration.CmsConfiguration.UseCmsForDifference,
                Core.Configuration.CmsConfiguration.UseCmsForRatio,
                Core.Configuration.CmsConfiguration.UseCmsForNormalized,
                Core.Configuration.CmsConfiguration.UseCmsForWeeklyDistribution,
                Core.Configuration.CmsConfiguration.UseCmsForWeekdayTrend,
                Core.Configuration.CmsConfiguration.UseCmsForHourlyDistribution,
                Core.Configuration.CmsConfiguration.UseCmsForBarPie
            },
            ReachabilityRecords = reachabilityRecords,
            DistributionParity = parity.DistributionParity,
            CombinedMetricParity = parity.CombinedMetricParity,
            SingleMetricParity = parity.SingleMetricParity,
            MultiMetricParity = parity.MultiMetricParity,
            NormalizedParity = parity.NormalizedParity,
            WeekdayTrendParity = parity.WeekdayTrendParity,
            TransformParity = parity.TransformParity,
            ParitySummary = parity.ParitySummary,
            ParityWarnings = parity.ParityWarnings,
            SessionMilestones = chartState.SessionMilestones,
            Diagnostics = diagnostics
        };

        var result = _exportWriter.Write(payload, _targetPathResolver.ResolveDocumentsDirectory(), utcNow);
        var notes = new List<string>();
        if (reachabilityRecords.Count == 0)
            notes.Add("No reachability records captured yet. Export includes parity data only.");

        _reachabilityStore.Clear();

        return new ReachabilityEvidenceExportResult(result.FilePath, reachabilityRecords.Count > 0, parity.ParityWarnings, notes);
    }
}
