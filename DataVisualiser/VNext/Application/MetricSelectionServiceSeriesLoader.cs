using DataVisualiser.Core.Services;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.VNext.Application;

public sealed class MetricSelectionServiceSeriesLoader : IMetricSeriesLoader
{
    private readonly MetricSelectionService _metricSelectionService;

    public MetricSelectionServiceSeriesLoader(MetricSelectionService metricSelectionService)
    {
        _metricSelectionService = metricSelectionService ?? throw new ArgumentNullException(nameof(metricSelectionService));
    }

    public async Task<LoadedMetricSeries> LoadAsync(
        MetricSeriesRequest request,
        DateTime from,
        DateTime to,
        string resolutionTableName,
        CancellationToken cancellationToken = default)
    {
        var legacySelection = request.ToLegacySelection();
        var (primaryCms, _, primaryLegacy, _) =
            await _metricSelectionService.LoadMetricDataWithCmsAsync(legacySelection, null, from, to, resolutionTableName);

        return new LoadedMetricSeries(primaryLegacy.ToList(), primaryCms);
    }
}
