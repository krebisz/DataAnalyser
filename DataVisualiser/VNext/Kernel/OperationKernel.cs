using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.VNext.Kernel;

public sealed class OperationKernel
{
    public ChartSeriesProgram BuildSummedSeries(AlignedSeriesBundle bundle, string label = "Sum")
    {
        ArgumentNullException.ThrowIfNull(bundle);

        if (bundle.Series.Count == 0)
            return new ChartSeriesProgram("sum", label, Array.Empty<double>(), Array.Empty<double>());

        var raw = new double[bundle.Timeline.Count];
        var smoothed = new double[bundle.Timeline.Count];

        for (var index = 0; index < bundle.Timeline.Count; index++)
        {
            raw[index] = bundle.Series.Sum(series => double.IsNaN(series.RawValues[index]) ? 0d : series.RawValues[index]);
            smoothed[index] = bundle.Series.Sum(series => double.IsNaN(series.SmoothedValues[index]) ? 0d : series.SmoothedValues[index]);
        }

        return new ChartSeriesProgram("sum", label, raw, smoothed);
    }

    public ChartSeriesProgram BuildDifferenceSeries(AlignedMetricSeries primary, AlignedMetricSeries secondary)
    {
        return BuildBinarySeries(
            "difference",
            $"{primary.Request.DisplayName} - {secondary.Request.DisplayName}",
            primary.RawValues,
            secondary.RawValues,
            (left, right) => left - right);
    }

    public ChartSeriesProgram BuildRatioSeries(AlignedMetricSeries primary, AlignedMetricSeries secondary)
    {
        return BuildBinarySeries(
            "ratio",
            $"{primary.Request.DisplayName} / {secondary.Request.DisplayName}",
            primary.RawValues,
            secondary.RawValues,
            (left, right) => right == 0d ? double.NaN : left / right);
    }

    public ChartSeriesProgram BuildNormalizedSeries(AlignedMetricSeries series)
    {
        return new ChartSeriesProgram(
            $"normalized::{series.Request.SignatureToken}",
            $"{series.Request.DisplayName} (normalized)",
            Normalize(series.RawValues),
            Normalize(series.SmoothedValues));
    }

    private static ChartSeriesProgram BuildBinarySeries(
        string id,
        string label,
        IReadOnlyList<double> leftValues,
        IReadOnlyList<double> rightValues,
        Func<double, double, double> operation)
    {
        var result = new double[leftValues.Count];
        for (var index = 0; index < leftValues.Count; index++)
        {
            var left = leftValues[index];
            var right = rightValues[index];
            result[index] = double.IsNaN(left) || double.IsNaN(right) ? double.NaN : operation(left, right);
        }

        return new ChartSeriesProgram(id, label, result, result.ToArray());
    }

    private static IReadOnlyList<double> Normalize(IReadOnlyList<double> values)
    {
        var valid = values.Where(value => !double.IsNaN(value)).ToList();
        if (valid.Count == 0)
            return values.ToList();

        var max = valid.Max();
        if (Math.Abs(max) < double.Epsilon)
            return values.ToList();

        return values.Select(value => double.IsNaN(value) ? double.NaN : value / max).ToList();
    }
}
