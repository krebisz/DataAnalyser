using DataVisualiser.Core.Computation.TimeSeries;

namespace DataVisualiser.Core.Rendering.Helpers;

public static class SeriesAlignmentHelper
{
    public static List<double> AlignSeriesToTimeline(List<DateTime> seriesTimestamps, List<double> seriesValues, List<DateTime> mainTimeline)
    {
        return TimeSeriesAlignment.AlignSeriesToTimeline(seriesTimestamps, seriesValues, mainTimeline);
    }
}
