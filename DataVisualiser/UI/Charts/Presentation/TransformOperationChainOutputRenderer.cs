using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Rendering.Transform;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Presentation.LiveCharts;
using DataVisualiser.UI.State;
using DataVisualiser.VNext.Contracts;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Charts.Presentation;

internal sealed class TransformOperationChainOutputRenderer
{
    private readonly ChartState _chartState;
    private readonly ITransformRenderingContract _renderingContract;

    public TransformOperationChainOutputRenderer()
        : this(TransformRenderingPipelineFactory.CreateIsolated())
    {
    }

    private TransformOperationChainOutputRenderer(TransformRenderingPipeline pipeline)
        : this(pipeline.ChartState, pipeline.RenderingContract)
    {
    }

    internal TransformOperationChainOutputRenderer(
        ChartState chartState,
        ITransformRenderingContract? renderingContract = null)
    {
        _chartState = chartState ?? throw new ArgumentNullException(nameof(chartState));
        _renderingContract = renderingContract ?? TransformRenderingPipelineFactory.CreateContract(_chartState);
    }

    public Task RenderAsync(CartesianChart chart, OperationChainResult result)
    {
        return RenderAsync(chart, result, null);
    }

    public Task RenderAsync(CartesianChart chart, OperationChainResult result, IReadOnlyList<bool>? includedRows)
    {
        ArgumentNullException.ThrowIfNull(result);

        var dataset = result.DerivedDatasets.LastOrDefault();
        return dataset == null
            ? ClearAsync(chart)
            : RenderComputedSeriesAsync(
                chart,
                FilterComputedSeries(ComputedSeriesResult.FromDerivedDataset(dataset), includedRows),
                CreateContext(result, dataset.Label),
                result.Trace.Entries.LastOrDefault()?.OperationKind);
    }

    public Task RenderCorrelationAsync(CartesianChart chart, TransformCorrelationSummary summary)
    {
        ArgumentNullException.ThrowIfNull(chart);
        ArgumentNullException.ThrowIfNull(summary);

        CartesianChartProjectionRenderer.Apply(chart, TransformCorrelationChartModelBuilder.Build(summary));
        ApplyOperationChartTheme(chart);
        return Task.CompletedTask;
    }

    public Task ClearAsync(CartesianChart chart)
    {
        ArgumentNullException.ThrowIfNull(chart);

        _renderingContract.Clear(TransformRenderingRoute.ResultCartesian, CreateHost(chart));
        TransformChartEmptyStateHelper.Apply(chart);
        ApplyOperationChartTheme(chart);
        return Task.CompletedTask;
    }

    public Task RenderInputSnapshotAsync(CartesianChart chart, MetricLoadSnapshot snapshot)
    {
        return RenderInputSnapshotAsync(chart, snapshot, null);
    }

    public Task RenderInputSnapshotAsync(CartesianChart chart, MetricLoadSnapshot snapshot, IReadOnlyList<bool>? includedRows)
    {
        var filteredSnapshot = FilterInputSnapshot(snapshot, includedRows);
        var orderedSeries = filteredSnapshot.Series
            .Select(series => MetricDataSeriesHelper.FilterValuedAndOrder(series.RawData).ToArray())
            .Where(series => series.Length > 0)
            .ToArray();

        if (orderedSeries.Length == 0)
            return ClearAsync(chart);

        var labels = filteredSnapshot.Series
            .Take(orderedSeries.Length)
            .Select(series => series.Request.DisplayName)
            .ToArray();
        var from = orderedSeries.SelectMany(series => series).Min(point => point.NormalizedTimestamp);
        var to = orderedSeries.SelectMany(series => series).Max(point => point.NormalizedTimestamp);
        var strategy = new MultiMetricStrategy(orderedSeries, labels, from, to);
        var context = new ChartDataContext
        {
            Data1 = orderedSeries[0].ToList(),
            From = from,
            To = to,
            DisplayName1 = labels.FirstOrDefault() ?? "Input Series",
            PrimaryMetricType = snapshot.Request.MetricType,
            MetricType = snapshot.Request.MetricType
        };

        return RenderStrategyAsync(
            chart,
            strategy,
            context,
            "Input Series",
            operationType: null,
            isOperationChart: false,
            TransformCapabilityContract.Create("Input Series"));
    }

