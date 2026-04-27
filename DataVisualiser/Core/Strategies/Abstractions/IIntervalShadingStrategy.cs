using System.Windows.Media;

namespace DataVisualiser.Core.Strategies.Abstractions;

public interface IIntervalShadingStrategy
{
    Dictionary<int, Dictionary<int, Color>> CalculateColorMap(IntervalShadingContext context);

    Color? CalculateIntervalColor(IntervalShadingContext context, int bucketIndex, int intervalIndex, int intervalMaxFrequency, int globalMaxFrequency);
}
