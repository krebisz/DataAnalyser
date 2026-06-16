using DataFileReader.Canonical;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.MainHost.Evidence;
using DataVisualiser.UI.ViewModels;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.UI.Charts.Presentation;

internal sealed class DistributionRenderInputBuilder
{
    private readonly TemporalMetricSeriesInputBuilder _inputBuilder;

    public DistributionRenderInputBuilder(MainWindowViewModel viewModel, MetricSelectionService metricSelectionService, VNextSeriesLoadCoordinator? vnextCoordinator = null)
    {
        _inputBuilder = new TemporalMetricSeriesInputBuilder(viewModel, metricSelectionService, vnextCoordinator);
    }

    public void ClearCache() => _inputBuilder.ClearCache();

    public async Task<DistributionRenderInput?> BuildAsync(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
    {
        return await BuildAsync(CreateRequest(ctx, selectedSeries));
    }

    public async Task<DistributionRenderInput?> BuildAsync(DistributionRenderInputRequest request)
    {
        var input = await _inputBuilder.BuildAsync(request.InputRequest);
        if (input == null || (input.Data.Count == 0 && input.CmsSeries == null))
            return null;

        return new DistributionRenderInput(
            input.SelectedSeries,
            input.Data,
            input.CmsSeries,
            input.DisplayName,
            input.From,
            input.To,
            input.RenderingContext);
    }

    public DistributionRenderInputRequest CreateRequest(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
    {
        return new DistributionRenderInputRequest(
            _inputBuilder.CreateRequest(
                ctx,
                selectedSeries,
                ResolveDistributionDisplayName(ctx, selectedSeries),
                ChartProgramKind.Distribution,
                EvidenceRuntimePath.VNextDistribution,
                async (metricSelectionService, sel, from, to, table) =>
                {
                    var (cms, _, data, _) = await metricSelectionService.LoadMetricDataWithCmsAsync(sel, null, from, to, table);
                    return (data.ToList(), cms);
                }));
    }

    private static string ResolveDistributionDisplayName(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
    {
        return MetricSeriesSelectionAdapterHelper.ResolveDisplayName(ctx, selectedSeries);
    }

    public static ChartDataContext BuildDistributionContext(ChartDataContext ctx, DistributionRenderInput renderInput)
    {
        return renderInput.RenderingContext
            ?? TemporalMetricSeriesInputBuilder.BuildContext(ctx, renderInput.SelectedSeries, renderInput.Data, renderInput.CmsSeries, renderInput.DisplayName, renderInput.From, renderInput.To);
    }
}

internal sealed record DistributionRenderInputRequest(
    TemporalMetricSeriesInputRequest InputRequest);

internal sealed record DistributionRenderInput(
    MetricSeriesSelection? SelectedSeries,
    IReadOnlyList<MetricData> Data,
    ICanonicalMetricSeries? CmsSeries,
    string DisplayName,
    DateTime From,
    DateTime To,
    ChartDataContext? RenderingContext = null);
