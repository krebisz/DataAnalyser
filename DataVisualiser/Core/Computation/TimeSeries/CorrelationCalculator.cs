namespace DataVisualiser.Core.Computation.TimeSeries;

public static class CorrelationCalculator
{
    public static CorrelationResult Pearson(IReadOnlyList<double> left, IReadOnlyList<double> right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        var pairs = left.Zip(right)
            .Where(pair => double.IsFinite(pair.First) && double.IsFinite(pair.Second))
            .ToArray();
        if (pairs.Length < 2)
            return new CorrelationResult(double.NaN, double.NaN, double.NaN, pairs.Length);

        var leftMean = pairs.Average(pair => pair.First);
        var rightMean = pairs.Average(pair => pair.Second);
        var numerator = pairs.Sum(pair => (pair.First - leftMean) * (pair.Second - rightMean));
        var leftVariance = pairs.Sum(pair => Math.Pow(pair.First - leftMean, 2));
        var rightVariance = pairs.Sum(pair => Math.Pow(pair.Second - rightMean, 2));
        var denominator = Math.Sqrt(leftVariance * rightVariance);
        var correlation = denominator <= double.Epsilon ? double.NaN : numerator / denominator;
        var (lower, upper) = CalculateFisherConfidenceInterval(correlation, pairs.Length);

        return new CorrelationResult(correlation, lower, upper, pairs.Length);
    }

    public static (double Lower, double Upper) CalculateFisherConfidenceInterval(double correlation, int sampleCount)
    {
        if (!double.IsFinite(correlation) || sampleCount < 4)
            return (double.NaN, double.NaN);

        var bounded = Math.Clamp(correlation, -0.999999d, 0.999999d);
        var z = 0.5d * Math.Log((1d + bounded) / (1d - bounded));
        var delta = 1.96d / Math.Sqrt(sampleCount - 3d);
        return (Math.Tanh(z - delta), Math.Tanh(z + delta));
    }
}

public sealed record CorrelationResult(
    double Correlation,
    double ConfidenceLower,
    double ConfidenceUpper,
    int SampleCount);
