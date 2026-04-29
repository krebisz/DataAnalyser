using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Configuration;
using DataVisualiser.Core.Data;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Validation;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.Tests.UI.Charts.Presentation;

public sealed class Phase20BuilderInvokerTests
{
    // ─── SyncfusionSunburstRenderModelBuilder ───────────────────────────────

    [Fact]
    public async Task SunburstBuilder_WithNoSelections_ReturnsEmptyModel()
    {
        var builder = CreateSunburstBuilder(out _, new FakeMetricQueries());
        var model = await WithCmsDisabledAsync(() => builder.BuildAsync());
        Assert.Empty(model.Items);
        Assert.Equal(0, model.SelectionCount);
    }

    [Fact]
    public async Task SunburstBuilder_WithSelectionsButNoDateRange_ReturnsEmptyItems()
    {
        var builder = CreateSunburstBuilder(out var viewModel, new FakeMetricQueries());
        viewModel.MetricState.SetSeriesSelections([new MetricSeriesSelection("Weight", "fat")]);

        var model = await WithCmsDisabledAsync(() => builder.BuildAsync());

        Assert.Empty(model.Items);
        Assert.Equal(1, model.SelectionCount);
    }

    [Fact]
    public async Task SunburstBuilder_WithSelectionsAndData_ReturnsBucketedItems()
    {
        var queries = new FakeMetricQueries();
        queries.SetData("Weight", "fat", [
            new MetricData { NormalizedTimestamp = new DateTime(2026, 4, 1), Value = 10m },
            new MetricData { NormalizedTimestamp = new DateTime(2026, 4, 2), Value = 14m }
        ]);

        var builder = CreateSunburstBuilder(out var viewModel, queries);
        viewModel.ChartState.BarPieBucketCount = 2;
        viewModel.MetricState.FromDate = new DateTime(2026, 4, 1);
        viewModel.MetricState.ToDate = new DateTime(2026, 4, 3);
        viewModel.MetricState.SetSeriesSelections([new MetricSeriesSelection("Weight", "fat")]);

        var model = await WithCmsDisabledAsync(() => builder.BuildAsync());

        Assert.NotEmpty(model.Items);
        Assert.Equal(2, model.BucketCount);
        Assert.Equal(1, model.SelectionCount);
    }

    // ─── CartesianMetricOverlaySeriesBuilder ────────────────────────────────

    [Fact]
    public async Task OverlayBuilder_WithEmptySelections_ReturnsNull()
    {
        var builder = CreateOverlayBuilder(out _, new FakeMetricQueries());
        var result = await builder.BuildAsync(new ChartDataContext(), []);
        Assert.Null(result);
    }

    [Fact]
    public async Task OverlayBuilder_WithMatchingContextData_ReturnsSeriesResult()
    {
        var builder = CreateOverlayBuilder(out var viewModel, new FakeMetricQueries());
        var data = new List<MetricData>
        {
            new() { NormalizedTimestamp = new DateTime(2026, 4, 1), Value = 10m },
            new() { NormalizedTimestamp = new DateTime(2026, 4, 2), Value = 12m }
        };
        var selection = new MetricSeriesSelection("Weight", "fat");
        viewModel.ChartState.SelectedStackedOverlaySeries = selection;

        var ctx = new ChartDataContext
        {
            Data1 = data,
            PrimaryMetricType = "Weight",
            PrimarySubtype = "fat",
            From = new DateTime(2026, 1, 1),
            To = new DateTime(2026, 12, 31)
        };

        var result = await builder.BuildAsync(ctx, [selection]);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Contains("(overlay)", result[0].DisplayName);
        Assert.Equal("overlay_0", result[0].SeriesId);
    }

    [Fact]
    public async Task OverlayBuilder_WithNoMatchingContextAndNoMetricType_ReturnsNull()
    {
        var builder = CreateOverlayBuilder(out _, new FakeMetricQueries());
        var ctx = new ChartDataContext
        {
            Data1 = null,
            From = new DateTime(2026, 1, 1),
            To = new DateTime(2026, 12, 31)
        };
        var selection = new MetricSeriesSelection(null!, null);

        var result = await builder.BuildAsync(ctx, [selection]);

        Assert.Null(result);
    }

    // ─── DistributionRenderInputBuilder ─────────────────────────────────────

