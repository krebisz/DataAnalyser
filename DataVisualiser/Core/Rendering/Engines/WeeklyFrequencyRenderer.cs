using System.Windows.Media;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;
using LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Rendering.Engines;

/// <summary>
///     Separated concerns for rendering weekly frequency distribution:
///     1. Normalize y-values to create intervals (bins)
///     2. Binning - assign values to bins
///     3. Dynamic shading range - map frequencies to colors
///     4. Assign color shade to each y-interval for each day
///     5. Final drawing
/// </summary>
public class WeeklyFrequencyRenderer
{
    private const double MaxColumnWidth = 40.0;
    private const int    BucketCount    = 7;

    /// <summary>
    ///     Step 1 & 2: Normalize y-values and create bins with frequency counts per bucket.
    ///     Returns a tuple with bins and frequency data.
    /// </summary>
    public static(List<(double Min, double Max)> Bins, double BinSize, Dictionary<int, Dictionary<int, int>> FrequenciesPerBucket, Dictionary<int, Dictionary<int, double>> NormalizedFrequenciesPerBucket) PrepareBinsAndFrequencies(Dictionary<int, List<double>> bucketValues, // bucketIndex -> values for that bucket
                                                                                                                                                                                                                                      double                        globalMin,    double globalMax)
    {
        // Step 1: Calculate bin size based on range
        var binSize = FrequencyBinningHelper.CalculateBinSize(globalMin, globalMax);

        // Step 2: Create bins
        var bins = FrequencyBinningHelper.CreateBins(globalMin, globalMax, binSize);

        // Step 2: Bin values and count frequencies for each bucket
        var frequenciesPerBucket = new Dictionary<int, Dictionary<int, int>>();
        for (var bucketIndex = 0; bucketIndex < BucketCount; bucketIndex++)
        {
            var values = bucketValues.TryGetValue(bucketIndex, out var v) ? v : new List<double>();
            var bucketFrequencies = FrequencyBinningHelper.BinValuesAndCountFrequencies(values, bins);
            frequenciesPerBucket[bucketIndex] = bucketFrequencies;
        }

        // Step 3: Normalize frequencies across all buckets
        var normalizedFrequenciesPerBucket = FrequencyBinningHelper.NormalizeFrequencies(frequenciesPerBucket);

        return (bins, binSize, frequenciesPerBucket, normalizedFrequenciesPerBucket);
    }

    /// <summary>
    ///     Step 3: Dynamic shading range - maps normalized frequency [0.0, 1.0] to color.
    ///     Higher frequency = darker color (closer to black).
    /// </summary>
    public static Color MapFrequencyToColor(double normalizedFrequency)
    {
        // Clamp to [0.0, 1.0]
        normalizedFrequency = Math.Max(0.0, Math.Min(1.0, normalizedFrequency));

        // Start color: light blue (when frequency = 0)
        byte r0 = 173, g0 = 216, b0 = 230;

        // End color: near-black/dark blue (when frequency = 1.0)
        byte r1 = 8, g1 = 10, b1 = 25;

        // Interpolate based on normalized frequency
        var r = (byte)Math.Round(r0 + (r1 - r0) * normalizedFrequency);
        var g = (byte)Math.Round(g0 + (g1 - g0) * normalizedFrequency);
        var b = (byte)Math.Round(b0 + (b1 - b0) * normalizedFrequency);

        return Color.FromRgb(r, g, b);
    }

    /// <summary>
    ///     Step 4 & 5: Assign color shade to each y-interval for each bucket and draw the chart.
    ///     Creates stacked column series where each bin is a segment, colored by frequency.
    /// </summary>
    public static void RenderChart(CartesianChart targetChart, WeeklyDistributionResult result, double minHeight)
    {
        if (result?.Bins == null || result.Bins.Count == 0)
            return;

        var seriesCollection = new SeriesCollection();

        var cumulativeBaseline = new double[BucketCount];
        Array.Fill(cumulativeBaseline, 0.0);

        for (var binIndex = 0; binIndex < result.Bins.Count; binIndex++)
            RenderBin(seriesCollection, result, binIndex, cumulativeBaseline);

        targetChart.Series = seriesCollection;
        targetChart.LegendLocation = LegendLocation.None;
    }

    private static void RenderBin(SeriesCollection seriesCollection, WeeklyDistributionResult result, int binIndex, double[] cumulativeBaseline)
    {
        var bin = result.Bins[binIndex];
        var binHeight = bin.Max - bin.Min;

        for (var bucketIndex = 0; bucketIndex < BucketCount; bucketIndex++)
            RenderBinForBucket(seriesCollection, result, binIndex, bucketIndex, binHeight, cumulativeBaseline);
    }

    private static void RenderBinForBucket(SeriesCollection seriesCollection, WeeklyDistributionResult result, int binIndex, int bucketIndex, double binHeight, double[] cumulativeBaseline)
    {
        if (!TryGetNormalizedFrequency(result, bucketIndex, binIndex, out var normalizedFreq) || normalizedFreq <= 0.0)
            return;

        var color = MapFrequencyToColor(normalizedFreq);
        var baseline = cumulativeBaseline[bucketIndex];

        var baselineValues = BuildValues(bucketIndex, baseline);
        var heightValues = BuildValues(bucketIndex, binHeight);

        seriesCollection.Add(CreateBaselineSeries(baselineValues));
        seriesCollection.Add(CreateColoredSeries(heightValues, color));

        cumulativeBaseline[bucketIndex] += binHeight;
    }

    private static bool TryGetNormalizedFrequency(WeeklyDistributionResult result, int bucketIndex, int binIndex, out double normalizedFreq)
    {
        normalizedFreq = 0.0;

        return result.NormalizedFrequenciesPerBucket.TryGetValue(bucketIndex, out var bucketFreqs) && bucketFreqs.TryGetValue(binIndex, out normalizedFreq);
    }

    private static ChartValues<double> BuildValues(int activeBucketIndex, double value)
    {
        var values = new ChartValues<double>();

        for (var b = 0; b < BucketCount; b++)
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
                MaxColumnWidth = MaxColumnWidth,
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
                MaxColumnWidth = MaxColumnWidth,
                DataLabels = false
        };
    }
}