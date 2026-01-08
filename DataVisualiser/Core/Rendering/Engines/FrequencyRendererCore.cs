using System.Windows.Media;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;
using LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Rendering.Engines;

internal static class FrequencyRendererCore
{
    public static(List<(double Min, double Max)> Bins, double BinSize, Dictionary<int, Dictionary<int, int>> FrequenciesPerBucket, Dictionary<int, Dictionary<int, double>> NormalizedFrequenciesPerbucket) PrepareBinsAndFrequencies(Dictionary<int, List<double>> bucketValues, double globalMin, double globalMax, int bucketCount)
    {
        // Step 1: Calculate bin size based on range
        var binSize = FrequencyBinningHelper.CalculateBinSize(globalMin, globalMax);

        // Step 2: Create bins
        var bins = FrequencyBinningHelper.CreateBins(globalMin, globalMax, binSize);

        // Step 2: Bin values and count frequencies for each bucket
        var frequenciesPerBucket = new Dictionary<int, Dictionary<int, int>>();
        for (var bucketIndex = 0; bucketIndex < bucketCount; bucketIndex++)
        {
            var values = bucketValues.TryGetValue(bucketIndex, out var v) ? v : new List<double>();
            var bucketFrequencies = FrequencyBinningHelper.BinValuesAndCountFrequencies(values, bins);
            frequenciesPerBucket[bucketIndex] = bucketFrequencies;
        }

        // Step 3: Normalize frequencies across all buckets
        var normalizedFrequenciesPerBucket = FrequencyBinningHelper.NormalizeFrequencies(frequenciesPerBucket);

        return (bins, binSize, frequenciesPerBucket, normalizedFrequenciesPerBucket);
    }

    public static Color MapFrequencyToColor(double normalizedFrequency)
    {
        // Clamp to [0.0, 1.0]
        normalizedFrequency = Math.Max(0.0, Math.Min(1.0, normalizedFrequency));

        // Start color: light blue (when frequency = 0)
        byte r0 = FrequencyShadingDefaults.StartR, g0 = FrequencyShadingDefaults.StartG, b0 = FrequencyShadingDefaults.StartB;

        // End color: near-black/dark blue (when frequency = 1.0)
        byte r1 = FrequencyShadingDefaults.EndR, g1 = FrequencyShadingDefaults.EndG, b1 = FrequencyShadingDefaults.EndB;

        // Interpolate based on normalized frequency
        var r = (byte)Math.Round(r0 + (r1 - r0) * normalizedFrequency);
        var g = (byte)Math.Round(g0 + (g1 - g0) * normalizedFrequency);
        var b = (byte)Math.Round(b0 + (b1 - b0) * normalizedFrequency);

        return Color.FromRgb(r, g, b);
    }

    public static void RenderChart(CartesianChart targetChart, BucketDistributionResult result, double minHeight, int bucketCount)
    {
        if (result?.Bins == null || result.Bins.Count == 0)
            return;

        var seriesCollection = new SeriesCollection();

        var cumulativeBaseline = new double[bucketCount];
        Array.Fill(cumulativeBaseline, 0.0);

        for (var binIndex = 0; binIndex < result.Bins.Count; binIndex++)
            RenderBin(seriesCollection, result, binIndex, cumulativeBaseline, bucketCount);

        targetChart.Series = seriesCollection;
        targetChart.LegendLocation = LegendLocation.None;
    }

    private static void RenderBin(SeriesCollection seriesCollection, BucketDistributionResult result, int binIndex, double[] cumulativeBaseline, int bucketCount)
    {
        var bin = result.Bins[binIndex];
        var binHeight = bin.Max - bin.Min;

        for (var bucketIndex = 0; bucketIndex < bucketCount; bucketIndex++)
            RenderBinForBucket(seriesCollection, result, binIndex, bucketIndex, binHeight, cumulativeBaseline, bucketCount);
    }

    private static void RenderBinForBucket(SeriesCollection seriesCollection, BucketDistributionResult result, int binIndex, int bucketIndex, double binHeight, double[] cumulativeBaseline, int bucketCount)
    {
        if (!TryGetNormalizedFrequency(result, bucketIndex, binIndex, out var normalizedFreq) || normalizedFreq <= 0.0)
            return;

        var color = MapFrequencyToColor(normalizedFreq);
        var baseline = cumulativeBaseline[bucketIndex];

        var baselineValues = BuildValues(bucketIndex, baseline, bucketCount);
        var heightValues = BuildValues(bucketIndex, binHeight, bucketCount);

        seriesCollection.Add(CreateBaselineSeries(baselineValues));
        seriesCollection.Add(CreateColoredSeries(heightValues, color));

        cumulativeBaseline[bucketIndex] += binHeight;
    }

    private static bool TryGetNormalizedFrequency(BucketDistributionResult result, int bucketIndex, int binIndex, out double normalizedFreq)
    {
        normalizedFreq = 0.0;

        return result.NormalizedFrequenciesPerBucket.TryGetValue(bucketIndex, out var bucketFreqs) && bucketFreqs.TryGetValue(binIndex, out normalizedFreq);
    }

    private static ChartValues<double> BuildValues(int activeBucketIndex, double value, int bucketCount)
    {
        var values = new ChartValues<double>();

        for (var b = 0; b < bucketCount; b++)
            values.Add(b == activeBucketIndex ? value : 0.0);

        return values;
    }

    private static StackedColumnSeries CreateBaselineSeries(ChartValues<double> baselineValues)
    {
        return new StackedColumnSeries
        {
                Title = null,
                Values = baselineValues,
                Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
                StrokeThickness = 0,
                MaxColumnWidth = RenderingDefaults.MaxColumnWidth,
                DataLabels = false
        };
    }

    private static StackedColumnSeries CreateColoredSeries(ChartValues<double> heightValues, Color color)
    {
        var fillBrush = new SolidColorBrush(color);
        fillBrush.Freeze();

        var strokeBrush = new SolidColorBrush(color);
        strokeBrush.Freeze();

        return new StackedColumnSeries
        {
                Title = null,
                Values = heightValues,
                Fill = fillBrush,
                Stroke = strokeBrush,
                StrokeThickness = 0.5,
                MaxColumnWidth = RenderingDefaults.MaxColumnWidth,
                DataLabels = false
        };
    }
}