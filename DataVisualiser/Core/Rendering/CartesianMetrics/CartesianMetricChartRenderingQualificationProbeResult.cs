namespace DataVisualiser.Core.Rendering.CartesianMetrics;

public sealed record CartesianMetricChartRenderingQualificationProbeResult(
    CartesianMetricChartRoute Route,
    bool InitialRenderPassed,
    bool RepeatedUpdatePassed,
    bool VisibilityTransitionPassed,
    bool OffscreenTransitionPassed,
    bool ResetViewPassed,
    bool ClearPassed,
    IReadOnlyList<string> Failures)
{
    public bool Passed => InitialRenderPassed
                          && RepeatedUpdatePassed
                          && VisibilityTransitionPassed
                          && OffscreenTransitionPassed
                          && ResetViewPassed
                          && ClearPassed
                          && Failures.Count == 0;
}
