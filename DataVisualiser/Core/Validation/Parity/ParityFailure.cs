namespace DataVisualiser.Core.Validation.Parity;

public sealed class ParityFailure
{
    public ParityLayer Layer { get; init; }
    public string Message { get; init; } = string.Empty;
}
