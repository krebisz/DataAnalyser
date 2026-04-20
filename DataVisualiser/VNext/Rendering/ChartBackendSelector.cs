namespace DataVisualiser.VNext.Rendering;

public sealed class ChartBackendSelector
{
    public ChartBackendCapabilities Select(ChartRenderPlan plan, IReadOnlyList<ChartBackendCapabilities> candidates)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(candidates);

        var match = candidates.FirstOrDefault(candidate => candidate.Supports(plan.PlanKind));
        if (match == null)
            throw new InvalidOperationException($"No chart backend supports render plan kind '{plan.PlanKind}'.");

        return match;
    }
}
