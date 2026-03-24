namespace DataVisualiser.Core.Rendering.Transform;

public sealed record TransformRenderingQualificationProbeResult(
    TransformRenderingRoute Route,
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
