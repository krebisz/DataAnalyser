using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Orchestration;

public class ChartDataContext
{
    public object? PrimaryCms { get; set; }
    public object? SecondaryCms { get; set; }

    // Raw input from DB
    public IReadOnlyList<MetricData>? Data1 { get; init; }
    public IReadOnlyList<MetricData>? Data2 { get; init; }

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
    public int SemanticMetricCount { get; init; }

    // Raw metric information for label formatting
    public string? MetricType { get; init; }
    public string? PrimaryMetricType { get; init; }
    public string? SecondaryMetricType { get; init; }
    public string? PrimarySubtype { get; init; }
    public string? SecondarySubtype { get; init; }

    public DateTime From { get; init; }
    public DateTime To { get; init; }
}