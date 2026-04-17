namespace DataVisualiser.Core.Rendering.Helpers;

internal static class ChartTooltipSeriesTitleParser
{
    public static (string BaseName, bool IsRaw, bool IsSmoothed) Parse(string title)
    {
        if (title.EndsWith(" (Raw)") || title.EndsWith(" (raw)"))
            return (title[..^6], true, false);

        if (title.EndsWith(" (smooth)"))
            return (title[..^9], false, true);

        return (title, false, false);
    }
}
