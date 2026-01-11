namespace DataVisualiser.Core.Validation.Parity;

public sealed class ParityTolerance
{
    public double ValueEpsilon { get; init; } = 0.0;
    public bool AllowFloatingPointDrift { get; init; } = false;
}
