using System.Windows.Media;

namespace DataVisualiser.Core.Rendering.Helpers;

public static class ColourPalette
{
    private static readonly List<Color> _colors = new()
    {
        Colors.SlateBlue,
        Colors.MediumSeaGreen,
        Colors.IndianRed,
        Colors.DarkOrange,
        Colors.MediumPurple,
        Colors.CadetBlue,
        Colors.Goldenrod,
        Colors.Teal,
        Colors.Lavender,
        Colors.Firebrick,
        Colors.Brown,
        Colors.SteelBlue
    };

    private static readonly Dictionary<object, int> _chartColorIndex = new();

    public static Color Next(object chart)
    {
        if (!_chartColorIndex.ContainsKey(chart))
            _chartColorIndex[chart] = 0;

        var index = _chartColorIndex[chart];
        var color = _colors[index];

        _chartColorIndex[chart] = (index + 1) % _colors.Count;
        return color;
    }

    public static void Reset(object chart)
    {
        if (_chartColorIndex.ContainsKey(chart))
            _chartColorIndex[chart] = 0;
    }
}
