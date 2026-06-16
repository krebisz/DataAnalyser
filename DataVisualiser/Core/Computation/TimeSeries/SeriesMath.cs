using DataVisualiser.Core.Configuration.Defaults;

namespace DataVisualiser.Core.Computation.TimeSeries;

public static class SeriesMath
{
    public static IReadOnlyList<double> Smooth(IReadOnlyList<double> values, int window)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (window <= 1)
            return values.ToList();

        var result = new double[values.Count];

        for (var i = 0; i < values.Count; i++)
        {
            var start = Math.Max(0, i - window);
            var end = Math.Min(values.Count - 1, i + window);
            var count = end - start + 1;

            double sum = 0;
            for (var j = start; j <= end; j++)
                sum += values[j];

            result[i] = sum / count;
        }

        return result.ToList();
    }

    public static IReadOnlyList<double> Difference(IReadOnlyList<double> primaryValues, IReadOnlyList<double> secondaryValues)
    {
        var count = Math.Min(primaryValues.Count, secondaryValues.Count);
        var result = new double[count];
        for (var i = 0; i < count; i++)
            result[i] = primaryValues[i] - secondaryValues[i];

        return result.ToList();
    }

    public static IReadOnlyList<double> Ratio(IReadOnlyList<double> primaryValues, IReadOnlyList<double> secondaryValues)
    {
        var count = Math.Min(primaryValues.Count, secondaryValues.Count);
        var result = new double[count];
        for (var i = 0; i < count; i++)
            result[i] = secondaryValues[i] == 0 ? ComputationDefaults.RatioDivideByZeroValue : primaryValues[i] / secondaryValues[i];

        return result.ToList();
    }

    public static IReadOnlyList<double> NormalizeToMax(IReadOnlyList<double> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (values.Count == 0)
            return Array.Empty<double>();

        var max = values.Max();
        if (max <= 0)
            return values.ToList();

        return values.Select(v => v / max).ToList();
    }

    public static IReadOnlyList<double> SumByIndex(IEnumerable<IReadOnlyList<double>> seriesValues, int count)
    {
        var inputs = seriesValues?.ToArray() ?? throw new ArgumentNullException(nameof(seriesValues));
        var result = new double[count];
        for (var index = 0; index < count; index++)
            result[index] = inputs.Sum(values => index < values.Count && !double.IsNaN(values[index]) ? values[index] : 0d);

        return result;
    }

    public static double AverageFinite(IEnumerable<double> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        var finiteValues = values.Where(double.IsFinite).ToArray();
        return finiteValues.Length > 0 ? finiteValues.Average() : double.NaN;
    }

    public static void AddInto(double[] target, IList<double> values)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(values);

        var count = Math.Min(target.Length, values.Count);
        for (var i = 0; i < count; i++)
        {
            var value = values[i];
            if (!double.IsFinite(value))
                continue;

            target[i] += value;
        }
    }
}
