using System.Text.Json.Serialization;

namespace DataVisualiser.UI.MainHost;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EvidenceRuntimePath
{
    Legacy,
    VNextMain
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
    public EvidenceRuntimePath RuntimePath { get; set; } = EvidenceRuntimePath.Legacy;
    public SelectionDiagnosticsSnapshot Selection { get; set; } = new();
    public LoadedContextDiagnosticsSnapshot LoadedContext { get; set; } = new();
    public MainChartPipelineDiagnosticsSnapshot MainChartPipeline { get; set; } = new();
    public ReachabilityDiagnosticsSnapshot Reachability { get; set; } = new();
    public UiSurfaceDiagnosticsSnapshot UiSurface { get; set; } = new();
    public SmokeHeuristicsSnapshot SmokeChecks { get; set; } = new();
    public TransitionDiagnosticsSnapshot Transition { get; set; } = new();
    public VNextDiagnosticsSnapshot? VNext { get; set; }
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

public sealed class VNextDiagnosticsSnapshot
{
    public string? RequestSignature { get; set; }
    public string? SnapshotSignature { get; set; }
    public string? ProgramKind { get; set; }
    public string? ProgramSourceSignature { get; set; }
    public string? ProjectedContextSignature { get; set; }
    public bool RequestMatchesSnapshot { get; set; }
    public bool SnapshotMatchesProgramSource { get; set; }
    public bool ProgramSourceMatchesProjectedContext { get; set; }
    public bool SupportsOnlyMainChart { get; set; }
    public string? FailureReason { get; set; }
}
