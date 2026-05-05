namespace DataVisualiser.VNext.Contracts;

public static class DerivedDatasetFitnessRules
{
    public const string FiniteCoverageScenario = "DerivedDatasetFiniteCoverage";
    public const double UsefulCoverageThreshold = 0.8d;
    public const double CautionCoverageThreshold = 0.5d;

    public static AnalyticalFitnessAssessment Evaluate(
        string datasetId,
        string operationSignature,
        IReadOnlyList<double> rawValues,
        IReadOnlyList<double> smoothedValues,
        ConfidenceAnnotationSet confidence)
    {
        if (string.IsNullOrWhiteSpace(datasetId))
            throw new ArgumentException("Dataset id cannot be null or empty.", nameof(datasetId));
        if (string.IsNullOrWhiteSpace(operationSignature))
            throw new ArgumentException("Operation signature cannot be null or empty.", nameof(operationSignature));
        ArgumentNullException.ThrowIfNull(rawValues);
        ArgumentNullException.ThrowIfNull(smoothedValues);
        ArgumentNullException.ThrowIfNull(confidence);

        var rawCoverage = CalculateFiniteCoverage(rawValues);
        var smoothedCoverage = CalculateFiniteCoverage(smoothedValues);
        var minimumCoverage = Math.Min(rawCoverage, smoothedCoverage);
        var status = minimumCoverage >= UsefulCoverageThreshold && confidence.CriticalCount == 0
            ? AnalyticalFitnessStatus.Useful
            : minimumCoverage >= CautionCoverageThreshold
                ? AnalyticalFitnessStatus.Caution
                : AnalyticalFitnessStatus.DistortionProne;

        return new AnalyticalFitnessAssessment(
            FiniteCoverageScenario,
            status,
            $"Raw and smoothed finite coverage must remain above {CautionCoverageThreshold:P0}; {UsefulCoverageThreshold:P0} is useful.",
            CreateMessage(status),
            new Dictionary<string, string>
            {
                ["DatasetId"] = datasetId,
                [ConstructionMetadataKeys.OperationSignature] = operationSignature,
                ["RawFiniteCoverage"] = rawCoverage.ToString("0.###"),
                ["SmoothedFiniteCoverage"] = smoothedCoverage.ToString("0.###"),
                ["CriticalConfidenceAnnotations"] = confidence.CriticalCount.ToString(),
                ["Authoritative"] = "False"
            });
    }

    private static double CalculateFiniteCoverage(IReadOnlyList<double> values)
    {
        if (values.Count == 0)
            return 0d;

        return values.Count(double.IsFinite) / (double)values.Count;
    }

    private static string CreateMessage(AnalyticalFitnessStatus status) =>
        status switch
        {
            AnalyticalFitnessStatus.Useful => "Derived dataset is finite enough for ordinary analytical consumption.",
            AnalyticalFitnessStatus.Caution => "Derived dataset is valid but has enough non-finite output to require caution.",
            AnalyticalFitnessStatus.DistortionProne => "Derived dataset is valid but non-finite output dominates the construction.",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
}
