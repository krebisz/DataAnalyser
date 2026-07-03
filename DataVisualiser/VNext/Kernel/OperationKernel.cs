using DataVisualiser.Core.Computation.TimeSeries;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.VNext.Kernel;

public sealed class OperationKernel
{
    public ChartSeriesProgram BuildSeries(AlignedSeriesBundle bundle, SeriesOperationRequest request)
    {
        ArgumentNullException.ThrowIfNull(bundle);
        ArgumentNullException.ThrowIfNull(request);

        var indexes = request.InputIndexes.Select(index => GetSeries(bundle, index)).ToArray();

        return request.Kind switch
        {
            SeriesOperationKind.Identity => CreateSeriesProgram(request, indexes[0].RawValues, indexes[0].SmoothedValues),
            SeriesOperationKind.Normalize => BuildNormalizedSeries(bundle, request),
            SeriesOperationKind.Logarithm => CreateSeriesProgram(request, ApplyUnary(indexes[0].RawValues, value => value <= 0d ? double.NaN : Math.Log(value)), ApplyUnary(indexes[0].SmoothedValues, value => value <= 0d ? double.NaN : Math.Log(value))),
            SeriesOperationKind.SquareRoot => CreateSeriesProgram(request, ApplyUnary(indexes[0].RawValues, value => value < 0d ? double.NaN : Math.Sqrt(value)), ApplyUnary(indexes[0].SmoothedValues, value => value < 0d ? double.NaN : Math.Sqrt(value))),
            SeriesOperationKind.Sum => CreateSeriesProgram(request, Sum(indexes.Select(series => series.RawValues), bundle.Timeline.Count), Sum(indexes.Select(series => series.SmoothedValues), bundle.Timeline.Count)),
            SeriesOperationKind.Difference => CreateSeriesProgram(request, ApplyBinary(indexes[0].RawValues, indexes[1].RawValues, (left, right) => left - right), ApplyBinary(indexes[0].SmoothedValues, indexes[1].SmoothedValues, (left, right) => left - right)),
            SeriesOperationKind.Ratio => CreateSeriesProgram(request, ApplyBinary(indexes[0].RawValues, indexes[1].RawValues, (left, right) => right == 0d ? double.NaN : left / right), ApplyBinary(indexes[0].SmoothedValues, indexes[1].SmoothedValues, (left, right) => right == 0d ? double.NaN : left / right)),
            SeriesOperationKind.MovingAverage => CreateSeriesProgram(request, ApplyMovingAverage(indexes[0].RawValues, request.WindowSize > 0 ? request.WindowSize : 7), ApplyMovingAverage(indexes[0].SmoothedValues, request.WindowSize > 0 ? request.WindowSize : 7)),
            _ => throw new InvalidOperationException($"Unsupported operation kind '{request.Kind}'.")
        };
    }

    public ChartSeriesProgram BuildSummedSeries(AlignedSeriesBundle bundle, string label = "Sum")
    {
        ArgumentNullException.ThrowIfNull(bundle);
        if (bundle.Series.Count == 0)
            return new ChartSeriesProgram("sum", label, Array.Empty<double>(), Array.Empty<double>());

        return BuildSeries(bundle, SeriesOperationRequest.Sum(Enumerable.Range(0, bundle.Series.Count).ToArray(), label));
    }

    public ChartSeriesProgram BuildDifferenceSeries(AlignedMetricSeries primary, AlignedMetricSeries secondary)
    {
        return CreateSeriesProgram(
            SeriesOperationRequest.Difference(0, 1, $"{primary.Request.DisplayName} - {secondary.Request.DisplayName}"),
            ApplyBinary(primary.RawValues, secondary.RawValues, (left, right) => left - right),
            ApplyBinary(primary.SmoothedValues, secondary.SmoothedValues, (left, right) => left - right));
    }

    public ChartSeriesProgram BuildRatioSeries(AlignedMetricSeries primary, AlignedMetricSeries secondary)
    {
        return CreateSeriesProgram(
            SeriesOperationRequest.Ratio(0, 1, $"{primary.Request.DisplayName} / {secondary.Request.DisplayName}"),
            ApplyBinary(primary.RawValues, secondary.RawValues, (left, right) => right == 0d ? double.NaN : left / right),
            ApplyBinary(primary.SmoothedValues, secondary.SmoothedValues, (left, right) => right == 0d ? double.NaN : left / right));
    }

