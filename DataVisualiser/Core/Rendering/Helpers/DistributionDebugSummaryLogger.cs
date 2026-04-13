using System.Diagnostics;

namespace DataVisualiser.Core.Rendering.Helpers;

internal static class DistributionDebugSummaryLogger
{
    public static void LogSummary(
        string logPrefix,
        string bucketName,
        int bucketCount,
        IReadOnlyList<double> mins,
        IReadOnlyList<double> ranges,
        IReadOnlyDictionary<int, List<double>> bucketValues,
        double globalMin,
        double globalMax)
    {
        Debug.WriteLine($"=== {logPrefix}: Data Summary ===");
        Debug.WriteLine($"Global Min: {globalMin:F4}, Global Max: {globalMax:F4}, Range: {globalMax - globalMin:F4}");
        Debug.WriteLine($"{bucketName} Min/Max values:");

        for (var i = 0; i < bucketCount; i++)
        {
            var bucketMin = double.IsNaN(mins[i]) ? 0.0 : mins[i];
            var bucketMax = bucketMin + (double.IsNaN(ranges[i]) ? 0.0 : ranges[i]);
            Debug.WriteLine($"  {bucketName} {i}: Min={bucketMin:F4}, Max={bucketMax:F4}, Range={ranges[i]:F4}");
        }

        for (var bucketIndex = 0; bucketIndex < bucketCount; bucketIndex++)
            if (bucketValues.TryGetValue(bucketIndex, out var values) && values.Count > 0)
            {
                Debug.WriteLine($"{bucketName} {bucketIndex} raw values (first 10): {string.Join(", ", values.Take(10).Select(v => v.ToString("F4")))}");
                Debug.WriteLine($"{bucketName} {bucketIndex} total value count: {values.Count}");
                break;
            }
    }
}
