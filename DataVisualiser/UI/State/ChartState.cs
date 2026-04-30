using DataVisualiser.Core.Orchestration;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.MainHost.Evidence;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Rendering;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.State;

public class ChartState
{
    private readonly Dictionary<DistributionMode, DistributionModeSettings> _distributionSettings = new();
    private readonly Dictionary<ChartProgramKind, LoadRuntimeState> _familyLoadRuntimes = new();
    private readonly Dictionary<ChartProgramKind, RenderPlanDiagnosticsSnapshot> _renderPlanDiagnostics = new();
    private readonly List<RenderPlanHistorySnapshot> _renderPlanHistory = new();
    private readonly List<InterpretationResultDiagnosticsSnapshot> _interpretationDiagnostics = new();
    private readonly List<PerformanceTimingSnapshot> _performanceTimings = new();
    private readonly List<SessionMilestoneSnapshot> _sessionMilestones = new();
    private const int MaxRenderPlanHistory = 100;
    private const int MaxInterpretationDiagnostics = 100;
    private const int MaxSessionMilestones = 50;
    private const int MaxPerformanceTimings = 100;

    public ChartState()
    {
        foreach (var definition in DistributionModeCatalog.All)
            _distributionSettings[definition.Mode] = new DistributionModeSettings(true, definition.DefaultIntervalCount);
    }

    // Track which charts are visible
    public bool IsMainVisible { get; set; } = true; // Default to visible (Show on startup)
    public MainChartDisplayMode MainChartDisplayMode { get; set; } = MainChartDisplayMode.Regular;
    public bool IsNormalizedVisible { get; set; }
    public bool IsDiffRatioVisible { get; set; }                // Unified Diff/Ratio chart
    public bool IsDiffRatioDifferenceMode { get; set; } = true; // true = Difference (-), false = Ratio (/)
    public bool IsDistributionVisible { get; set; }
    public bool IsWeeklyTrendVisible { get; set; }
    public bool IsTransformPanelVisible { get; set; }
    public bool IsBarPieVisible { get; set; }
    public bool IsSyncfusionSunburstVisible { get; set; } = true;
    public WeekdayTrendChartMode WeekdayTrendChartMode { get; set; } = WeekdayTrendChartMode.Cartesian;
    public bool IsDistributionPolarMode { get; set; } = false; // Default to Cartesian
    public int BarPieBucketCount { get; set; } = 3;

    // Weekly Trend (weekday series toggles)
    public bool ShowMonday { get; set; } = true;
    public bool ShowTuesday { get; set; } = true;
    public bool ShowWednesday { get; set; } = true;
    public bool ShowThursday { get; set; } = true;
    public bool ShowFriday { get; set; } = true;
    public bool ShowSaturday { get; set; } = true;
    public bool ShowSunday { get; set; } = true;
    public bool ShowAverage { get; set; } = true;
    public WeekdayTrendAverageWindow WeekdayTrendAverageWindow { get; set; } = WeekdayTrendAverageWindow.RunningMean;


    // Normalization mode
    public NormalizationMode SelectedNormalizationMode { get; set; } = NormalizationMode.PercentageOfMax;

    // Distribution chart options
    public DistributionMode SelectedDistributionMode { get; set; } = DistributionMode.Weekly;
    public MetricSeriesSelection? SelectedDistributionSeries { get; set; }
    public MetricSeriesSelection? SelectedWeekdayTrendSeries { get; set; }
    public MetricSeriesSelection? SelectedStackedOverlaySeries { get; set; }
    public MetricSeriesSelection? SelectedNormalizedPrimarySeries { get; set; }
    public MetricSeriesSelection? SelectedNormalizedSecondarySeries { get; set; }
    public MetricSeriesSelection? SelectedDiffRatioPrimarySeries { get; set; }
    public MetricSeriesSelection? SelectedDiffRatioSecondarySeries { get; set; }
    public MetricSeriesSelection? SelectedTransformPrimarySeries { get; set; }
    public MetricSeriesSelection? SelectedTransformSecondarySeries { get; set; }

    // Chart data from last load
    public ChartDataContext? LastContext { get; set; }
    public LoadRuntimeState? LastLoadRuntime { get; set; }

    public LoadRuntimeState? GetFamilyRuntime(ChartProgramKind kind) =>
        _familyLoadRuntimes.TryGetValue(kind, out var runtime) ? runtime : null;

    public void SetFamilyRuntime(ChartProgramKind kind, LoadRuntimeState runtime) =>
        _familyLoadRuntimes[kind] = runtime;

    public IReadOnlyDictionary<ChartProgramKind, LoadRuntimeState> FamilyLoadRuntimes => _familyLoadRuntimes;
    public IReadOnlyDictionary<ChartProgramKind, RenderPlanDiagnosticsSnapshot> RenderPlanDiagnostics => _renderPlanDiagnostics;
    public IReadOnlyList<RenderPlanHistorySnapshot> RenderPlanHistory => _renderPlanHistory;
    public IReadOnlyList<InterpretationResultDiagnosticsSnapshot> InterpretationDiagnostics => _interpretationDiagnostics;
    public IReadOnlyList<PerformanceTimingSnapshot> PerformanceTimings => _performanceTimings;
    public IReadOnlyList<SessionMilestoneSnapshot> SessionMilestones => _sessionMilestones;

    // Current chart titles (left + right)
    public string LeftTitle { get; set; } = string.Empty;
    public string RightTitle { get; set; } = string.Empty;

    // Timestamps linked to each chart
    public Dictionary<CartesianChart, List<DateTime>> ChartTimestamps { get; } = new();

