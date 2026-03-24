namespace DataVisualiser.Core.Rendering.WeekdayTrend;

public sealed record WeekdayTrendRenderingQualificationProbeResult(
    WeekdayTrendRenderingRoute Route,
    bool InitialRenderPassed,
    bool RepeatedUpdatePassed,
    bool VisibilityTransitionPassed,
    bool OffscreenTransitionPassed,
    bool ResetViewPassed,
    bool ClearPassed,
    IReadOnlyList<string> Failures)
{
    public bool Passed =>
        InitialRenderPassed &&
        RepeatedUpdatePassed &&
        VisibilityTransitionPassed &&
        OffscreenTransitionPassed &&
        ResetViewPassed &&
        ClearPassed;
}
