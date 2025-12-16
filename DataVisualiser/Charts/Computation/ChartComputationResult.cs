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

        // NEW: Multi-series support for Main Chart (additive; existing fields remain authoritative for 1–2 series paths)
        public List<SeriesResult>? Series { get; init; }
    }

    public sealed class SeriesResult
    {
        public string SeriesId { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;

        public List<DateTime> Timestamps { get; init; } = new();
        public List<double> RawValues { get; init; } = new();
        public List<double>? Smoothed { get; init; }
    }
}
