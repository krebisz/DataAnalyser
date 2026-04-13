using DataVisualiser.Shared.Models;
using DataVisualiser.UI.MainHost;
using DataVisualiser.VNext.Application;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Kernel;

namespace DataVisualiser.Tests.VNext;

public sealed class VNextDistributionIntegrationCoordinatorTests
{
    private static readonly DateTime From = new(2026, 1, 1);
    private static readonly DateTime To = new(2026, 1, 7);

    [Fact]
    public async Task LoadDistributionAsync_ShouldReturnDataWithSignatureChain()
    {
        var coordinator = CreateCoordinator();
        var series = new MetricSeriesSelection("Weight", "morning", "Weight", "Morning");

        var result = await coordinator.LoadDistributionAsync(series, From, To, "HealthMetrics");

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data!.Count > 0);
        Assert.Equal("Weight - Morning", result.DisplayName);
        Assert.NotNull(result.RequestSignature);
        Assert.NotNull(result.SnapshotSignature);
        Assert.Equal(ChartProgramKind.Distribution, result.ProgramKind);
        Assert.NotNull(result.ProgramSourceSignature);
        Assert.Null(result.FailureReason);
    }

    [Fact]
    public async Task LoadDistributionAsync_ShouldPreserveSignatureChain()
    {
        var coordinator = CreateCoordinator();
        var series = new MetricSeriesSelection("Weight", "morning", "Weight", "Morning");

        var result = await coordinator.LoadDistributionAsync(series, From, To, "HealthMetrics");

        Assert.True(result.Success);
        Assert.Equal(result.RequestSignature, result.SnapshotSignature);
        Assert.Equal(result.RequestSignature, result.ProgramSourceSignature);
    }

    [Fact]
    public async Task LoadDistributionAsync_ShouldReturnFailureOnException()
    {
        var coordinator = new VNextDistributionIntegrationCoordinator(
            () => throw new InvalidOperationException("Factory failure"));
        var series = new MetricSeriesSelection("Weight", "morning");

        var result = await coordinator.LoadDistributionAsync(series, From, To, "HealthMetrics");

        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.Null(result.CmsSeries);
        Assert.NotNull(result.FailureReason);
        Assert.Contains("Factory failure", result.FailureReason);
    }

    [Fact]
    public async Task LoadDistributionAsync_ShouldReturnFailureForEmptyMetricType()
    {
        var coordinator = CreateCoordinator();
        var series = new MetricSeriesSelection("", "morning");

        var result = await coordinator.LoadDistributionAsync(series, From, To, "HealthMetrics");

        Assert.False(result.Success);
        Assert.NotNull(result.FailureReason);
        Assert.Contains("metric type", result.FailureReason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadDistributionAsync_ShouldUseFreshCoordinatorPerCall()
    {
        var callCount = 0;
        var coordinator = new VNextDistributionIntegrationCoordinator(() =>
        {
            callCount++;
            return CreateStubSessionCoordinator();
        });

        var series = new MetricSeriesSelection("Weight", "morning");
        await coordinator.LoadDistributionAsync(series, From, To, "HealthMetrics");
        await coordinator.LoadDistributionAsync(series, From, To, "HealthMetrics");

        Assert.Equal(2, callCount);
    }

    [Fact]
    public async Task LoadDistributionAsync_SingleSeries_ShouldProduceSingleSeriesData()
    {
        var coordinator = CreateCoordinator();
        var series = new MetricSeriesSelection("Weight", "morning", "Weight", "Morning");

        var result = await coordinator.LoadDistributionAsync(series, From, To, "HealthMetrics");

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data!.Count > 0);
    }

    private static VNextDistributionIntegrationCoordinator CreateCoordinator()
    {
        return new VNextDistributionIntegrationCoordinator(CreateStubSessionCoordinator);
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
}
