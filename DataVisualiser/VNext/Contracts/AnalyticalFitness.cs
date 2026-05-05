namespace DataVisualiser.VNext.Contracts;

public enum AnalyticalFitnessStatus
{
    Useful = 0,
    Caution = 1,
    DistortionProne = 2
}

public sealed record AnalyticalFitnessAssessment(
    string Scenario,
    AnalyticalFitnessStatus Status,
    string Criterion,
    string Message,
    IReadOnlyDictionary<string, string>? Metadata = null)
{
    public IReadOnlyDictionary<string, string> ResolvedMetadata { get; } = Metadata == null
        ? new Dictionary<string, string>()
        : new Dictionary<string, string>(Metadata);
}