    [Fact]
    public async Task DistributionBuilder_WithNoData_ReturnsNull()
    {
        var builder = CreateDistributionBuilder(out _);
        var ctx = new ChartDataContext { Data1 = null, From = new DateTime(2026, 1, 1), To = new DateTime(2026, 1, 7) };
        var result = await builder.BuildAsync(ctx, selectedSeries: null);
        Assert.Null(result);
    }

    [Fact]
    public void DistributionBuilder_ClearCache_DoesNotThrow()
    {
        var builder = CreateDistributionBuilder(out _);
        var ex = Record.Exception(() => builder.ClearCache());
        Assert.Null(ex);
    }

    [Fact]
    public void DistributionBuilder_BuildDistributionContext_MapsRenderInputCorrectly()
    {
        var data = new List<MetricData>();
        var renderInput = new DistributionRenderInput(
            new MetricSeriesSelection("Weight", "fat"),
            data,
            null,
            "Fat Mass");
        var ctx = new ChartDataContext
        {
            MetricType = "Weight",
            PrimaryMetricType = "Weight",
            From = new DateTime(2026, 1, 1),
            To = new DateTime(2026, 1, 7)
        };

        var result = DistributionRenderInputBuilder.BuildDistributionContext(ctx, renderInput);

        Assert.Same(data, result.Data1);
        Assert.Equal("Fat Mass", result.DisplayName1);
        Assert.Equal("Weight", result.PrimaryMetricType);
        Assert.Equal(new DateTime(2026, 1, 1), result.From);
        Assert.Equal(new DateTime(2026, 1, 7), result.To);
    }

    // ─── WeekdayTrendComputationInvoker ─────────────────────────────────────

    [Fact]
    public void WeekdayTrendInvoker_ClearCache_DoesNotThrow()
    {
        var invoker = CreateWeekdayTrendInvoker(result: null);
        var ex = Record.Exception(() => invoker.ClearCache());
        Assert.Null(ex);
    }

    [Fact]
    public async Task WeekdayTrendInvoker_ComputeAsync_WithNoContextData_ReturnsNull()
    {
        var invoker = CreateWeekdayTrendInvoker(result: null);
        var ctx = new ChartDataContext { Data1 = null, From = new DateTime(2026, 1, 1), To = new DateTime(2026, 1, 7) };
        var result = await invoker.ComputeAsync(ctx, selectedSeries: null, displayName: "Weight");
        Assert.Null(result);
    }

    [Fact]
    public void WeekdayTrendInvoker_ComputeFromContext_DelegatesToStrategy()
    {
        var expected = new WeekdayTrendResult { From = new DateTime(2026, 1, 1), To = new DateTime(2026, 1, 7) };
        var invoker = CreateWeekdayTrendInvoker(result: expected);
        var ctx = new ChartDataContext
        {
            Data1 = [new MetricData { NormalizedTimestamp = new DateTime(2026, 1, 1), Value = 5m }],
            DisplayName1 = "Weight",
            From = new DateTime(2026, 1, 1),
            To = new DateTime(2026, 1, 7)
        };

        var result = invoker.ComputeFromContext(ctx);

        Assert.Same(expected, result);
    }

    // ─── Helpers ────────────────────────────────────────────────────────────

    private static SyncfusionSunburstRenderModelBuilder CreateSunburstBuilder(out MainWindowViewModel viewModel, FakeMetricQueries queries)
    {
        var metricService = new MetricSelectionService(queries, "Server=(localdb)\\MSSQLLocalDB;Database=Fake;Trusted_Connection=True;");
        var chartState = new ChartState { IsSyncfusionSunburstVisible = true };
        viewModel = new MainWindowViewModel(chartState, new MetricState(), new UiState(), metricService);
        return new SyncfusionSunburstRenderModelBuilder(viewModel, metricService);
    }

    private static CartesianMetricOverlaySeriesBuilder CreateOverlayBuilder(out MainWindowViewModel viewModel, FakeMetricQueries queries)
    {
        var metricService = new MetricSelectionService(queries, "Server=(localdb)\\MSSQLLocalDB;Database=Fake;Trusted_Connection=True;");
        viewModel = new MainWindowViewModel(new ChartState(), new MetricState(), new UiState(), metricService);
        return new CartesianMetricOverlaySeriesBuilder(viewModel, metricService);
    }

