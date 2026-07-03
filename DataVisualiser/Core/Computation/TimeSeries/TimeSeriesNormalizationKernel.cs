using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Computation.TimeSeries;

public static class TimeSeriesNormalizationKernel
{
    public static IReadOnlyList<double> Normalize(
        IReadOnlyList<double> values,
        NormalizationMode mode)
    {
        ArgumentNullException.ThrowIfNull(values);

        return NormalizeSeries([values], mode)[0];
    }

    public static IReadOnlyList<IReadOnlyList<double>> NormalizeSeries(
        IReadOnlyList<IReadOnlyList<double>> seriesValues,
        NormalizationMode mode)
    {
        ArgumentNullException.ThrowIfNull(seriesValues);

        return mode == NormalizationMode.RelativeToMax
            ? NormalizeAgainstSharedMax(seriesValues)
            : seriesValues.Select(values => NormalizeIndependent(values, mode)).ToArray();
    }

    private static IReadOnlyList<double> NormalizeIndependent(
        IReadOnlyList<double> values,
        NormalizationMode mode)
    {
        var valid = values.Where(value => !double.IsNaN(value)).ToList();
        if (valid.Count == 0)
            return values.Select(_ => double.NaN).ToArray();

        var min = valid.Min();
        var max = valid.Max();

        return mode switch
        {
            NormalizationMode.ZeroToOne => max == min
                ? values.Select(value => double.IsNaN(value) ? double.NaN : 0d).ToArray()
                : values.Select(value => double.IsNaN(value) ? double.NaN : (value - min) / (max - min)).ToArray(),
            NormalizationMode.PercentageOfMax => max == 0d
                ? values.Select(value => double.IsNaN(value) ? double.NaN : 0d).ToArray()
                : values.Select(value => double.IsNaN(value) ? double.NaN : value / max * 100d).ToArray(),
            _ => throw new NotSupportedException($"Normalization mode '{mode}' is not supported for independent series normalization.")
        };
    }

    private static IReadOnlyList<IReadOnlyList<double>> NormalizeAgainstSharedMax(
        IReadOnlyList<IReadOnlyList<double>> seriesValues)
    {
        var sharedMax = seriesValues
            .SelectMany(values => values)
            .Where(value => !double.IsNaN(value))
            .DefaultIfEmpty(double.NaN)
            .Max();

        if (double.IsNaN(sharedMax))
            return seriesValues.Select(values => values.Select(_ => double.NaN).ToArray()).ToArray();

        if (sharedMax == 0d)
            return seriesValues
                .Select(values => values.Select(value => double.IsNaN(value) ? double.NaN : 0d).ToArray())
                .ToArray();

        return seriesValues
            .Select(values => values.Select(value => double.IsNaN(value) ? double.NaN : value / sharedMax * 100d).ToArray())
            .ToArray();
    }
}
