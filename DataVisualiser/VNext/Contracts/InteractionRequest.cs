namespace DataVisualiser.VNext.Contracts;

public sealed record InteractionRequest(
    InteractionKind Kind,
    string Target,
    IReadOnlyDictionary<string, string>? Parameters = null)
{
    public IReadOnlyDictionary<string, string> ResolvedParameters { get; } = Parameters == null
        ? new Dictionary<string, string>()
        : new Dictionary<string, string>(Parameters);

    public string Signature => $"{Kind}:{Target}:{string.Join("|", ResolvedParameters.OrderBy(item => item.Key).Select(item => $"{item.Key}={item.Value}"))}";
}
