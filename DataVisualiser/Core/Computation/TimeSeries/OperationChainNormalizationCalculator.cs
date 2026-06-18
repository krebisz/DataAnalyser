using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Kernel;

namespace DataVisualiser.Core.Computation.TimeSeries;

public sealed record OperationChainNormalizedSeries(
    MetricSeriesRequest Request,
    string Id,
    string Label,
    IReadOnlyList<double> RawValues,
    IReadOnlyList<double> SmoothedValues);

public static class OperationChainNormalizationCalculator
{
    public static IReadOnlyList<OperationChainNormalizedSeries> Compute(
        AlignedSeriesBundle bundle,
        NormalizationMode mode)
    {
        ArgumentNullException.ThrowIfNull(bundle);

        if (bundle.Series.Count == 0)
            throw new ArgumentException("Normalization requires at least one input series.", nameof(bundle));

        return mode == NormalizationMode.RelativeToMax
            ? ComputeRelativeToSharedMax(bundle)
            : ComputeIndependentNormalization(bundle, mode);
    }

    private static IReadOnlyList<OperationChainNormalizedSeries> ComputeIndependentNormalization(
        AlignedSeriesBundle bundle,
        NormalizationMode mode)
    {
        return bundle.Series
            .Select((series, index) => new OperationChainNormalizedSeries(
                series.Request,
                BuildId(mode, index, series.Request),
                BuildLabel(mode, series.Request),
                MathHelper.ReturnValueNormalized(series.RawValues.ToList(), mode) ?? Empty(series.RawValues.Count),
                MathHelper.ReturnValueNormalized(series.SmoothedValues.ToList(), mode) ?? Empty(series.SmoothedValues.Count)))
            .ToArray();
    }

    private static IReadOnlyList<OperationChainNormalizedSeries> ComputeRelativeToSharedMax(AlignedSeriesBundle bundle)
    {
        var sharedRawMax = ResolveSharedMax(bundle.Series.Select(series => series.RawValues));
        var sharedSmoothedMax = ResolveSharedMax(bundle.Series.Select(series => series.SmoothedValues));

        return bundle.Series
            .Select((series, index) => new OperationChainNormalizedSeries(
                series.Request,
                BuildId(NormalizationMode.RelativeToMax, index, series.Request),
                BuildLabel(NormalizationMode.RelativeToMax, series.Request),
                NormalizeAgainstSharedMax(series.RawValues, sharedRawMax),
                NormalizeAgainstSharedMax(series.SmoothedValues, sharedSmoothedMax)))
            .ToArray();
    }

    private static string BuildId(NormalizationMode mode, int index, MetricSeriesRequest request) =>
        $"operation-chain-normalized::{mode}::{index}::{request.SignatureToken}";

    private static string BuildLabel(NormalizationMode mode, MetricSeriesRequest request)
    {
        return mode switch
        {
            NormalizationMode.ZeroToOne => $"{request.DisplayName} (Zero-To-One)",
            NormalizationMode.PercentageOfMax => $"{request.DisplayName} (% of Max)",
            NormalizationMode.RelativeToMax => $"{request.DisplayName} (Relative to Max)",
            _ => request.DisplayName
        };
    }

    private static double ResolveSharedMax(IEnumerable<IReadOnlyList<double>> seriesValues)
    {
        var values = seriesValues
            .SelectMany(values => values)
            .Where(value => !double.IsNaN(value))
            .ToList();

        return values.Count == 0 ? double.NaN : values.Max();
    }

    private static IReadOnlyList<double> NormalizeAgainstSharedMax(IReadOnlyList<double> values, double sharedMax)
    {
        if (double.IsNaN(sharedMax))
            return values.Select(_ => double.NaN).ToList();

        if (sharedMax == 0d)
            return values.Select(value => double.IsNaN(value) ? double.NaN : 0d).ToList();

        return values.Select(value => double.IsNaN(value) ? double.NaN : value / sharedMax * 100d).ToList();
    }

    private static IReadOnlyList<double> Empty(int count) =>
        Enumerable.Repeat(double.NaN, count).ToArray();
}
