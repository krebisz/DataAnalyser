namespace DataVisualiser.Core.Rendering.BarPie;

public sealed record BarPieRenderingQualificationProbeResult(
    BarPieRenderingRoute Route,
    bool InitialRenderPassed,
    bool RepeatedUpdatePassed,
    bool VisibilityTransitionPassed,
    bool ResetViewPassed,
    bool ClearPassed,
    bool DisposalPassed,
    IReadOnlyList<string> Failures)
{
    public bool Passed =>
        InitialRenderPassed &&
        RepeatedUpdatePassed &&
        VisibilityTransitionPassed &&
        ResetViewPassed &&
        ClearPassed &&
        DisposalPassed;
}
