using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Rendering.Helpers;

public static class ChartLabelFormatter
{
    public static string FormatDateTimeLabel(DateTime dateTime, TickInterval interval)
    {
        return interval switch
        {
                TickInterval.Month => dateTime.ToString("MMM yyyy"),
                TickInterval.Week => dateTime.ToString("MMM dd"),
                TickInterval.Day => dateTime.ToString("MM/dd"),
                TickInterval.Hour => dateTime.ToString("MM/dd HH:mm"),
                _ => dateTime.ToString("MM/dd HH:mm")
        };
    }
}
