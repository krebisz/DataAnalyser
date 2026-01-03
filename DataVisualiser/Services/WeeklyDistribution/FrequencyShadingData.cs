using System.Windows.Media;

namespace DataVisualiser.Services.WeeklyDistribution;

/// <summary>
///     Data structure for frequency shading calculations.
/// </summary>
public sealed record FrequencyShadingData(List<(double Min, double Max)> Intervals, Dictionary<int, Dictionary<int, int>> FrequenciesPerDay, Dictionary<int, Dictionary<int, Color>> ColorMap, Dictionary<int, List<double>> DayValues)
{
    public static readonly FrequencyShadingData Empty = new(new List<(double Min, double Max)>(), new Dictionary<int, Dictionary<int, int>>(), new Dictionary<int, Dictionary<int, Color>>(), new Dictionary<int, List<double>>());
}