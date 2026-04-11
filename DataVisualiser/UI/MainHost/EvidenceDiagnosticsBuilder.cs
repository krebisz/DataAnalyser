using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Strategies.Reachability;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.State;

namespace DataVisualiser.UI.MainHost;

public sealed class EvidenceDiagnosticsBuilder
{
    private readonly Func<UiSurfaceDiagnosticsSnapshot>? _getUiSurfaceDiagnostics;
    private readonly MetricSelectionService _metricSelectionService;

    public EvidenceDiagnosticsBuilder(
        MetricSelectionService metricSelectionService,
        Func<UiSurfaceDiagnosticsSnapshot>? getUiSurfaceDiagnostics)
    {
        _metricSelectionService = metricSelectionService ?? throw new ArgumentNullException(nameof(metricSelectionService));
        _getUiSurfaceDiagnostics = getUiSurfaceDiagnostics;
    }

    public async Task<DiagnosticsSnapshot> BuildDiagnosticsAsync(
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
            recentErrorCount,
            chartState.LastLoadRuntime,
            HasVisibleExtendedCharts(chartState));

        return new DiagnosticsSnapshot
        {
            RuntimePath = chartState.LastLoadRuntime?.RuntimePath ?? EvidenceRuntimePath.Legacy,
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
            Transition = transition,
            VNext = BuildVNextDiagnostics(chartState.LastLoadRuntime)
        };
    }

    internal static TransitionDiagnosticsSnapshot BuildTransitionDiagnostics(
        MetricState metricState,
        IReadOnlyList<MetricSeriesSelection> selectedSeries,
        ChartDataContext? context,
        bool reusableContext,
        StrategyReachabilityRecord? latestRecord,
        int expectedSeriesCount,
        int recentErrorCount,
        LoadRuntimeState? runtime,
        bool visibleChartsRequireExtendedContext)
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
        else if (runtime?.SupportsOnlyMainChart == true && visibleChartsRequireExtendedContext)
        {
            state = "ReloadRequiredForExtendedCharts";
            interpretation = "The current load only supports the main chart. Reload is required before extended chart families can render.";
            reloadLikelyRequired = true;
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

    internal static string BuildSelectionSignature(MetricState metricState, IReadOnlyList<MetricSeriesSelection> selectedSeries)
    {
        var orderedSeries = string.Join(
            "|",
            selectedSeries.Select(series => $"{series.MetricType}:{series.QuerySubtype ?? "<none>"}"));

        return $"{metricState.SelectedMetricType ?? "<none>"}::{metricState.ResolutionTableName ?? "<none>"}::{metricState.FromDate:O}->{metricState.ToDate:O}::{orderedSeries}";
    }

    internal static string? BuildContextSignature(ChartDataContext? context)
    {
        if (context == null)
            return null;

        var metricType = context.PrimaryMetricType ?? context.MetricType ?? "<none>";
        var secondaryMetricType = context.SecondaryMetricType ?? "<none>";
        var primarySubtype = context.PrimarySubtype ?? "<none>";
        var secondarySubtype = context.SecondarySubtype ?? "<none>";

        return $"{metricType}:{primarySubtype}|{secondaryMetricType}:{secondarySubtype}::{context.From:O}->{context.To:O}::series={context.ActualSeriesCount}";
    }

    internal static string? BuildReachabilitySignature(StrategyReachabilityRecord? latestRecord)
    {
        if (latestRecord == null)
            return null;

        return $"{latestRecord.StrategyType}::actual={latestRecord.ActualSeriesCount}::cms={latestRecord.CmsSeriesCount}::reason={latestRecord.DecisionReason}";
    }

    internal static bool IsSameSelection(MetricSeriesSelection selection, string? metricType, string? subtype)
    {
        return MetricSeriesSelectionCache.IsSameSelection(selection, metricType, subtype);
    }

    internal static VNextDiagnosticsSnapshot? BuildVNextDiagnostics(LoadRuntimeState? runtime)
    {
        if (runtime == null || runtime.RuntimePath != EvidenceRuntimePath.VNextMain)
            return null;

        return new VNextDiagnosticsSnapshot
        {
            RequestSignature = runtime.RequestSignature,
            SnapshotSignature = runtime.SnapshotSignature,
            ProgramKind = runtime.ProgramKind?.ToString(),
            ProgramSourceSignature = runtime.ProgramSourceSignature,
            ProjectedContextSignature = runtime.ProjectedContextSignature,
            RequestMatchesSnapshot = !string.IsNullOrWhiteSpace(runtime.RequestSignature) &&
                                    string.Equals(runtime.RequestSignature, runtime.SnapshotSignature, StringComparison.Ordinal),
            SnapshotMatchesProgramSource = !string.IsNullOrWhiteSpace(runtime.SnapshotSignature) &&
                                          string.Equals(runtime.SnapshotSignature, runtime.ProgramSourceSignature, StringComparison.Ordinal),
            ProgramSourceMatchesProjectedContext = !string.IsNullOrWhiteSpace(runtime.ProgramSourceSignature) &&
                                                  string.Equals(runtime.ProgramSourceSignature, runtime.ProjectedContextSignature, StringComparison.Ordinal),
            SupportsOnlyMainChart = runtime.SupportsOnlyMainChart,
            FailureReason = runtime.FailureReason
        };
    }

    private static bool HasVisibleExtendedCharts(ChartState chartState)
    {
        return chartState.IsNormalizedVisible ||
               chartState.IsDiffRatioVisible ||
               chartState.IsDistributionVisible ||
               chartState.IsWeeklyTrendVisible ||
               chartState.IsTransformPanelVisible ||
               chartState.IsBarPieVisible;
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
}
