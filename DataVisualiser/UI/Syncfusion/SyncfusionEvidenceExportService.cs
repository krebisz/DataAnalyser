using DataVisualiser.Core.Configuration;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.State;
using DataVisualiser.UI.SyncfusionViews;

namespace DataVisualiser.UI.SyncfusionViews;

public sealed class SyncfusionEvidenceExportService
{
    private readonly ReachabilityExportWriter _exportWriter;
    private readonly IReachabilityEvidenceStore _reachabilityStore;
    private readonly IReachabilityExportPathResolver _targetPathResolver;

    public SyncfusionEvidenceExportService(
        ReachabilityExportWriter exportWriter,
        IReachabilityExportPathResolver targetPathResolver,
        IReachabilityEvidenceStore reachabilityStore)
    {
        _exportWriter = exportWriter ?? throw new ArgumentNullException(nameof(exportWriter));
        _targetPathResolver = targetPathResolver ?? throw new ArgumentNullException(nameof(targetPathResolver));
        _reachabilityStore = reachabilityStore ?? throw new ArgumentNullException(nameof(reachabilityStore));
    }

    public void ClearEvidence()
    {
        _reachabilityStore.Clear();
    }

    public ReachabilityEvidenceExportResult Export(ChartState chartState, MetricState metricState, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(chartState);
        ArgumentNullException.ThrowIfNull(metricState);

        var records = _reachabilityStore.Snapshot();
        var notes = new List<string>
        {
            SyncfusionChartsViewCoordinator.ReachabilityExportNotWiredMessage
        };
        if (records.Count > 0)
            notes.Add($"Shared session reachability records omitted from Syncfusion export: {records.Count}.");

        var payload = new
        {
            ExportedAtUtc = utcNow,
            ExportScope = "Syncfusion",
            MetricState = new
            {
                metricState.SelectedMetricType,
                metricState.FromDate,
                metricState.ToDate,
                metricState.ResolutionTableName,
                SelectedSeries = metricState.SelectedSeries.Select(series => new
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
                chartState.IsSyncfusionSunburstVisible,
                chartState.BarPieBucketCount
            },
            CmsConfiguration = new
            {
                CmsConfiguration.UseCmsData,
                CmsConfiguration.UseCmsForSingleMetric,
                CmsConfiguration.UseCmsForCombinedMetric,
                CmsConfiguration.UseCmsForMultiMetric,
                CmsConfiguration.UseCmsForNormalized,
                CmsConfiguration.UseCmsForWeeklyDistribution,
                CmsConfiguration.UseCmsForWeekdayTrend,
                CmsConfiguration.UseCmsForHourlyDistribution,
                CmsConfiguration.UseCmsForBarPie
            },
            ReachabilityRecords = Array.Empty<object>(),
            SharedSessionReachabilityRecordsOmitted = records.Count,
            ReachabilityStatus = "NotApplicable",
            Notes = notes
        };

        var result = _exportWriter.Write(payload, _targetPathResolver.ResolveDocumentsDirectory(), utcNow);
        _reachabilityStore.Clear();
        return new ReachabilityEvidenceExportResult(result.FilePath, false, Array.Empty<string>(), notes);
    }
}