    private static DistributionRenderInputBuilder CreateDistributionBuilder(out MainWindowViewModel viewModel)
    {
        var metricService = new MetricSelectionService("Server=(localdb)\\MSSQLLocalDB;Database=Fake;Trusted_Connection=True;");
        viewModel = new MainWindowViewModel(new ChartState(), new MetricState(), new UiState(), metricService);
        return new DistributionRenderInputBuilder(viewModel, metricService);
    }

    private static WeekdayTrendComputationInvoker CreateWeekdayTrendInvoker(WeekdayTrendResult? result)
    {
        var metricService = new MetricSelectionService("Server=(localdb)\\MSSQLLocalDB;Database=Fake;Trusted_Connection=True;");
        var viewModel = new MainWindowViewModel(new ChartState(), new MetricState(), new UiState(), metricService);
        var cutOverService = new FakeStrategyCutOverService(result);
        return new WeekdayTrendComputationInvoker(viewModel, metricService, () => cutOverService);
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

    private sealed class FakeMetricQueries : IMetricSelectionDataQueries
    {
        private readonly Dictionary<(string, string?), IReadOnlyList<MetricData>> _data = new();

        public void SetData(string metricType, string? subtype, IReadOnlyList<MetricData> data)
            => _data[(metricType, subtype)] = data;

        public Task<IEnumerable<MetricData>> GetHealthMetricsDataByBaseType(string baseType, string? subtype, DateTime? from, DateTime? to, string tableName, int? maxRecords = null, SamplingMode samplingMode = SamplingMode.None, int? targetSamples = null)
        {
            if (!_data.TryGetValue((baseType, subtype), out var data))
                return Task.FromResult<IEnumerable<MetricData>>(Array.Empty<MetricData>());

            IEnumerable<MetricData> filtered = data;
            if (from.HasValue) filtered = filtered.Where(d => d.NormalizedTimestamp >= from.Value);
            if (to.HasValue) filtered = filtered.Where(d => d.NormalizedTimestamp <= to.Value);
            return Task.FromResult(filtered);
        }

        public Task<long> GetRecordCount(string metricType, string? metricSubtype = null)
            => Task.FromResult((long)(_data.TryGetValue((metricType, metricSubtype), out var d) ? d.Count : 0));

        public Task<IEnumerable<MetricNameOption>> GetBaseMetricTypeOptions(string tableName) => Task.FromResult<IEnumerable<MetricNameOption>>(Array.Empty<MetricNameOption>());
        public Task<IEnumerable<MetricNameOption>> GetSubtypeOptionsForBaseType(string baseType, string tableName) => Task.FromResult<IEnumerable<MetricNameOption>>(Array.Empty<MetricNameOption>());
        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRange(string baseType, string? subtype, string tableName) => Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRangeFromCounts(string baseType, IReadOnlyCollection<string>? subtypes = null) => Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRangeForSubtypes(string baseType, IReadOnlyCollection<string>? subtypes, string tableName) => Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
    }

    private sealed class FakeStrategyCutOverService : IStrategyCutOverService
    {
        private readonly WeekdayTrendResult? _result;

        public FakeStrategyCutOverService(WeekdayTrendResult? result) => _result = result;

        public IChartComputationStrategy CreateStrategy(StrategyType strategyType, ChartDataContext ctx, StrategyCreationParameters parameters)
            => new FakeWeekdayTrendStrategy(_result);

        public IChartComputationStrategy CreateCmsStrategy(StrategyType strategyType, ChartDataContext ctx, StrategyCreationParameters parameters)
            => throw new NotSupportedException();

        public IChartComputationStrategy CreateLegacyStrategy(StrategyType strategyType, StrategyCreationParameters parameters)
            => throw new NotSupportedException();

        public bool ShouldUseCms(StrategyType strategyType, ChartDataContext ctx) => false;

        public ParityResult ValidateParity(IChartComputationStrategy legacyStrategy, IChartComputationStrategy cmsStrategy)
            => throw new NotSupportedException();
    }

    private sealed class FakeWeekdayTrendStrategy : IChartComputationStrategy, IWeekdayTrendResultProvider
    {
        public FakeWeekdayTrendStrategy(WeekdayTrendResult? result) => ExtendedResult = result;

        public string PrimaryLabel => "Weight";
        public string SecondaryLabel => string.Empty;
        public string? Unit => "kg";
        public WeekdayTrendResult? ExtendedResult { get; }

        public ChartComputationResult? Compute() => new();
    }
}
