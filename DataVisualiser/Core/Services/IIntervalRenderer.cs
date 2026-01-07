using System.Windows.Media;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Services;

/// <summary>
///     Interface for interval renderers (weekly, hourly, etc.)
/// </summary>
public interface IIntervalRenderer
{
    int RenderIntervals(CartesianChart chart, List<double> mins, List<double> ranges, List<(double Min, double Max)> intervals, Dictionary<int, Dictionary<int, int>> frequenciesPerBucket, Dictionary<int, Dictionary<int, Color>> colorMap, double uniformIntervalHeight, double[] cumulativeStackHeight, int globalMaxFreq);
}