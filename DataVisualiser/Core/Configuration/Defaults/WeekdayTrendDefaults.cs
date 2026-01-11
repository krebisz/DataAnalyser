using System.Windows.Media;

namespace DataVisualiser.Core.Configuration.Defaults;

/// <summary>
///     Centralized defaults for weekday trend rendering.
/// </summary>
public static class WeekdayTrendDefaults
{
    public const int BucketCount = 7;

    public static readonly Brush[] WeekdayStrokes =
    {
            Brushes.SteelBlue,
            Brushes.CadetBlue,
            Brushes.SeaGreen,
            Brushes.OliveDrab,
            Brushes.Goldenrod,
            Brushes.OrangeRed,
            Brushes.IndianRed
    };
}