    public ChartSeriesProgram BuildNormalizedSeries(AlignedMetricSeries series)
    {
        return CreateSeriesProgram(
            SeriesOperationRequest.Normalize(0, $"normalized::{series.Request.SignatureToken}", $"{series.Request.DisplayName} (normalized)"),
            Normalize(series.RawValues),
            Normalize(series.SmoothedValues));
    }

    private ChartSeriesProgram BuildNormalizedSeries(AlignedSeriesBundle bundle, SeriesOperationRequest request)
    {
        var target = GetSeries(bundle, request.InputIndexes[0]);
        if (request.NormalizationMode == null)
            return CreateSeriesProgram(request, Normalize(target.RawValues), Normalize(target.SmoothedValues));

        var referenceIndexes = request.NormalizationReferenceIndexes.Count == 0
            ? request.InputIndexes
            : request.NormalizationReferenceIndexes;
        var referenceIndexArray = referenceIndexes.ToArray();
        var referenceSeries = referenceIndexArray.Select(index => GetSeries(bundle, index)).ToArray();
        var targetReferenceIndex = Array.IndexOf(referenceIndexArray, request.InputIndexes[0]);
        if (targetReferenceIndex < 0)
            throw new InvalidOperationException("Normalization references must include the target input index.");

        var normalizedRaw = TimeSeriesNormalizationKernel.NormalizeSeries(
            referenceSeries.Select(series => series.RawValues).ToArray(),
            request.NormalizationMode.Value);
        var normalizedSmoothed = TimeSeriesNormalizationKernel.NormalizeSeries(
            referenceSeries.Select(series => series.SmoothedValues).ToArray(),
            request.NormalizationMode.Value);

        return CreateSeriesProgram(
            request,
            normalizedRaw[targetReferenceIndex],
            normalizedSmoothed[targetReferenceIndex]);
    }

    private static AlignedMetricSeries GetSeries(AlignedSeriesBundle bundle, int index)
    {
        if (index < 0 || index >= bundle.Series.Count)
            throw new ArgumentOutOfRangeException(nameof(index), $"Series index {index} is outside the aligned bundle.");

        return bundle.Series[index];
    }

    private static ChartSeriesProgram CreateSeriesProgram(SeriesOperationRequest request, IReadOnlyList<double> rawValues, IReadOnlyList<double> smoothedValues)
    {
        return new ChartSeriesProgram(request.Id, request.Label, rawValues, smoothedValues);
    }

    private static IReadOnlyList<double> Sum(IEnumerable<IReadOnlyList<double>> seriesValues, int timelineCount)
    {
        var inputs = seriesValues.ToArray();
        var result = new double[timelineCount];
        for (var index = 0; index < timelineCount; index++)
            result[index] = inputs.Sum(values => double.IsNaN(values[index]) ? 0d : values[index]);

        return result;
    }

    private static IReadOnlyList<double> ApplyBinary(
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

        return result;
    }

    private static IReadOnlyList<double> ApplyUnary(
        IReadOnlyList<double> values,
        Func<double, double> operation)
    {
        var result = new double[values.Count];
        for (var index = 0; index < values.Count; index++)
        {
            var value = values[index];
            result[index] = double.IsNaN(value) ? double.NaN : operation(value);
        }

        return result;
    }

    private static IReadOnlyList<double> ApplyMovingAverage(IReadOnlyList<double> values, int windowSize)
    {
        var result = new double[values.Count];
        for (var i = 0; i < values.Count; i++)
        {
            var start = Math.Max(0, i - windowSize + 1);
            var sum = 0d;
            var count = 0;
            for (var j = start; j <= i; j++)
            {
                if (!double.IsNaN(values[j]))
                {
                    sum += values[j];
                    count++;
                }
            }
            result[i] = count == 0 ? double.NaN : sum / count;
        }
        return result;
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
