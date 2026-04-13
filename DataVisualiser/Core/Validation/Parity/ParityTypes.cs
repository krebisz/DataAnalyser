namespace DataVisualiser.Core.Validation.Parity;

public enum ParityLayer
{
    StructuralParity,
    TemporalParity,
    ValueParity
}

public enum ParityMode
{
    Diagnostic, // return failures but do not throw
    Strict      // throw on first failure
}

public sealed class ParityFailure
{
    public ParityLayer Layer { get; init; }
    public string Message { get; init; } = string.Empty;
}

public sealed class ParityPoint
{
    public DateTime Time { get; init; }
    public double Value { get; init; }
}

public sealed class ParitySeries
{
    public string SeriesKey { get; init; } = string.Empty;
    public IReadOnlyList<ParityPoint> Points { get; init; } = Array.Empty<ParityPoint>();
}

public sealed class ParityTolerance
{
    public double ValueEpsilon { get; init; } = 0.0;
    public bool AllowFloatingPointDrift { get; init; } = false;
}
