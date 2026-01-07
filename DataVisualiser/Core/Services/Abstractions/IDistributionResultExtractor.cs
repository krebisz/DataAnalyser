namespace DataVisualiser.Core.Services.Abstractions;

/// <summary>
///     Interface for extracting bucket values from distribution results.
///     Allows the base service to work with different result types (WeeklyDistributionResult, BucketDistributionResult,
///     etc.)
/// </summary>
public interface IDistributionResultExtractor
{
    /// <summary>
    ///     Extracts bucket values from the result.
    ///     Returns a dictionary mapping bucket index to list of values for that bucket.
    /// </summary>
    Dictionary<int, List<double>> ExtractBucketValues(object result);
}