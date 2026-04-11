using DataFileReader.Canonical;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.VNext.Contracts;

public sealed record MetricSeriesSnapshot(
    MetricSeriesRequest Request,
    IReadOnlyList<MetricData> RawData,
    ICanonicalMetricSeries? CanonicalSeries)
{
    public bool HasRawData => RawData.Count > 0;
    public bool HasCanonicalData => CanonicalSeries != null;
}
