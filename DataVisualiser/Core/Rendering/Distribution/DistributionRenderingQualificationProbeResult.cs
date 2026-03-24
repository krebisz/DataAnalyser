namespace DataVisualiser.Core.Rendering.Distribution;

public sealed record DistributionRenderingQualificationProbeResult(
    DistributionRenderingRoute Route,
    bool InitialRenderPassed,
    bool RepeatedUpdatePassed,
    bool VisibilityTransitionPassed,
    bool OffscreenTransitionPassed,
    bool ResetViewPassed,
    bool ClearPassed,
    bool DisposalPassed,
    IReadOnlyList<string> Failures)
{
    public bool Passed =>
        InitialRenderPassed &&
        RepeatedUpdatePassed &&
        VisibilityTransitionPassed &&
        OffscreenTransitionPassed &&
        ResetViewPassed &&
        ClearPassed &&
        DisposalPassed;
}
