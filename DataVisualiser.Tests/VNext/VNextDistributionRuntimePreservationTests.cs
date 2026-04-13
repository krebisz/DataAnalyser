using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.Distribution;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.Charts.Controllers;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.MainHost.Evidence;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using DataVisualiser.VNext.Application;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Kernel;

namespace DataVisualiser.Tests.VNext;

public sealed class VNextDistributionRuntimePreservationTests
{
    private static readonly DateTime From = new(2026, 1, 1);
    private static readonly DateTime To = new(2026, 1, 31);

    [Fact]
    public async Task RenderAsync_FreshVNextLoad_SetsVNextDistributionRuntime()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var (adapter, chartState) = CreateAdapterWithVNextCoordinator();

            chartState.IsDistributionVisible = true;
            chartState.SelectedDistributionSeries = new MetricSeriesSelection("Weight", "water", "Weight", "Water");

            var ctx = CreateContext("Weight", "fat_free_mass", "muscle_mass");
            await adapter.RenderAsync(ctx);

            Assert.NotNull(chartState.LastDistributionLoadRuntime);
            Assert.Equal(EvidenceRuntimePath.VNextDistribution, chartState.LastDistributionLoadRuntime!.RuntimePath);
            Assert.NotNull(chartState.LastDistributionLoadRuntime.SnapshotSignature);
            Assert.Equal(ChartProgramKind.Distribution, chartState.LastDistributionLoadRuntime.ProgramKind);
        });
    }

    [Fact]
    public async Task RenderAsync_ReusePrimaryPath_PreservesExistingDistributionRuntime()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var (adapter, chartState) = CreateAdapterWithVNextCoordinator();

            chartState.IsDistributionVisible = true;
            chartState.SelectedDistributionSeries = new MetricSeriesSelection("Weight", "water", "Weight", "Water");

            var ctx = CreateContext("Weight", "fat_free_mass", "muscle_mass");
            await adapter.RenderAsync(ctx);

            Assert.NotNull(chartState.LastDistributionLoadRuntime);
            var savedRuntime = chartState.LastDistributionLoadRuntime;

            // Switch to primary series — reuse path
            chartState.SelectedDistributionSeries = new MetricSeriesSelection("Weight", "fat_free_mass", "Weight", "Fat Free");
            await adapter.RenderAsync(ctx);

            Assert.Same(savedRuntime, chartState.LastDistributionLoadRuntime);
        });
    }

    [Fact]
    public async Task RenderAsync_ReuseSecondaryPath_PreservesExistingDistributionRuntime()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var (adapter, chartState) = CreateAdapterWithVNextCoordinator();

            chartState.IsDistributionVisible = true;
            chartState.SelectedDistributionSeries = new MetricSeriesSelection("Weight", "water", "Weight", "Water");

            var ctx = CreateContext("Weight", "fat_free_mass", "muscle_mass");
            await adapter.RenderAsync(ctx);

            Assert.NotNull(chartState.LastDistributionLoadRuntime);
            var savedRuntime = chartState.LastDistributionLoadRuntime;

            // Switch to secondary series — reuse path
            chartState.SelectedDistributionSeries = new MetricSeriesSelection("Weight", "muscle_mass", "Weight", "Muscle");
            await adapter.RenderAsync(ctx);

            Assert.Same(savedRuntime, chartState.LastDistributionLoadRuntime);
        });
    }

    [Fact]
    public async Task RenderAsync_CacheHit_PreservesExistingDistributionRuntime()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var (adapter, chartState) = CreateAdapterWithVNextCoordinator();

            chartState.IsDistributionVisible = true;
            chartState.SelectedDistributionSeries = new MetricSeriesSelection("Weight", "water", "Weight", "Water");

            var ctx = CreateContext("Weight", "fat_free_mass", "muscle_mass");
            await adapter.RenderAsync(ctx);

            Assert.NotNull(chartState.LastDistributionLoadRuntime);
            var savedRuntime = chartState.LastDistributionLoadRuntime;

            // Render again with same non-primary/secondary series — cache hit
            await adapter.RenderAsync(ctx);

            Assert.Same(savedRuntime, chartState.LastDistributionLoadRuntime);
        });
    }

    private static (DistributionChartControllerAdapter Adapter, ChartState ChartState) CreateAdapterWithVNextCoordinator()
    {
        var chartState = new ChartState { SelectedDistributionMode = DistributionMode.Weekly };
        var metricState = new MetricState();
        metricState.ResolutionTableName = "HealthMetrics";
        var uiState = new UiState();
        var metricService = new MetricSelectionService("TestConnection");
        var viewModel = new MainWindowViewModel(chartState, metricState, uiState, metricService);
        var controller = new DistributionChartController();
        var renderingContract = new FakeDistributionRenderingContract();
        var vnextCoordinator = new VNextDistributionIntegrationCoordinator(CreateStubSessionCoordinator);

        var adapter = new DistributionChartControllerAdapter(
            controller,
            viewModel,
            () => false,
            () => new NoOpScope(),
            metricService,
            renderingContract,
            () => null,
            vnextCoordinator);

        return (adapter, chartState);
    }

    private static ChartDataContext CreateContext(string metricType, string primarySubtype, string secondarySubtype)
    {
        return new ChartDataContext
        {
            Data1 = [new MetricData { NormalizedTimestamp = From, Value = 1m }],
            Data2 = [new MetricData { NormalizedTimestamp = From, Value = 2m }],
            DisplayName1 = primarySubtype,
            DisplayName2 = secondarySubtype,
            MetricType = metricType,
            PrimaryMetricType = metricType,
            SecondaryMetricType = metricType,
            PrimarySubtype = primarySubtype,
            SecondarySubtype = secondarySubtype,
            From = From,
            To = To
        };
    }

    private static ReasoningSessionCoordinator CreateStubSessionCoordinator()
    {
        var loader = new StubMetricSeriesLoader();
        var gateway = new LegacyMetricViewGateway(loader);
        var planner = new ChartProgramPlanner(new TimeSeriesAlignmentKernel(), new OperationKernel());
        var engine = new ReasoningEngine(gateway, planner);
        return new ReasoningSessionCoordinator(engine);
    }

    private sealed class StubMetricSeriesLoader : IMetricSeriesLoader
    {
        public Task<LoadedMetricSeries> LoadAsync(
            MetricSeriesRequest request,
            DateTime from,
            DateTime to,
            string resolutionTableName,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new LoadedMetricSeries(
                [new MetricData { NormalizedTimestamp = from, Value = 1m }],
                null));
        }
    }

    private sealed class NoOpScope : IDisposable
    {
        public void Dispose() { }
    }

    private sealed class FakeDistributionRenderingContract : IDistributionRenderingContract
    {
        public IReadOnlyList<DistributionBackendQualification> GetBackendQualificationMatrix() => [];

        public DistributionRenderingCapabilities GetCapabilities(DistributionRenderingRoute route) =>
            new("test", DistributionRenderingQualification.Qualified, true, true, true, true, true, true);

        public Task RenderAsync(DistributionChartRenderRequest request, DistributionChartRenderHost host) =>
            Task.CompletedTask;

        public void Clear(DistributionChartRenderHost host) { }

        public void ResetView(DistributionRenderingRoute route, DistributionChartRenderHost host) { }

        public bool HasRenderableContent(DistributionRenderingRoute route, DistributionChartRenderHost host) => false;
    }
}
