namespace DataVisualiser.VNext.Rendering;

public sealed class ChartBackendCandidateSet
{
    private readonly IReadOnlyList<ChartBackendCapabilities> _candidates;

    public ChartBackendCandidateSet(IReadOnlyList<ChartBackendCapabilities> candidates)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        _candidates = candidates.ToArray();
    }

    public IReadOnlyList<ChartBackendCapabilities> Candidates => _candidates;

    public IReadOnlyList<ChartBackendCapabilities> FindQualified(ChartRenderPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        return _candidates
            .Where(candidate => ChartRenderPlanAdapterQualificationRules.CanRender(candidate, plan))
            .ToArray();
    }

    public IReadOnlyList<ChartBackendCapabilities> FindQualified(
        ChartRenderPlanKind planKind,
        string? providerKey = null)
    {
        return _candidates
            .Where(candidate => candidate.Supports(planKind))
            .Where(candidate => string.IsNullOrWhiteSpace(providerKey) ||
                                string.Equals(candidate.BackendKey, providerKey, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    public ChartBackendCapabilities Select(
        ChartRenderPlanKind planKind,
        string? providerKey = null)
    {
        var match = FindQualified(planKind, providerKey).FirstOrDefault();
        if (match == null)
        {
            var providerDescription = !string.IsNullOrWhiteSpace(providerKey)
                ? $" for provider '{providerKey}'"
                : string.Empty;
            throw new InvalidOperationException(
                $"No chart backend supports render plan kind '{planKind}'{providerDescription}.");
        }

        return match;
    }

    public static ChartBackendCandidateSet BuiltIn { get; } = new(
        [
            ChartBackendCapabilities.LiveChartsWpf,
            ChartBackendCapabilities.SyncfusionSunburst,
            ChartBackendCapabilities.TabularSummary
        ]);
}
