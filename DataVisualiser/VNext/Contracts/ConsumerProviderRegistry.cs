using DataVisualiser.VNext.Rendering;

namespace DataVisualiser.VNext.Contracts;

public sealed class ConsumerProviderRegistry
{
    private readonly IReadOnlyList<ConsumerProviderContract> _providers;

    public ConsumerProviderRegistry(IReadOnlyList<ConsumerProviderContract> providers)
    {
        ArgumentNullException.ThrowIfNull(providers);

        var duplicate = providers
            .GroupBy(provider => provider.ProviderKey, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicate != null)
            throw new ArgumentException($"Duplicate consumer provider key '{duplicate.Key}'.", nameof(providers));

        _providers = providers.ToArray();
    }

    public IReadOnlyList<ConsumerProviderContract> Providers => _providers;

    public IReadOnlyList<ConsumerProviderContract> FindCandidates(
        ConsumerDeliveryContract delivery,
        ChartRenderPlanKind? planKind = null)
    {
        ArgumentNullException.ThrowIfNull(delivery);

        return _providers
            .Where(candidate => candidate.Supports(delivery, planKind))
            .ToArray();
    }

    public bool TryResolve(
        ConsumerDeliveryContract delivery,
        out ConsumerProviderContract? provider,
        ChartRenderPlanKind? planKind = null)
    {
        ArgumentNullException.ThrowIfNull(delivery);

        provider = _providers.FirstOrDefault(candidate => candidate.Supports(delivery, planKind));
        return provider != null;
    }

    public ConsumerProviderContract Resolve(
        ConsumerDeliveryContract delivery,
        ChartRenderPlanKind? planKind = null)
    {
        ArgumentNullException.ThrowIfNull(delivery);

        if (!TryResolve(delivery, out var provider, planKind))
        {
            var planDescription = planKind.HasValue ? $" and render plan kind '{planKind}'" : string.Empty;
            throw new InvalidOperationException(
                $"No consumer provider supports '{delivery.ConsumerKind}' delivery for program '{delivery.ProgramKind}'{planDescription}.");
        }

        return provider!;
    }

    public static ConsumerProviderRegistry BuiltIn { get; } = new(
        [
            ConsumerProviderContracts.LiveChartsWpf,
            ConsumerProviderContracts.SyncfusionSunburst,
            ConsumerProviderContracts.TabularSummaryChart,
            ConsumerProviderContracts.EvidenceExport,
            ConsumerProviderContracts.ApiResponse
        ]);
}
