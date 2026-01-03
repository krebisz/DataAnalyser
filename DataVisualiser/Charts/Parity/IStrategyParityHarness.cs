// File: DataVisualiser/Charts/Parity/IStrategyParityHarness.cs

namespace DataVisualiser.Charts.Parity;
// ---------- Contracts (kept in this file to avoid "missing type" drift) ----------

public enum ParityMode
{
    Diagnostic, // return failures but do not throw
    Strict // throw on first failure
}

public enum ParityLayer
{
    InputParity,
    StructuralParity,
    TemporalParity,
    ValueParity,
    SemanticIntegrity,
    PresentationParity
}

public sealed class StrategyParityContext
{
    public string StrategyName { get; init; } = string.Empty;
    public string MetricIdentity { get; init; } = string.Empty; // opaque string; do not parse
    public ParityTolerance Tolerance { get; init; } = new();
    public ParityMode Mode { get; init; } = ParityMode.Diagnostic;
}

public sealed class ParityTolerance
{
    public double ValueEpsilon { get; init; } = 0.0;
    public bool AllowFloatingPointDrift { get; init; } = false;
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

public sealed class LegacyExecutionResult
{
    public IReadOnlyList<ParitySeries> Series { get; init; } = Array.Empty<ParitySeries>();
}

public sealed class CmsExecutionResult
{
    public IReadOnlyList<ParitySeries> Series { get; init; } = Array.Empty<ParitySeries>();
}

public sealed class ParityFailure
{
    public ParityLayer Layer { get; init; }
    public string Message { get; init; } = string.Empty;
}

public sealed class ParityResult
{
    public bool Passed { get; init; }
    public IReadOnlyList<ParityFailure> Failures { get; init; } = Array.Empty<ParityFailure>();

    public static ParityResult Pass()
    {
        return new ParityResult
        {
            Passed = true
        };
    }

    public static ParityResult Fail(params ParityFailure[] failures)
    {
        return new ParityResult
        {
            Passed = false,
            Failures = failures ?? Array.Empty<ParityFailure>()
        };
    }
}

// ---------- Harness interface ----------

public interface IStrategyParityHarness
{
    ParityResult Validate(StrategyParityContext context, Func<LegacyExecutionResult> legacyExecution, Func<CmsExecutionResult> cmsExecution);
}