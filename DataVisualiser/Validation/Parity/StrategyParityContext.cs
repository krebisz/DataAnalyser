namespace DataVisualiser.Validation.Parity;

public sealed class StrategyParityContext
{
    public string StrategyName { get; init; } = string.Empty;
    public string MetricIdentity { get; init; } = string.Empty; // opaque string; do not parse
    public ParityTolerance Tolerance { get; init; } = new();
    public ParityMode Mode { get; init; } = ParityMode.Diagnostic;
}
