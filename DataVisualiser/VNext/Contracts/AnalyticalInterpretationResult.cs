namespace DataVisualiser.VNext.Contracts;

public sealed record AnalyticalInterpretationResult(
    AnalyticalExecutionResult Execution,
    ConfidenceAnnotationSet Confidence,
    IReadOnlyList<OverlayPlan> Overlays)
{
    public string Signature =>
        $"{Execution.Signature}::{Confidence.Signature}::{string.Join("||", Overlays.Select(overlay => overlay.Signature))}";
}

public sealed record AnalyticalInterpretationSetResult(
    AnalyticalResultSet ExecutionSet,
    IReadOnlyList<AnalyticalInterpretationResult> Interpretations)
{
    public string Signature =>
        $"{ExecutionSet.Signature}::{string.Join("||", Interpretations.Select(interpretation => interpretation.Signature))}";
}
