using System.Windows.Media;
using DataVisualiser.Helper;
using DataVisualiser.Models;
using LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.Services;

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

    /// <summary>
    ///     Step 1 & 2: Normalize y-values and create bins with frequency counts per day.
    ///     Returns a tuple with bins and frequency data.
    /// </summary>
    public static (List<(double Min, double Max)> Bins, double BinSize, Dictionary<int, Dictionary<int, int>> FrequenciesPerDay, Dictionary<int, Dictionary<int, double>> NormalizedFrequenciesPerDay) PrepareBinsAndFrequencies(Dictionary<int, List<double>> dayValues, // dayIndex -> values for that day
        double globalMin, double globalMax)
    {
        // Step 1: Calculate bin size based on range
        var binSize = FrequencyBinningHelper.CalculateBinSize(globalMin, globalMax);

        // Step 2: Create bins
        var bins = FrequencyBinningHelper.CreateBins(globalMin, globalMax, binSize);

        // Step 2: Bin values and count frequencies for each day
        var frequenciesPerDay = new Dictionary<int, Dictionary<int, int>>();
        for (var dayIndex = 0; dayIndex < 7; dayIndex++)
        {
            var values = dayValues.TryGetValue(dayIndex, out var v) ? v : new List<double>();
            var dayFrequencies = FrequencyBinningHelper.BinValuesAndCountFrequencies(values, bins);
            frequenciesPerDay[dayIndex] = dayFrequencies;
        }

        // Step 3: Normalize frequencies across all days
        var normalizedFrequenciesPerDay = FrequencyBinningHelper.NormalizeFrequencies(frequenciesPerDay);

        return (bins, binSize, frequenciesPerDay, normalizedFrequenciesPerDay);
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
    ///     Step 4 & 5: Assign color shade to each y-interval for each day and draw the chart.
    ///     Creates stacked column series where each bin is a segment, colored by frequency.
    /// </summary>
    public static void RenderChart(CartesianChart targetChart, WeeklyDistributionResult result, double minHeight)
    {
        if (result?.Bins == null || result.Bins.Count == 0)
            return;

        var seriesCollection = new SeriesCollection();

        // Track cumulative baseline per day (Mon=0 .. Sun=6)
        var cumulativeBaseline = new double[7];
        Array.Fill(cumulativeBaseline, 0.0);

        // For each bin (interval on Y axis)
        for (var binIndex = 0; binIndex < result.Bins.Count; binIndex++)
        {
            var bin = result.Bins[binIndex];
            var binHeight = bin.Max - bin.Min;

            // For each day, compute baseline, height, and color
            for (var dayIndex = 0; dayIndex < 7; dayIndex++)
            {
                // Lookup normalized frequency
                var normalizedFreq = 0.0;
                if (result.NormalizedFrequenciesPerDay.TryGetValue(dayIndex, out var dayFreqs))
                    dayFreqs.TryGetValue(binIndex, out normalizedFreq);

                // Skip empty segments
                if (normalizedFreq <= 0.0)
                    continue;

                // Determine color for this bin/day
                var binColor = MapFrequencyToColor(normalizedFreq);

                var baselineValue = cumulativeBaseline[dayIndex];

                // Build baseline values (only active day has baseline)
                var baselineValues = new ChartValues<double>();
                var heightValues = new ChartValues<double>();
                for (var d = 0; d < 7; d++)
                {
                    baselineValues.Add(d == dayIndex ? baselineValue : 0.0);
                    heightValues.Add(d == dayIndex ? binHeight : 0.0);
                }

                // Transparent baseline series
                var baselineSeries = new StackedColumnSeries
                {
                    Title = null,
                    Values = baselineValues,
                    Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
                    StrokeThickness = 0,
                    MaxColumnWidth = MaxColumnWidth,
                    DataLabels = false
                };

                // Colored bin segment
                var fillBrush = new SolidColorBrush(binColor);
                fillBrush.Freeze();

                var strokeBrush = new SolidColorBrush(binColor);
                strokeBrush.Freeze();

                var binSeries = new StackedColumnSeries
                {
                    Title = null,
                    Values = heightValues,
                    Fill = fillBrush,
                    Stroke = strokeBrush,
                    StrokeThickness = 0.5,
                    MaxColumnWidth = MaxColumnWidth,
                    DataLabels = false
                };

                seriesCollection.Add(baselineSeries);
                seriesCollection.Add(binSeries);

                // Advance baseline for this day
                cumulativeBaseline[dayIndex] += binHeight;
            }
        }

        // Apply to chart
        targetChart.Series = seriesCollection;
        targetChart.LegendLocation = LegendLocation.None;
    }
}