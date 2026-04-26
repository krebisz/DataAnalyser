using DataVisualiser.VNext.Rendering;

namespace DataVisualiser.VNext.Contracts;

public sealed record ConsumerProviderContract
{
    public ConsumerProviderContract(
        string providerKey,
        string displayName,
        ConsumerKind consumerKind,
        IReadOnlySet<ChartProgramKind> supportedProgramKinds,
        IReadOnlySet<ChartRenderPlanKind> supportedPlanKinds,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        if (string.IsNullOrWhiteSpace(providerKey))
            throw new ArgumentException("Provider key cannot be null or empty.", nameof(providerKey));
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name cannot be null or empty.", nameof(displayName));
        ArgumentNullException.ThrowIfNull(supportedProgramKinds);
        ArgumentNullException.ThrowIfNull(supportedPlanKinds);

        ProviderKey = providerKey;
        DisplayName = displayName;
        ConsumerKind = consumerKind;
        SupportedProgramKinds = supportedProgramKinds.ToHashSet();
        SupportedPlanKinds = supportedPlanKinds.ToHashSet();
        Metadata = metadata == null
            ? new Dictionary<string, string>()
            : new Dictionary<string, string>(metadata);
    }

    public string ProviderKey { get; }
    public string DisplayName { get; }
    public ConsumerKind ConsumerKind { get; }
    public IReadOnlySet<ChartProgramKind> SupportedProgramKinds { get; }
    public IReadOnlySet<ChartRenderPlanKind> SupportedPlanKinds { get; }
    public IReadOnlyDictionary<string, string> Metadata { get; }

    public string Signature =>
        $"{ProviderKey}:{ConsumerKind}:{string.Join(",", SupportedProgramKinds.OrderBy(kind => kind))}:{string.Join(",", SupportedPlanKinds.OrderBy(kind => kind))}";

    public bool Supports(ConsumerDeliveryContract delivery, ChartRenderPlanKind? planKind = null)
    {
        ArgumentNullException.ThrowIfNull(delivery);

        if (delivery.ConsumerKind != ConsumerKind)
            return false;

        if (!SupportedProgramKinds.Contains(delivery.ProgramKind))
            return false;

        return planKind == null || SupportedPlanKinds.Contains(planKind.Value);
    }
}
