using DataFileReader.Canonical;
using DataVisualiser.Core.Rendering.Shading;
using DataVisualiser.Shared.Models;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Services.Abstractions;

public interface IDistributionService
{
    Task UpdateDistributionChartAsync(CartesianChart targetChart, IEnumerable<MetricData> data, string displayName, DateTime from, DateTime to, double minHeight = 400.0, bool useFrequencyShading = true, int intervalCount = 10, ICanonicalMetricSeries? cmsSeries = null, bool enableParity = false);

    Task<DistributionRangeResult?> ComputeSimpleRangeAsync(IEnumerable<MetricData> data, string displayName, DateTime from, DateTime to, ICanonicalMetricSeries? cmsSeries = null, bool enableParity = false);

    void SetShadingStrategy(IIntervalShadingStrategy strategy);
}