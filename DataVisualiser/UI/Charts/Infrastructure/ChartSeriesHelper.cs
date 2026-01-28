using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.Charts.Adapters;
using System.Collections;

namespace DataVisualiser.UI.Charts.Infrastructure;

public static class ChartSeriesHelper
{
    public static bool HasSeries(IEnumerable? series)
    {
        if (series == null)
            return false;

        return series.Cast<object>().Any();
    }
}
