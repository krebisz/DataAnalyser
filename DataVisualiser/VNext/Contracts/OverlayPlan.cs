namespace DataVisualiser.VNext.Contracts;

public sealed record OverlayPlan(
    OverlayKind Kind,
    string Label,
    IReadOnlyDictionary<string, string>? Parameters = null)
{
    public IReadOnlyDictionary<string, string> ResolvedParameters { get; } = Parameters == null
        ? new Dictionary<string, string>()
        : new Dictionary<string, string>(Parameters);

    public string Signature => $"{Kind}:{Label}:{string.Join("|", ResolvedParameters.OrderBy(item => item.Key).Select(item => $"{item.Key}={item.Value}"))}";
}
