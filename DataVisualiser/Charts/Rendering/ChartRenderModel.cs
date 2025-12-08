using DataVisualiser.Models;
using System.Windows.Media;

namespace DataVisualiser.Charts.Rendering
{
    public sealed class ChartRenderModel
    {
        public string PrimarySeriesName { get; init; } = "Primary";
        public string SecondarySeriesName { get; init; } = "Secondary";
        public IList<double> PrimaryRaw { get; init; } = Array.Empty<double>();
        public IList<double>? SecondaryRaw { get; init; }
        public IList<double> PrimarySmoothed { get; init; } = Array.Empty<double>();
        public IList<double>? SecondarySmoothed { get; init; }
        public Color PrimaryColor { get; init; } = Colors.DarkGray;
        public Color SecondaryColor { get; init; } = Colors.Red;
        public string? Unit { get; init; }
        public List<DateTime> Timestamps { get; init; } = new();
        public List<int> IntervalIndices { get; init; } = new();
        public List<DateTime> NormalizedIntervals { get; init; } = new();
        public TickInterval TickInterval { get; init; }
    }
}

