using System.Diagnostics;
using DataFileReader.Canonical;
using DataVisualiser.Core.Configuration;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Strategies;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Reachability;
using DataVisualiser.Core.Transforms;
using DataVisualiser.Core.Transforms.Expressions;
using DataVisualiser.Core.Transforms.Operations;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.State;

namespace DataVisualiser.UI.MainHost;

public sealed class MainChartsEvidenceExportService
{
    private readonly ReachabilityExportWriter _exportWriter;
    private readonly Func<string> _getBarPieMode;
    private readonly Func<string?> _getSelectedTransformOperation;
    private readonly Func<IStrategyCutOverService?> _getStrategyCutOverService;
    private readonly Func<UiSurfaceDiagnosticsSnapshot>? _getUiSurfaceDiagnostics;
    private readonly MetricSelectionService _metricSelectionService;
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
        _metricSelectionService = metricSelectionService ?? throw new ArgumentNullException(nameof(metricSelectionService));
        _getStrategyCutOverService = getStrategyCutOverService ?? throw new ArgumentNullException(nameof(getStrategyCutOverService));
        _getSelectedTransformOperation = getSelectedTransformOperation ?? throw new ArgumentNullException(nameof(getSelectedTransformOperation));
        _getBarPieMode = getBarPieMode ?? throw new ArgumentNullException(nameof(getBarPieMode));
        _getUiSurfaceDiagnostics = getUiSurfaceDiagnostics;
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
        var distributionParity = await BuildDistributionParitySnapshotAsync(chartState, metricState, chartState.LastContext);
        var combinedParity = BuildCombinedMetricParitySnapshot(chartState.LastContext);
        var singleParity = BuildSingleMetricParitySnapshot(chartState.LastContext);
        var multiParity = await BuildMultiMetricParitySnapshotAsync(metricState, chartState.LastContext);
        var normalizedParity = BuildNormalizedParitySnapshot(chartState, chartState.LastContext);
        var weekdayTrendParity = BuildWeekdayTrendParitySnapshot(chartState.LastContext);
        var transformParity = await BuildTransformParitySnapshotAsync(chartState, metricState, chartState.LastContext);
        var paritySummary = BuildParitySummary(distributionParity, combinedParity, singleParity, multiParity, normalizedParity, weekdayTrendParity, transformParity);
        var parityWarnings = BuildParityWarnings(distributionParity, combinedParity, singleParity, multiParity, normalizedParity, weekdayTrendParity, transformParity, selectedSeries.Count);
        var selectedDistributionSettings = chartState.GetDistributionSettings(chartState.SelectedDistributionMode);
        var diagnostics = await BuildDiagnosticsAsync(chartState, metricState, reachabilityRecords);

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
                CmsConfiguration.UseCmsData,
                CmsConfiguration.UseCmsForSingleMetric,
                CmsConfiguration.UseCmsForMultiMetric,
                CmsConfiguration.UseCmsForCombinedMetric,
                CmsConfiguration.UseCmsForDifference,
                CmsConfiguration.UseCmsForRatio,
                CmsConfiguration.UseCmsForNormalized,
                CmsConfiguration.UseCmsForWeeklyDistribution,
                CmsConfiguration.UseCmsForWeekdayTrend,
                CmsConfiguration.UseCmsForHourlyDistribution,
                CmsConfiguration.UseCmsForBarPie
            },
            ReachabilityRecords = reachabilityRecords,
            DistributionParity = distributionParity,
            CombinedMetricParity = combinedParity,
            SingleMetricParity = singleParity,
            MultiMetricParity = multiParity,
            NormalizedParity = normalizedParity,
            WeekdayTrendParity = weekdayTrendParity,
            TransformParity = transformParity,
            ParitySummary = paritySummary,
            ParityWarnings = parityWarnings,
            Diagnostics = diagnostics
        };

        var result = _exportWriter.Write(payload, _targetPathResolver.ResolveDocumentsDirectory(), utcNow);
        var notes = new List<string>();
        if (reachabilityRecords.Count == 0)
            notes.Add("No reachability records captured yet. Export includes parity data only.");

        _reachabilityStore.Clear();

        return new ReachabilityEvidenceExportResult(result.FilePath, reachabilityRecords.Count > 0, parityWarnings, notes);
    }

    public sealed class DistributionParitySnapshot
    {
        public string Status { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public object? Selection { get; set; }
        public string? DataSource { get; set; }
        public int LegacySamples { get; set; }
        public int CmsSamplesTotal { get; set; }
        public int CmsSamplesInRange { get; set; }
        public ParityResultSnapshot? Weekly { get; set; }
        public ParityResultSnapshot? Hourly { get; set; }
    }

    public sealed class ParityResultSnapshot
    {
        public bool Passed { get; set; }
        public string? Message { get; set; }
        public object? Details { get; set; }
        public string? Error { get; set; }
    }

    public sealed class ParitySummarySnapshot
    {
        public string Status { get; set; } = string.Empty;
        public bool? OverallPassed { get; set; }
        public bool? WeeklyPassed { get; set; }
        public bool? HourlyPassed { get; set; }
        public bool? CombinedMetricPassed { get; set; }
        public bool? SingleMetricPassed { get; set; }
        public bool? MultiMetricPassed { get; set; }
        public bool? NormalizedPassed { get; set; }
        public bool? WeekdayTrendPassed { get; set; }
        public bool? TransformPassed { get; set; }
        public string[] StrategiesEvaluated { get; set; } = Array.Empty<string>();
    }

    public sealed class CombinedMetricParitySnapshot
    {
        public string Status { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public ParityResultSnapshot? Result { get; set; }
    }

    public sealed class SimpleParitySnapshot
    {
        public string Status { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public ParityResultSnapshot? Result { get; set; }
    }

    public sealed class TransformParitySnapshot
    {
        public string Status { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public string? Operation { get; set; }
        public bool IsUnary { get; set; }
        public bool ExpressionAvailable { get; set; }
        public int LegacySamples { get; set; }
        public int NewSamples { get; set; }
        public ParityResultSnapshot? Result { get; set; }
    }

    public sealed class DiagnosticsSnapshot
    {
        public SelectionDiagnosticsSnapshot Selection { get; set; } = new();
        public LoadedContextDiagnosticsSnapshot LoadedContext { get; set; } = new();
        public MainChartPipelineDiagnosticsSnapshot MainChartPipeline { get; set; } = new();
        public ReachabilityDiagnosticsSnapshot Reachability { get; set; } = new();
        public UiSurfaceDiagnosticsSnapshot UiSurface { get; set; } = new();
        public SmokeHeuristicsSnapshot SmokeChecks { get; set; } = new();
        public TransitionDiagnosticsSnapshot Transition { get; set; } = new();
    }

    public sealed class UiSurfaceDiagnosticsSnapshot
    {
        public MetricTypeUiDiagnosticsSnapshot MetricType { get; set; } = new();
        public DateRangeUiDiagnosticsSnapshot DateRange { get; set; } = new();
        public SubtypeUiDiagnosticsSnapshot Subtypes { get; set; } = new();
        public TransformUiDiagnosticsSnapshot Transform { get; set; } = new();
        public IReadOnlyList<HostMessageDiagnosticsSnapshot> RecentMessages { get; set; } = Array.Empty<HostMessageDiagnosticsSnapshot>();
    }

    public sealed class MetricTypeUiDiagnosticsSnapshot
    {
        public string? SelectedValue { get; set; }
        public string? SelectedDisplay { get; set; }
        public int OptionCount { get; set; }
    }

    public sealed class DateRangeUiDiagnosticsSnapshot
    {
        public DateTime? SelectedFromDate { get; set; }
        public DateTime? SelectedToDate { get; set; }
        public DateTime ExpectedDefaultFromDateUtc { get; set; }
        public DateTime ExpectedDefaultToDateUtc { get; set; }
        public bool MatchesExpectedDefaultWindow { get; set; }
    }

    public sealed class SubtypeUiDiagnosticsSnapshot
    {
        public int ActiveComboCount { get; set; }
        public bool PrimarySelectionMaterialized { get; set; }
        public bool AllCombosBoundToSelectedMetricType { get; set; }
        public bool? PrimaryOptionsMatchSelectedMetric { get; set; }
        public IReadOnlyList<string> ExpectedPrimaryOptionValues { get; set; } = Array.Empty<string>();
        public IReadOnlyList<SubtypeComboDiagnosticsSnapshot> OrderedCombos { get; set; } = Array.Empty<SubtypeComboDiagnosticsSnapshot>();
    }

    public sealed class SubtypeComboDiagnosticsSnapshot
    {
        public int Index { get; set; }
        public string? BoundMetricType { get; set; }
        public string? SelectedValue { get; set; }
        public string? SelectedDisplay { get; set; }
        public int OptionCount { get; set; }
        public IReadOnlyList<string> OptionValues { get; set; } = Array.Empty<string>();
    }

    public sealed class TransformUiDiagnosticsSnapshot
    {
        public bool PanelVisible { get; set; }
        public bool SecondaryPanelVisible { get; set; }
        public bool ComputeEnabled { get; set; }
        public string? SelectedOperation { get; set; }
        public string? SelectedPrimarySubtype { get; set; }
        public string? SelectedSecondarySubtype { get; set; }
        public int PrimaryOptionCount { get; set; }
        public int SecondaryOptionCount { get; set; }
    }

    public sealed class HostMessageDiagnosticsSnapshot
    {
        public DateTime TimestampUtc { get; set; }
        public string Severity { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Message { get; set; }
    }

    public sealed class SmokeHeuristicsSnapshot
    {
        public bool? SelectedMetricMatchesUiMetric { get; set; }
        public bool? SubtypeComboCountMatchesSelectedSeries { get; set; }
        public bool? LoadedSeriesCountMatchesSelection { get; set; }
        public bool? PrimarySubtypeOptionsMatchSelectedMetric { get; set; }
        public bool DateRangeMatchesExpectedDefaultWindow { get; set; }
        public int RecentErrorCount { get; set; }
        public bool RecentErrorsPresent { get; set; }
    }

    public sealed class TransitionDiagnosticsSnapshot
    {
        public string CurrentSelectionSignature { get; set; } = string.Empty;
        public bool? LoadedRequestMatchesCurrentSelection { get; set; }
        public string? LoadedContextSignature { get; set; }
        public string? LatestReachabilitySignature { get; set; }
        public int ExpectedSeriesCount { get; set; }
        public int LoadedContextSeriesCount { get; set; }
        public int LatestReachabilitySeriesCount { get; set; }
        public bool RenderEvidenceExceedsStoredContext { get; set; }
        public bool ReloadLikelyRequired { get; set; }
        public string State { get; set; } = string.Empty;
        public string Interpretation { get; set; } = string.Empty;
    }

    public sealed class SelectionDiagnosticsSnapshot
    {
        public string? SelectedMetricType { get; set; }
        public int SelectedSeriesCount { get; set; }
        public int DistinctDisplayKeyCount { get; set; }
        public IReadOnlyList<SeriesSelectionDiagnosticsSnapshot> OrderedSeries { get; set; } = Array.Empty<SeriesSelectionDiagnosticsSnapshot>();
    }

    public sealed class SeriesSelectionDiagnosticsSnapshot
    {
        public int Index { get; set; }
        public string? MetricType { get; set; }
        public string? Subtype { get; set; }
        public string? DisplayName { get; set; }
        public string? DisplayKey { get; set; }
        public bool MatchesLoadedPrimary { get; set; }
        public bool MatchesLoadedSecondary { get; set; }
        public bool RequiresOnDemandResolution { get; set; }
    }

    public sealed class LoadedContextDiagnosticsSnapshot
    {
        public bool Present { get; set; }
        public bool ReusableForCurrentSelection { get; set; }
        public string? LoadRequestSignature { get; set; }
        public string? MetricType { get; set; }
        public string? PrimaryMetricType { get; set; }
        public string? SecondaryMetricType { get; set; }
        public string? PrimarySubtype { get; set; }
        public string? SecondarySubtype { get; set; }
        public string? DisplayName1 { get; set; }
        public string? DisplayName2 { get; set; }
        public int ActualSeriesCount { get; set; }
        public int Data1Count { get; set; }
        public int Data2Count { get; set; }
        public int CmsSeriesCount { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }

    public sealed class MainChartPipelineDiagnosticsSnapshot
    {
        public int ExpectedSeriesFromSelection { get; set; }
        public int ExpectedBaseSeriesLoad { get; set; }
        public int ExpectedAdditionalSeriesLoad { get; set; }
        public int ContextActualSeriesCount { get; set; }
        public bool MultiMetricRequested { get; set; }
        public bool ContextLikelyStaleForSelection { get; set; }
    }

    public sealed class ReachabilityDiagnosticsSnapshot
    {
        public int RecordCount { get; set; }
        public int MaxActualSeriesCount { get; set; }
        public int MaxCmsSeriesCount { get; set; }
        public string? LatestStrategy { get; set; }
        public int LatestActualSeriesCount { get; set; }
        public int LatestCmsSeriesCount { get; set; }
        public string? LatestDecisionReason { get; set; }
        public DateTime? LatestTimestampUtc { get; set; }
    }

    private async Task<DiagnosticsSnapshot> BuildDiagnosticsAsync(
        ChartState chartState,
        MetricState metricState,
        IReadOnlyList<StrategyReachabilityRecord> reachabilityRecords)
    {
        var selectedSeries = metricState.SelectedSeries.ToList();
        var ctx = chartState.LastContext;
        var uiSurface = _getUiSurfaceDiagnostics?.Invoke() ?? new UiSurfaceDiagnosticsSnapshot();
        uiSurface.Subtypes.PrimaryOptionsMatchSelectedMetric = await DeterminePrimaryOptionsMatchSelectedMetricAsync(metricState, uiSurface.Subtypes);
        var reusableContext = ChartContextSelectionGuard.IsCompatibleWithCurrentSelection(
            ctx,
            metricState.SelectedMetricType,
            selectedSeries,
            metricState.FromDate,
            metricState.ToDate,
            metricState.ResolutionTableName);
        var latestRecord = reachabilityRecords
            .OrderByDescending(record => record.TimestampUtc)
            .FirstOrDefault();
        var expectedSeriesCount = selectedSeries.Count(series => series.QuerySubtype != null);
        var recentErrorCount = uiSurface.RecentMessages.Count(message =>
            string.Equals(message.Severity, "Error", StringComparison.OrdinalIgnoreCase));
        var transition = BuildTransitionDiagnostics(
            metricState,
            selectedSeries,
            ctx,
            reusableContext,
            latestRecord,
            expectedSeriesCount,
            recentErrorCount);

        return new DiagnosticsSnapshot
        {
            Selection = new SelectionDiagnosticsSnapshot
            {
                SelectedMetricType = metricState.SelectedMetricType,
                SelectedSeriesCount = selectedSeries.Count,
                DistinctDisplayKeyCount = selectedSeries
                    .Select(series => series.DisplayKey)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count(),
                OrderedSeries = selectedSeries
                    .Select((series, index) => new SeriesSelectionDiagnosticsSnapshot
                    {
                        Index = index,
                        MetricType = series.MetricType,
                        Subtype = series.Subtype,
                        DisplayName = series.DisplayName,
                        DisplayKey = series.DisplayKey,
                        MatchesLoadedPrimary = IsSameSelection(series, ctx?.PrimaryMetricType ?? ctx?.MetricType, ctx?.PrimarySubtype),
                        MatchesLoadedSecondary = IsSameSelection(series, ctx?.SecondaryMetricType, ctx?.SecondarySubtype),
                        RequiresOnDemandResolution =
                            !IsSameSelection(series, ctx?.PrimaryMetricType ?? ctx?.MetricType, ctx?.PrimarySubtype) &&
                            !IsSameSelection(series, ctx?.SecondaryMetricType, ctx?.SecondarySubtype)
                    })
                    .ToList()
            },
            LoadedContext = new LoadedContextDiagnosticsSnapshot
            {
                Present = ctx != null,
                ReusableForCurrentSelection = reusableContext,
                LoadRequestSignature = ctx?.LoadRequestSignature,
                MetricType = ctx?.MetricType,
                PrimaryMetricType = ctx?.PrimaryMetricType,
                SecondaryMetricType = ctx?.SecondaryMetricType,
                PrimarySubtype = ctx?.PrimarySubtype,
                SecondarySubtype = ctx?.SecondarySubtype,
                DisplayName1 = ctx?.DisplayName1,
                DisplayName2 = ctx?.DisplayName2,
                ActualSeriesCount = ctx?.ActualSeriesCount ?? 0,
                Data1Count = ctx?.Data1?.Count() ?? 0,
                Data2Count = ctx?.Data2?.Count() ?? 0,
                CmsSeriesCount = ctx?.CmsSeries?.Count ?? 0,
                From = ctx?.From,
                To = ctx?.To
            },
            MainChartPipeline = new MainChartPipelineDiagnosticsSnapshot
            {
                ExpectedSeriesFromSelection = selectedSeries.Count(series => series.QuerySubtype != null),
                ExpectedBaseSeriesLoad = Math.Min(2, selectedSeries.Count(series => series.QuerySubtype != null)),
                ExpectedAdditionalSeriesLoad = Math.Max(0, selectedSeries.Count(series => series.QuerySubtype != null) - 2),
                ContextActualSeriesCount = ctx?.ActualSeriesCount ?? 0,
                MultiMetricRequested = selectedSeries.Count(series => series.QuerySubtype != null) >= 3,
                ContextLikelyStaleForSelection = selectedSeries.Count > 0 && !reusableContext
            },
            Reachability = new ReachabilityDiagnosticsSnapshot
            {
                RecordCount = reachabilityRecords.Count,
                MaxActualSeriesCount = reachabilityRecords.Count == 0 ? 0 : reachabilityRecords.Max(record => record.ActualSeriesCount),
                MaxCmsSeriesCount = reachabilityRecords.Count == 0 ? 0 : reachabilityRecords.Max(record => record.CmsSeriesCount),
                LatestStrategy = latestRecord?.StrategyType.ToString(),
                LatestActualSeriesCount = latestRecord?.ActualSeriesCount ?? 0,
                LatestCmsSeriesCount = latestRecord?.CmsSeriesCount ?? 0,
                LatestDecisionReason = latestRecord?.DecisionReason,
                LatestTimestampUtc = latestRecord?.TimestampUtc
            },
            UiSurface = uiSurface,
            SmokeChecks = new SmokeHeuristicsSnapshot
            {
                SelectedMetricMatchesUiMetric =
                    string.IsNullOrWhiteSpace(uiSurface.MetricType.SelectedValue)
                        ? null
                        : string.Equals(metricState.SelectedMetricType, uiSurface.MetricType.SelectedValue, StringComparison.OrdinalIgnoreCase),
                SubtypeComboCountMatchesSelectedSeries =
                    uiSurface.Subtypes.ActiveComboCount > 0
                        ? uiSurface.Subtypes.ActiveComboCount == selectedSeries.Count
                        : null,
                LoadedSeriesCountMatchesSelection =
                    ctx == null || expectedSeriesCount == 0
                        ? null
                        : ctx.ActualSeriesCount == expectedSeriesCount,
                PrimarySubtypeOptionsMatchSelectedMetric = uiSurface.Subtypes.PrimaryOptionsMatchSelectedMetric,
                DateRangeMatchesExpectedDefaultWindow = uiSurface.DateRange.MatchesExpectedDefaultWindow,
                RecentErrorCount = recentErrorCount,
                RecentErrorsPresent = recentErrorCount > 0
            },
            Transition = transition
        };
    }

    private static TransitionDiagnosticsSnapshot BuildTransitionDiagnostics(
        MetricState metricState,
        IReadOnlyList<MetricSeriesSelection> selectedSeries,
        ChartDataContext? context,
        bool reusableContext,
        StrategyReachabilityRecord? latestRecord,
        int expectedSeriesCount,
        int recentErrorCount)
    {
        var loadedSeriesCount = context?.ActualSeriesCount ?? 0;
        var latestReachabilitySeriesCount = latestRecord?.ActualSeriesCount ?? 0;
        var renderEvidenceExceedsStoredContext = latestReachabilitySeriesCount > loadedSeriesCount;

        string state;
        string interpretation;
        var reloadLikelyRequired = false;

        if (recentErrorCount > 0)
        {
            state = "HostErrorObserved";
            interpretation = "A host error dialog was recorded during this scenario.";
        }
        else if (selectedSeries.Count == 0)
        {
            state = "NoSelection";
            interpretation = "No series are currently selected.";
        }
        else if (expectedSeriesCount == 0)
        {
            state = "SelectionIncomplete";
            interpretation = "The selection exists, but one or more subtype values are still missing.";
        }
        else if (context == null || loadedSeriesCount == 0)
        {
            if (latestReachabilitySeriesCount >= expectedSeriesCount && expectedSeriesCount > 0)
            {
                state = "RenderRecordedWithoutStoredContext";
                interpretation = "Reachability shows a render/load decision for the current selection, but the stored chart context is empty.";
                reloadLikelyRequired = true;
            }
            else
            {
                state = "AwaitingLoad";
                interpretation = "Selection state is configured, but no successful data load is reflected yet.";
            }
        }
        else if (reusableContext && loadedSeriesCount == expectedSeriesCount)
        {
            state = "ContextAligned";
            interpretation = "Stored chart context matches the current selection and expected series count.";
        }
        else if (reusableContext && loadedSeriesCount != expectedSeriesCount)
        {
            if (renderEvidenceExceedsStoredContext && latestReachabilitySeriesCount >= expectedSeriesCount)
            {
                state = "StoredContextLagging";
                interpretation = "Reachability shows the newer series count, but the stored chart context still reflects an older count.";
            }
            else
            {
                state = "SeriesCountMismatch";
                interpretation = "Stored chart context is reusable by metric family, but its series count does not match the current selection.";
            }

            reloadLikelyRequired = true;
        }
        else if (renderEvidenceExceedsStoredContext && latestReachabilitySeriesCount >= expectedSeriesCount)
        {
            state = "StaleContextAfterRender";
            interpretation = "A newer render/load appears to have happened, but the stored chart context is stale for the current selection.";
            reloadLikelyRequired = true;
        }
        else
        {
            state = "StaleContext";
            interpretation = "Stored chart context does not match the current metric/subtype selection.";
            reloadLikelyRequired = true;
        }

        return new TransitionDiagnosticsSnapshot
        {
            CurrentSelectionSignature = BuildSelectionSignature(metricState, selectedSeries),
            LoadedRequestMatchesCurrentSelection = string.IsNullOrWhiteSpace(context?.LoadRequestSignature)
                ? null
                : string.Equals(context.LoadRequestSignature, BuildSelectionSignature(metricState, selectedSeries), StringComparison.Ordinal),
            LoadedContextSignature = BuildContextSignature(context),
            LatestReachabilitySignature = BuildReachabilitySignature(latestRecord),
            ExpectedSeriesCount = expectedSeriesCount,
            LoadedContextSeriesCount = loadedSeriesCount,
            LatestReachabilitySeriesCount = latestReachabilitySeriesCount,
            RenderEvidenceExceedsStoredContext = renderEvidenceExceedsStoredContext,
            ReloadLikelyRequired = reloadLikelyRequired,
            State = state,
            Interpretation = interpretation
        };
    }

    private static string BuildSelectionSignature(MetricState metricState, IReadOnlyList<MetricSeriesSelection> selectedSeries)
    {
        var orderedSeries = string.Join(
            "|",
            selectedSeries.Select(series => $"{series.MetricType}:{series.QuerySubtype ?? "<none>"}"));

        return $"{metricState.SelectedMetricType ?? "<none>"}::{metricState.ResolutionTableName ?? "<none>"}::{metricState.FromDate:O}->{metricState.ToDate:O}::{orderedSeries}";
    }

    private static string? BuildContextSignature(ChartDataContext? context)
    {
        if (context == null)
            return null;

        var metricType = context.PrimaryMetricType ?? context.MetricType ?? "<none>";
        var secondaryMetricType = context.SecondaryMetricType ?? "<none>";
        var primarySubtype = context.PrimarySubtype ?? "<none>";
        var secondarySubtype = context.SecondarySubtype ?? "<none>";

        return $"{metricType}:{primarySubtype}|{secondaryMetricType}:{secondarySubtype}::{context.From:O}->{context.To:O}::series={context.ActualSeriesCount}";
    }

    private static string? BuildReachabilitySignature(StrategyReachabilityRecord? latestRecord)
    {
        if (latestRecord == null)
            return null;

        return $"{latestRecord.StrategyType}::actual={latestRecord.ActualSeriesCount}::cms={latestRecord.CmsSeriesCount}::reason={latestRecord.DecisionReason}";
    }

    private async Task<bool?> DeterminePrimaryOptionsMatchSelectedMetricAsync(
        MetricState metricState,
        SubtypeUiDiagnosticsSnapshot subtypes)
    {
        if (string.IsNullOrWhiteSpace(metricState.SelectedMetricType) ||
            string.IsNullOrWhiteSpace(metricState.ResolutionTableName) ||
            subtypes.OrderedCombos.Count == 0)
            return null;

        try
        {
            var expected = await _metricSelectionService.LoadSubtypesAsync(metricState.SelectedMetricType, metricState.ResolutionTableName);
            var expectedValues = expected
                .Select(option => option.Value)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToList();

            subtypes.ExpectedPrimaryOptionValues = expectedValues;

            var actualValues = subtypes.OrderedCombos[0].OptionValues
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return expectedValues.SequenceEqual(actualValues, StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return null;
        }
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

    private static bool IsSameSelection(MetricSeriesSelection selection, string? metricType, string? subtype)
    {
        if (!string.Equals(selection.MetricType, metricType ?? string.Empty, StringComparison.OrdinalIgnoreCase))
            return false;

        var normalizedSubtype = string.IsNullOrWhiteSpace(subtype) || subtype == "(All)" ? null : subtype;
        var selectionSubtype = selection.QuerySubtype;
        return string.Equals(selectionSubtype ?? string.Empty, normalizedSubtype ?? string.Empty, StringComparison.OrdinalIgnoreCase);
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
        if (ctx.Data1 != null && IsSameSelection(selection, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return (ctx.Data1, ctx.PrimaryCms as ICanonicalMetricSeries);

        if (ctx.Data2 != null && IsSameSelection(selection, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return (ctx.Data2, ctx.SecondaryCms as ICanonicalMetricSeries);

        if (ctx.CmsSeries != null)
        {
            var targetMetricId = CanonicalMetricMapping.FromLegacyFields(selection.MetricType, selection.QuerySubtype);
            var matchingCms = ctx.CmsSeries.FirstOrDefault(series => string.Equals(series.MetricId?.Value, targetMetricId, StringComparison.OrdinalIgnoreCase));
            if (matchingCms != null)
            {
                var legacyData = await ResolveTransformParityDataAsync(null, ctx, selection);
                return (legacyData, matchingCms);
            }
        }

        var (primaryCms, _, primaryData, _) = await _metricSelectionService.LoadMetricDataWithCmsAsync(selection, null, ctx.From, ctx.To, tableName);
        return (primaryData.ToList(), primaryCms);
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

    private async Task<TransformParitySnapshot> BuildTransformParitySnapshotAsync(ChartState chartState, MetricState metricState, ChartDataContext? ctx)
    {
        if (ctx == null)
            return new TransformParitySnapshot { Status = "Unavailable", Reason = "No chart context available" };

        var operation = _getSelectedTransformOperation();
        if (string.IsNullOrWhiteSpace(operation))
            return new TransformParitySnapshot { Status = "Unavailable", Reason = "No transform operation selected" };

        var (primarySelection, secondarySelection) = ResolveTransformSelections(chartState, ctx);
        var primaryData = await ResolveTransformParityDataAsync(metricState, ctx, primarySelection);
        if (primaryData == null || primaryData.Count == 0)
            return new TransformParitySnapshot { Status = "Unavailable", Reason = "No primary data available for transform" };

        var isUnary = IsUnaryTransform(operation);
        IReadOnlyList<MetricData>? secondaryData = null;
        if (!isUnary)
        {
            secondaryData = await ResolveTransformParityDataAsync(metricState, ctx, secondarySelection);
            if (secondaryData == null || secondaryData.Count == 0)
                return new TransformParitySnapshot { Status = "Unavailable", Reason = "No secondary data available for binary transform" };
        }

        var result = isUnary ? ComputeUnaryTransformParity(primaryData, operation) : ComputeBinaryTransformParity(primaryData, secondaryData!, operation);
        return new TransformParitySnapshot
        {
            Status = "Completed",
            Operation = operation,
            IsUnary = isUnary,
            ExpressionAvailable = result.ExpressionAvailable,
            LegacySamples = result.LegacySamples,
            NewSamples = result.NewSamples,
            Result = result.Result
        };
    }

    private static bool IsUnaryTransform(string operation)
    {
        return string.Equals(operation, "Log", StringComparison.OrdinalIgnoreCase) || string.Equals(operation, "Sqrt", StringComparison.OrdinalIgnoreCase);
    }

    private static (MetricSeriesSelection? Primary, MetricSeriesSelection? Secondary) ResolveTransformSelections(ChartState chartState, ChartDataContext ctx)
    {
        var primary = chartState.SelectedTransformPrimarySeries;
        var secondary = chartState.SelectedTransformSecondarySeries;
        if (primary != null || secondary != null)
            return (primary, secondary);

        var primaryMetricType = ctx.PrimaryMetricType ?? ctx.MetricType;
        var primarySelection = string.IsNullOrWhiteSpace(primaryMetricType) ? null : new MetricSeriesSelection(primaryMetricType, ctx.PrimarySubtype);
        MetricSeriesSelection? secondarySelection = null;
        if (!string.IsNullOrWhiteSpace(ctx.SecondaryMetricType))
            secondarySelection = new MetricSeriesSelection(ctx.SecondaryMetricType, ctx.SecondarySubtype);

        return (primarySelection, secondarySelection);
    }

    private async Task<IReadOnlyList<MetricData>?> ResolveTransformParityDataAsync(MetricState? metricState, ChartDataContext ctx, MetricSeriesSelection? selection)
    {
        if (selection == null)
            return null;

        if (ctx.Data1 != null && IsSameSelection(selection, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return ctx.Data1;

        if (ctx.Data2 != null && IsSameSelection(selection, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return ctx.Data2;

        if (string.IsNullOrWhiteSpace(selection.MetricType))
            return ctx.Data1;

        var tableName = metricState?.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
        var (primaryData, _) = await _metricSelectionService.LoadMetricDataAsync(selection.MetricType, selection.QuerySubtype, null, ctx.From, ctx.To, tableName);
        return primaryData.ToList();
    }

    private static (ParityResultSnapshot Result, int LegacySamples, int NewSamples, bool ExpressionAvailable) ComputeUnaryTransformParity(IReadOnlyList<MetricData> data, string operation)
    {
        var prepared = data.Where(d => d.Value.HasValue).OrderBy(d => d.NormalizedTimestamp).ToList();
        if (prepared.Count == 0)
            return (new ParityResultSnapshot { Passed = false, Error = "No valid data points" }, 0, 0, false);

        var values = prepared.Select(d => (double)d.Value!.Value).ToList();
        var legacyOp = operation switch
        {
            "Log" => UnaryOperators.Logarithm,
            "Sqrt" => UnaryOperators.SquareRoot,
            _ => x => x
        };
        var legacy = MathHelper.ApplyUnaryOperation(values, legacyOp);
        var expression = TransformExpressionBuilder.BuildFromOperation(operation, 0);
        var modern = expression != null ? TransformExpressionEvaluator.Evaluate(expression, [prepared]) : legacy;
        return (CompareTransformResults(legacy, modern), legacy.Count, modern.Count, expression != null);
    }

    private static (ParityResultSnapshot Result, int LegacySamples, int NewSamples, bool ExpressionAvailable) ComputeBinaryTransformParity(IReadOnlyList<MetricData> data1, IReadOnlyList<MetricData> data2, string operation)
    {
        var prepared1 = data1.Where(d => d.Value.HasValue).OrderBy(d => d.NormalizedTimestamp).ToList();
        var prepared2 = data2.Where(d => d.Value.HasValue).OrderBy(d => d.NormalizedTimestamp).ToList();
        var (aligned1, aligned2) = TransformExpressionEvaluator.AlignMetricsByTimestamp(prepared1, prepared2);
        if (aligned1.Count == 0 || aligned2.Count == 0)
            return (new ParityResultSnapshot { Passed = false, Error = "No aligned data points" }, 0, 0, false);

        var values1 = aligned1.Select(d => (double)d.Value!.Value).ToList();
        var values2 = aligned2.Select(d => (double)d.Value!.Value).ToList();
        var legacyOp = operation switch
        {
            "Add" => BinaryOperators.Sum,
            "Subtract" => BinaryOperators.Difference,
            "Divide" => BinaryOperators.Ratio,
            _ => (a, b) => a
        };
        var legacy = MathHelper.ApplyBinaryOperation(values1, values2, legacyOp);
        var expression = TransformExpressionBuilder.BuildFromOperation(operation, 0, 1);
        var modern = expression != null ? TransformExpressionEvaluator.Evaluate(expression, [aligned1, aligned2]) : legacy;
        return (CompareTransformResults(legacy, modern), legacy.Count, modern.Count, expression != null);
    }

    private static ParityResultSnapshot CompareTransformResults(IReadOnlyList<double> legacy, IReadOnlyList<double> modern)
    {
        if (legacy.Count != modern.Count)
            return new ParityResultSnapshot { Passed = false, Error = $"Result count mismatch: legacy={legacy.Count}, new={modern.Count}" };

        const double epsilon = 1e-6;
        for (var i = 0; i < legacy.Count; i++)
        {
            if (double.IsNaN(legacy[i]) && double.IsNaN(modern[i]))
                continue;

            if (Math.Abs(legacy[i] - modern[i]) > epsilon)
                return new ParityResultSnapshot { Passed = false, Error = $"Value mismatch at index {i}: legacy={legacy[i]}, new={modern[i]}" };
        }

        return new ParityResultSnapshot { Passed = true, Message = "Transform parity validation passed" };
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
        if (ctx.Data1 != null && IsSameSelection(selection, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return (ctx.Data1, ctx.PrimaryCms as ICanonicalMetricSeries, "ChartContext.Primary");

        if (ctx.Data2 != null && IsSameSelection(selection, ctx.SecondaryMetricType, ctx.SecondarySubtype))
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
