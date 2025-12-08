using DataVisualiser.Models;

namespace DataVisualiser.Charts.Computation
{
    public sealed class ChartComputationResult
    {
        public List<DateTime> Timestamps { get; init; } = new();
        public List<int> IntervalIndices { get; init; } = new();
        public List<DateTime> NormalizedIntervals { get; init; } = new();
        public List<double> PrimaryRawValues { get; init; } = new();
        public List<double>? SecondaryRawValues { get; init; }
        public List<double> PrimarySmoothed { get; init; } = new();
        public List<double>? SecondarySmoothed { get; init; }
        public TickInterval TickInterval { get; init; }
        public TimeSpan DateRange { get; init; }
        public string? Unit { get; init; }
    }
}

