using DataVisualiser.Shared.Helpers;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Rendering.Helpers;

internal static class ChartTooltipValueFormatter
{
    public static string FormatSeriesValue(Series series, int index)
    {
        if (series.Values == null || index < 0 || index >= series.Values.Count)
            return "N/A";

        try
        {
            var raw = series.Values[index];
            return raw == null ? "N/A" : MathHelper.FormatDisplayedValue(Convert.ToDouble(raw));
        }
        catch
        {
            return "N/A";
        }
    }

    public static bool TryExtractNumericValue(Series series, int index, out double value)
    {
        value = double.NaN;

        if (series.Values == null || index < 0 || index >= series.Values.Count)
            return false;

        try
        {
            var raw = series.Values[index];
            if (raw == null)
                return false;

            value = Convert.ToDouble(raw);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool TryGetValue(IList<double> values, int index, out double value)
    {
        value = double.NaN;
        if (values == null || index < 0 || index >= values.Count)
            return false;

        value = values[index];
        return !double.IsNaN(value) && !double.IsInfinity(value);
    }
}
