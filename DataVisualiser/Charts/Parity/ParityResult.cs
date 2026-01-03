namespace DataVisualiser.Charts.Parity;

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