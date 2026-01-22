using DataFileReader.Canonical;

namespace DataVisualiser.Tests.Helpers;

internal class MockCanonicalMetricSeries : ICanonicalMetricSeries
{
    public CanonicalMetricId MetricId { get; init; } = default!;
    public TimeSemantics Time { get; init; } = default!;
    public IReadOnlyList<MetricSample> Samples { get; init; } = Array.Empty<MetricSample>();
    public MetricUnit Unit { get; init; } = default!;
    public MetricDimension Dimension { get; init; } = MetricDimension.Unknown;
    public MetricProvenance Provenance { get; init; } = new("Test", "Test", "1.0");
    public MetricQuality Quality { get; init; } = new(DataCompleteness.Complete, ValidationStatus.Validated);
}