using DataVisualiser.VNext.Kernel;

namespace DataVisualiser.UI.Charts.Presentation;

internal sealed record TransformCorrelationSummary(
    string Label,
    string SourceLabel,
    string TargetLabel,
    double Correlation,
    double ConfidenceLower,
    double ConfidenceUpper,
    int SampleCount);

internal static class TransformCorrelationMapper
{
    public static bool IsCorrelationOperation(string operationTag) =>
        string.Equals(operationTag, "Correlation", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(operationTag, "Sum3Correlation", StringComparison.OrdinalIgnoreCase);

    public static TransformCorrelationSummary Compute(AlignedSeriesBundle aligned, string operationTag)
    {
        ArgumentNullException.ThrowIfNull(aligned);

        return operationTag switch
        {
            "Correlation" when aligned.Series.Count >= 2 => ComputePair(
                aligned.Series[0].RawValues,
                aligned.Series[1].RawValues,
                aligned.Series[0].Request.DisplayName,
                aligned.Series[1].Request.DisplayName,
                $"{aligned.Series[0].Request.DisplayName} ~ {aligned.Series[1].Request.DisplayName}"),
            "Sum3Correlation" when aligned.Series.Count >= 4 => ComputePair(
                Sum(aligned.Series.Take(3).Select(series => series.RawValues), aligned.Timeline.Count),
                aligned.Series[3].RawValues,
                $"{ResolveShortLabel(aligned.Series[0])} + {ResolveShortLabel(aligned.Series[1])} + {ResolveShortLabel(aligned.Series[2])}",
                aligned.Series[3].Request.DisplayName,
                $"Correlation: ({ResolveShortLabel(aligned.Series[0])} + {ResolveShortLabel(aligned.Series[1])} + {ResolveShortLabel(aligned.Series[2])}) ~ {ResolveShortLabel(aligned.Series[3])}"),
            "Sum3Correlation" => throw new InvalidOperationException("Ternary correlation requires three source inputs plus one comparison input."),
            _ => throw new InvalidOperationException($"Unsupported correlation operation '{operationTag}'.")
        };
    }

    private static TransformCorrelationSummary ComputePair(
        IReadOnlyList<double> left,
        IReadOnlyList<double> right,
        string leftLabel,
        string rightLabel,
        string label)
    {
        var pairs = left.Zip(right)
            .Where(pair => double.IsFinite(pair.First) && double.IsFinite(pair.Second))
            .ToArray();
        if (pairs.Length < 2)
            return new TransformCorrelationSummary(label, leftLabel, rightLabel, double.NaN, double.NaN, double.NaN, pairs.Length);

        var leftMean = pairs.Average(pair => pair.First);
        var rightMean = pairs.Average(pair => pair.Second);
        var numerator = pairs.Sum(pair => (pair.First - leftMean) * (pair.Second - rightMean));
        var leftVariance = pairs.Sum(pair => Math.Pow(pair.First - leftMean, 2));
        var rightVariance = pairs.Sum(pair => Math.Pow(pair.Second - rightMean, 2));
        var denominator = Math.Sqrt(leftVariance * rightVariance);
        var correlation = denominator <= double.Epsilon ? double.NaN : numerator / denominator;
        var (lower, upper) = CalculateConfidenceInterval(correlation, pairs.Length);

        return new TransformCorrelationSummary(label, leftLabel, rightLabel, correlation, lower, upper, pairs.Length);
    }

    private static (double Lower, double Upper) CalculateConfidenceInterval(double correlation, int sampleCount)
    {
        if (!double.IsFinite(correlation) || sampleCount < 4)
            return (double.NaN, double.NaN);

        var bounded = Math.Clamp(correlation, -0.999999d, 0.999999d);
        var z = 0.5d * Math.Log((1d + bounded) / (1d - bounded));
        var delta = 1.96d / Math.Sqrt(sampleCount - 3d);
        return (Math.Tanh(z - delta), Math.Tanh(z + delta));
    }

    private static IReadOnlyList<double> Sum(IEnumerable<IReadOnlyList<double>> seriesValues, int count)
    {
        var inputs = seriesValues.ToArray();
        var result = new double[count];
        for (var index = 0; index < count; index++)
            result[index] = inputs.Sum(values => double.IsNaN(values[index]) ? 0d : values[index]);

        return result;
    }

    private static string ResolveShortLabel(AlignedMetricSeries series) =>
        string.IsNullOrWhiteSpace(series.Request.DisplaySubtype)
            ? series.Request.DisplayName
            : series.Request.DisplaySubtype;
}