    private static ComputedSeriesResult FilterComputedSeries(
        ComputedSeriesResult series,
        IReadOnlyList<bool>? includedRows)
    {
        if (includedRows == null)
            return series;

        var indexes = ResolveIncludedIndexes(includedRows, series.Timeline.Count);
        return new ComputedSeriesResult(
            series.Id,
            series.Label,
            indexes.Select(index => series.Timeline[index]).ToArray(),
            indexes.Select(index => series.RawValues[index]).ToArray(),
            indexes.Select(index => series.SmoothedValues[index]).ToArray(),
            series.SourceSeriesSignatures,
            series.OperationSignature,
            series.Metadata);
    }

    private static MetricLoadSnapshot FilterInputSnapshot(
        MetricLoadSnapshot snapshot,
        IReadOnlyList<bool>? includedRows)
    {
        if (includedRows == null)
            return snapshot;

        var cursor = 0;
        var filteredSeries = new List<MetricSeriesSnapshot>();
        foreach (var series in snapshot.Series)
        {
            var orderedData = MetricDataSeriesHelper.FilterValuedAndOrder(series.RawData).ToArray();
            var filteredData = new List<MetricData>();
            for (var index = 0; index < orderedData.Length; index++)
            {
                var include = cursor >= includedRows.Count || includedRows[cursor];
                if (include)
                    filteredData.Add(orderedData[index]);

                cursor++;
            }

            filteredSeries.Add(new MetricSeriesSnapshot(series.Request, filteredData, series.CanonicalSeries));
        }

        return new MetricLoadSnapshot(snapshot.Request, filteredSeries, snapshot.CreatedUtc);
    }

    private static IReadOnlyList<int> ResolveIncludedIndexes(IReadOnlyList<bool> includedRows, int count)
    {
        var indexes = new List<int>();
        for (var index = 0; index < count; index++)
            if (index >= includedRows.Count || includedRows[index])
                indexes.Add(index);

        return indexes;
    }

    private Task RenderComputedSeriesAsync(
        CartesianChart chart,
        ComputedSeriesResult computedSeries,
        ChartDataContext context,
        SeriesOperationKind? operationKind)
    {
        var dataList = TransformComputedSeriesPresentationAdapter.ProjectMetricData(computedSeries).ToList();
        if (dataList.Count == 0)
            return ClearAsync(chart);

        var from = dataList.Min(point => point.NormalizedTimestamp);
        var to = dataList.Max(point => point.NormalizedTimestamp);
        var strategy = new TransformResultStrategy(
            dataList,
            computedSeries.RawValues.ToList(),
            computedSeries.Label,
            from,
            to);

        return RenderStrategyAsync(
            chart,
            strategy,
            context,
            computedSeries.Label,
            TransformComputedSeriesPresentationAdapter.ResolveOperationType(operationKind),
            operationKind != null,
            TransformCapabilityContract.Create(
                computedSeries.Label,
                TransformComputedSeriesPresentationAdapter.CreateOperationRequests(computedSeries, operationKind)));
    }

    private async Task RenderStrategyAsync(
        CartesianChart chart,
        IChartComputationStrategy strategy,
        ChartDataContext context,
        string label,
        string? operationType,
        bool isOperationChart,
        TransformCapabilityContract capabilityContract)
    {
        await _renderingContract.RenderAsync(
            new TransformChartRenderRequest(
                TransformRenderingRoute.ResultCartesian,
                context,
                strategy,
                label,
                operationType,
                isOperationChart,
                MinHeight: 260.0,
                CapabilityContract: capabilityContract),
            CreateHost(chart));
        ApplyOperationChartTheme(chart);
    }

    private static void ApplyOperationChartTheme(CartesianChart chart) =>
        ChartThemeStylingHelper.ApplyCartesianChartTheme(chart);

    private TransformChartRenderHost CreateHost(CartesianChart chart) =>
        new(chart, _chartState);

    private static ChartDataContext CreateContext(OperationChainResult result, string label)
    {
        var selection = result.Request.Selection;
        return new ChartDataContext
        {
            From = selection.From,
            To = selection.To,
            DisplayName1 = label,
            PrimaryMetricType = selection.MetricType,
            MetricType = selection.MetricType,
            PrimarySubtype = selection.Series.FirstOrDefault()?.Subtype,
            DisplayPrimarySubtype = selection.Series.FirstOrDefault()?.DisplaySubtype
        };
    }
}
