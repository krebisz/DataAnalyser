using System.Windows.Media;
using DataVisualiser.Shared.Models;
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
    private const int BucketCount = 7;

    /// <summary>
    ///     Step 1 & 2: Normalize y-values and create bins with frequency counts per bucket.
    ///     Returns a tuple with bins and frequency data.
    /// </summary>
    public static (List<(double Min, double Max)> Bins, double BinSize, Dictionary<int, Dictionary<int, int>> FrequenciesPerBucket, Dictionary<int, Dictionary<int, double>> NormalizedFrequenciesPerbucket) PrepareBinsAndFrequencies(Dictionary<int, List<double>> bucketValues, double globalMin, double globalMax)
    {
        return FrequencyRendererCore.PrepareBinsAndFrequencies(bucketValues, globalMin, globalMax, BucketCount);
    }

    /// <summary>
    ///     Step 3: Dynamic shading range - maps normalized frequency [0.0, 1.0] to color.
    ///     Higher frequency = darker color (closer to black).
    /// </summary>
    public static Color MapFrequencyToColor(double normalizedFrequency)
    {
        return FrequencyRendererCore.MapFrequencyToColor(normalizedFrequency);
    }

    /// <summary>
    ///     Step 4 & 5: Assign color shade to each y-interval for each bucket and draw the chart.
    ///     Creates stacked column series where each bin is a segment, colored by frequency.
    /// </summary>
    public static void RenderChart(CartesianChart targetChart, BucketDistributionResult result, double minHeight)
    {
        FrequencyRendererCore.RenderChart(targetChart, result, minHeight, BucketCount);
    }
}
