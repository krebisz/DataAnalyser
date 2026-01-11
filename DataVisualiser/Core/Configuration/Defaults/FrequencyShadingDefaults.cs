using System.Windows.Media;

namespace DataVisualiser.Core.Configuration.Defaults;

/// <summary>
///     Shared defaults for frequency shading gradients and colors.
/// </summary>
public static class FrequencyShadingDefaults
{
    public const byte StartR = 173;
    public const byte StartG = 216;
    public const byte StartB = 230;

    public const byte EndR = 8;
    public const byte EndG = 10;
    public const byte EndB = 25;

    public static readonly Color FallbackColor = Color.FromRgb(StartR, StartG, StartB);
}