    public DistributionModeSettings GetDistributionSettings(DistributionMode mode)
    {
        if (_distributionSettings.TryGetValue(mode, out var settings))
            return settings;

        var definition = DistributionModeCatalog.Get(mode);
        settings = new DistributionModeSettings(true, definition.DefaultIntervalCount);
        _distributionSettings[mode] = settings;
        return settings;
    }

    public void RecordSessionMilestone(SessionMilestoneSnapshot milestone)
    {
        ArgumentNullException.ThrowIfNull(milestone);

        _sessionMilestones.Add(milestone);
        if (_sessionMilestones.Count <= MaxSessionMilestones)
            return;

        _sessionMilestones.RemoveRange(0, _sessionMilestones.Count - MaxSessionMilestones);
    }

    public void SetRenderPlanDiagnostics(ChartProgramKind kind, ChartRenderAdapterResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var snapshot = new RenderPlanDiagnosticsSnapshot
        {
            BackendKey = result.BackendKey,
            PlanId = result.PlanId,
            PlanKind = result.PlanKind.ToString(),
            DensityMode = result.DensityMode.ToString(),
            RenderedSeriesCount = result.RenderedSeriesCount,
            RenderedHierarchyNodeCount = result.RenderedHierarchyNodeCount,
            RenderedPointCount = result.RenderedPointCount,
            Metadata = result.Metadata
        };
        _renderPlanDiagnostics[kind] = snapshot;
        _renderPlanHistory.Add(new RenderPlanHistorySnapshot
        {
            TimestampUtc = DateTime.UtcNow,
            ProgramKind = kind.ToString(),
            BackendKey = snapshot.BackendKey,
            PlanId = snapshot.PlanId,
            PlanKind = snapshot.PlanKind,
            DensityMode = snapshot.DensityMode,
            RenderedSeriesCount = snapshot.RenderedSeriesCount,
            RenderedHierarchyNodeCount = snapshot.RenderedHierarchyNodeCount,
            RenderedPointCount = snapshot.RenderedPointCount,
            Metadata = snapshot.Metadata
        });

        if (_renderPlanHistory.Count > MaxRenderPlanHistory)
            _renderPlanHistory.RemoveRange(0, _renderPlanHistory.Count - MaxRenderPlanHistory);
    }

    public void ClearRenderPlanDiagnostics()
    {
        _renderPlanDiagnostics.Clear();
        _renderPlanHistory.Clear();
    }

    public void RecordInterpretationDiagnostics(AnalyticalInterpretationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var snapshot = new InterpretationResultDiagnosticsSnapshot
        {
            ProgramKind = result.Execution.Program.Kind.ToString(),
            ExecutionSignature = result.Execution.Signature,
            InterpretationSignature = result.Signature,
            SourceSignature = result.Execution.Program.SourceSignature,
            ConfidenceAnnotationCount = result.Confidence.Annotations.Count,
            CriticalConfidenceAnnotationCount = result.Confidence.CriticalCount,
            WarningConfidenceAnnotationCount = result.Confidence.WarningCount,
            OverlayCount = result.Overlays.Count,
            OverlayKinds = result.Overlays
                .Select(overlay => overlay.Kind.ToString())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToList()
        };

        _interpretationDiagnostics.Add(snapshot);
        if (_interpretationDiagnostics.Count <= MaxInterpretationDiagnostics)
            return;

        _interpretationDiagnostics.RemoveRange(0, _interpretationDiagnostics.Count - MaxInterpretationDiagnostics);
    }

    public void ClearInterpretationDiagnostics()
    {
        _interpretationDiagnostics.Clear();
    }

    public void RecordPerformanceTiming(
        string scope,
        string operation,
        long durationMs,
        EvidenceRuntimePath? runtimePath = null,
        string? note = null)
    {
        if (string.IsNullOrWhiteSpace(scope))
            throw new ArgumentException("Performance timing scope is required.", nameof(scope));

        if (string.IsNullOrWhiteSpace(operation))
            throw new ArgumentException("Performance timing operation is required.", nameof(operation));

        _performanceTimings.Add(new PerformanceTimingSnapshot
        {
            TimestampUtc = DateTime.UtcNow,
            Scope = scope,
            Operation = operation,
            DurationMs = durationMs,
            RuntimePath = runtimePath,
            Note = note
        });

        if (_performanceTimings.Count <= MaxPerformanceTimings)
            return;

        _performanceTimings.RemoveRange(0, _performanceTimings.Count - MaxPerformanceTimings);
    }
}

public sealed record LoadRuntimeState(
    EvidenceRuntimePath RuntimePath,
    string RequestSignature,
    string? SnapshotSignature,
    ChartProgramKind? ProgramKind,
    string? ProgramSourceSignature,
    string? ProjectedContextSignature,
    string? FailureReason,
    bool SupportsOnlyMainChart)
{
    public static LoadRuntimeState FromVNextSuccess(
        EvidenceRuntimePath path, string? requestSignature, string? snapshotSignature,
        ChartProgramKind? programKind, string? programSourceSignature)
    {
        return new LoadRuntimeState(path, requestSignature ?? string.Empty,
            snapshotSignature, programKind, programSourceSignature, null, null, false);
    }

    public static LoadRuntimeState LegacyFallback(string? requestSignature, string? failureReason)
    {
        return new LoadRuntimeState(EvidenceRuntimePath.Legacy, requestSignature ?? string.Empty,
            null, null, null, null, failureReason, false);
    }
}
