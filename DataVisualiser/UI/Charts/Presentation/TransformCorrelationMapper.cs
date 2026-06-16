using DataVisualiser.Core.Computation.TimeSeries;
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
        var result = CorrelationCalculator.Pearson(left, right);
        return new TransformCorrelationSummary(label, leftLabel, rightLabel, result.Correlation, result.ConfidenceLower, result.ConfidenceUpper, result.SampleCount);
    }

    private static IReadOnlyList<double> Sum(IEnumerable<IReadOnlyList<double>> seriesValues, int count) =>
        SeriesMath.SumByIndex(seriesValues, count);

    private static string ResolveShortLabel(AlignedMetricSeries series) =>
        string.IsNullOrWhiteSpace(series.Request.DisplaySubtype)
            ? series.Request.DisplayName
            : series.Request.DisplaySubtype;
}
