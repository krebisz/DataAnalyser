using System.Windows.Media;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Rendering.Shading;

public interface IFrequencyShadingRenderer
{
    void Render(CartesianChart targetChart, List<double> mins, List<double> ranges, List<(double Min, double Max)> intervals, Dictionary<int, Dictionary<int, int>> frequenciesPerBucket, Dictionary<int, Dictionary<int, Color>> colorMap, double globalMin, double globalMax, IntervalShadingContext shadingContext);
}