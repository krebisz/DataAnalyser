using DataVisualiser.Core.Configuration;
using DataVisualiser.Core.Data;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.Tests.Controls;

public sealed class BarPieRenderModelBuilderTests
{
    [Fact]
    public async Task BuildAsync_WithNoSelections_ReturnsEmptyModel()
    {
        var builder = CreateBuilder(out _, new FakeMetricSelectionDataQueries());

        var model = await WithCmsDisabledAsync(() => builder.BuildAsync(isPieMode: false));

        Assert.Empty(model.Series);
        Assert.Empty(model.Facets);
    }

    [Fact]
    public async Task BuildAsync_BarMode_BuildsDistinctSeriesAndBucketLabels()
    {
        var queries = new FakeMetricSelectionDataQueries();
        queries.SetSeriesData("Weight", "body_fat_mass",
        [
            CreateMetricData(2026, 4, 1, 10m),
            CreateMetricData(2026, 4, 2, 14m)
        ]);
        queries.SetSeriesData("Weight", "fat_free_mass",
        [
            CreateMetricData(2026, 4, 1, 20m),
            CreateMetricData(2026, 4, 2, 26m)
        ]);

        var builder = CreateBuilder(out var viewModel, queries);
        viewModel.ChartState.BarPieBucketCount = 2;
        viewModel.ChartState.IsBarPieVisible = true;
        viewModel.MetricState.FromDate = new DateTime(2026, 4, 1);
        viewModel.MetricState.ToDate = new DateTime(2026, 4, 3);
        viewModel.MetricState.SetSeriesSelections(
        [
            new MetricSeriesSelection("Weight", "body_fat_mass"),
            new MetricSeriesSelection("Weight", "body_fat_mass"),
            new MetricSeriesSelection("Weight", "fat_free_mass")
        ]);

        var model = await WithCmsDisabledAsync(() => builder.BuildAsync(isPieMode: false));

        Assert.Equal(2, model.Series.Count);
        var axis = Assert.Single(model.AxesX);
        Assert.NotNull(axis.Labels);
        Assert.Equal(2, axis.Labels.Count);
        Assert.All(model.Series, series => Assert.Equal(2, series.Values.Count));
    }

    [Fact]
    public async Task BuildAsync_PieMode_BuildsFacetPerBucket()
    {
        var queries = new FakeMetricSelectionDataQueries();
        queries.SetSeriesData("Weight", "body_fat_mass",
        [
            CreateMetricData(2026, 4, 1, 10m),
            CreateMetricData(2026, 4, 2, 12m)
        ]);

        var builder = CreateBuilder(out var viewModel, queries);
        viewModel.ChartState.BarPieBucketCount = 2;
        viewModel.ChartState.IsBarPieVisible = true;
        viewModel.MetricState.FromDate = new DateTime(2026, 4, 1);
        viewModel.MetricState.ToDate = new DateTime(2026, 4, 3);
        viewModel.MetricState.SetSeriesSelections(
        [
            new MetricSeriesSelection("Weight", "body_fat_mass")
        ]);

        var model = await WithCmsDisabledAsync(() => builder.BuildAsync(isPieMode: true));

        Assert.Equal(2, model.Facets.Count);
        Assert.All(model.Facets, facet => Assert.Single(facet.Series));
        Assert.Empty(model.Series);
    }

    private static BarPieRenderModelBuilder CreateBuilder(out MainWindowViewModel viewModel, IMetricSelectionDataQueries queries)
    {
        var chartState = new ChartState();
        var metricState = new MetricState();
        var uiState = new UiState();
        var metricSelectionService = new MetricSelectionService(queries, "Server=(localdb)\\MSSQLLocalDB;Database=Fake;Trusted_Connection=True;");
        viewModel = new MainWindowViewModel(chartState, metricState, uiState, metricSelectionService);
        return new BarPieRenderModelBuilder(viewModel, metricSelectionService, new object());
    }

    private static async Task<T> WithCmsDisabledAsync<T>(Func<Task<T>> action)
    {
        var originalUseCmsData = CmsConfiguration.UseCmsData;
        var originalUseCmsForBarPie = CmsConfiguration.UseCmsForBarPie;
        CmsConfiguration.UseCmsData = false;
        CmsConfiguration.UseCmsForBarPie = false;

        try
        {
            return await action();
        }
        finally
        {
            CmsConfiguration.UseCmsData = originalUseCmsData;
            CmsConfiguration.UseCmsForBarPie = originalUseCmsForBarPie;
        }
    }

    private static MetricData CreateMetricData(int year, int month, int day, decimal value)
    {
        return new MetricData
        {
            NormalizedTimestamp = new DateTime(year, month, day),
            Value = value
        };
    }

    private sealed class FakeMetricSelectionDataQueries : IMetricSelectionDataQueries
    {
        private readonly Dictionary<(string MetricType, string? Subtype), IReadOnlyList<MetricData>> _seriesData = new();

        public void SetSeriesData(string metricType, string? subtype, IReadOnlyList<MetricData> data)
        {
            _seriesData[(metricType, subtype)] = data;
        }

        public Task<long> GetRecordCount(string metricType, string? metricSubtype = null)
        {
            return Task.FromResult((long)(_seriesData.TryGetValue((metricType, metricSubtype), out var data) ? data.Count : 0));
        }

        public Task<IEnumerable<MetricData>> GetHealthMetricsDataByBaseType(string baseType, string? subtype, DateTime? from, DateTime? to, string tableName, int? maxRecords = null, SamplingMode samplingMode = SamplingMode.None, int? targetSamples = null)
        {
            if (!_seriesData.TryGetValue((baseType, subtype), out var data))
                return Task.FromResult<IEnumerable<MetricData>>(Array.Empty<MetricData>());

            IEnumerable<MetricData> filtered = data;
            if (from.HasValue)
                filtered = filtered.Where(item => item.NormalizedTimestamp >= from.Value);
            if (to.HasValue)
                filtered = filtered.Where(item => item.NormalizedTimestamp <= to.Value);

            return Task.FromResult(filtered);
        }

        public Task<IEnumerable<MetricNameOption>> GetBaseMetricTypeOptions(string tableName) => Task.FromResult<IEnumerable<MetricNameOption>>(Array.Empty<MetricNameOption>());
        public Task<IEnumerable<MetricNameOption>> GetSubtypeOptionsForBaseType(string baseType, string tableName) => Task.FromResult<IEnumerable<MetricNameOption>>(Array.Empty<MetricNameOption>());
        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRange(string baseType, string? subtype, string tableName) => Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRangeFromCounts(string baseType, IReadOnlyCollection<string>? subtypes = null) => Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRangeForSubtypes(string baseType, IReadOnlyCollection<string>? subtypes, string tableName) => Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
    }
}
