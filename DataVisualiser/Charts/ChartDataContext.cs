using DataVisualiser.Models;

namespace DataVisualiser.Charts
{
    public class ChartDataContext
    {
        // Raw input from DB
        public IReadOnlyList<HealthMetricData>? Data1 { get; init; }
        public IReadOnlyList<HealthMetricData>? Data2 { get; init; }

        // Unified timeline
        public IReadOnlyList<DateTime>? Timestamps { get; init; }

        // Numeric series (aligned)
        public IReadOnlyList<double>? RawValues1 { get; init; }
        public IReadOnlyList<double>? RawValues2 { get; init; }

        // Smoothed numeric series
        public IReadOnlyList<double>? SmoothedValues1 { get; init; }
        public IReadOnlyList<double>? SmoothedValues2 { get; init; }

        // Derived / computed series
        public IReadOnlyList<double>? DifferenceValues { get; init; }
        public IReadOnlyList<double>? RatioValues { get; init; }
        public IReadOnlyList<double>? NormalizedValues1 { get; init; }
        public IReadOnlyList<double>? NormalizedValues2 { get; init; }

        // Metadata
        public string DisplayName1 { get; init; } = string.Empty;
        public string DisplayName2 { get; init; } = string.Empty;

        public DateTime From { get; init; }
        public DateTime To { get; init; }
    }
}
