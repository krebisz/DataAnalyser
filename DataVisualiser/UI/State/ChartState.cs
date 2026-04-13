using DataVisualiser.Core.Orchestration;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.MainHost.Evidence;
using DataVisualiser.VNext.Contracts;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.State;

public class ChartState
{
    private readonly Dictionary<DistributionMode, DistributionModeSettings> _distributionSettings = new();
    private readonly List<SessionMilestoneSnapshot> _sessionMilestones = new();
    private const int MaxSessionMilestones = 50;

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
    public NormalizationMode SelectedNormalizationMode { get; set; }

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
    public LoadRuntimeState? LastDistributionLoadRuntime { get; set; }
    public LoadRuntimeState? LastWeekdayTrendLoadRuntime { get; set; }
    public LoadRuntimeState? LastTransformLoadRuntime { get; set; }
    public LoadRuntimeState? LastBarPieLoadRuntime { get; set; }
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
