using DataVisualiser.Services.Shading;
using LiveCharts.Wpf;
using System.Windows.Media;

namespace DataVisualiser.Charts.Rendering
{
    public interface IFrequencyShadingRenderer
    {
        void Render(
            CartesianChart targetChart,
            List<double> mins,
            List<double> ranges,
            List<(double Min, double Max)> intervals,
            Dictionary<int, Dictionary<int, int>> frequenciesPerDay,
            Dictionary<int, Dictionary<int, Color>> colorMap,
            double globalMin,
            double globalMax,
            IntervalShadingContext shadingContext);
    }

}
