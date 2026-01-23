using System.Collections;
using System.Linq;

namespace DataVisualiser.UI.Controls;

public static class ChartSeriesHelper
{
    public static bool HasSeries(IEnumerable? series)
    {
        if (series == null)
            return false;

        return series.Cast<object>().Any();
    }
}
