using DataVisualiser.Shared.Models;
using DataVisualiser.UI.MainHost;
using DataVisualiser.VNext.Application;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Kernel;

namespace DataVisualiser.Tests.VNext;

public sealed class VNextSeriesLoadCoordinatorTests
{
    private static readonly DateTime From = new(2026, 1, 1);
    private static readonly DateTime To = new(2026, 1, 7);

    [Theory]
    [InlineData(ChartProgramKind.WeekdayTrend)]
    [InlineData(ChartProgramKind.BarPie)]
    [InlineData(ChartProgramKind.Distribution)]
    [InlineData(ChartProgramKind.Transform)]
    public async Task LoadAsync_ShouldReturnDataWithCorrectProgramKind(ChartProgramKind kind)
    {
        var coordinator = CreateCoordinator();
        var series = new MetricSeriesSelection("Weight", "morning", "Weight", "Morning");

        var result = await coordinator.LoadAsync(series, From, To, "HealthMetrics", kind);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data!.Count > 0);
        Assert.Equal(kind, result.ProgramKind);
        Assert.Null(result.FailureReason);
    }

    [Fact]
    public async Task LoadAsync_ShouldPreserveSignatureChain()
    {
        var coordinator = CreateCoordinator();
        var series = new MetricSeriesSelection("Weight", "morning", "Weight", "Morning");

        var result = await coordinator.LoadAsync(series, From, To, "HealthMetrics", ChartProgramKind.WeekdayTrend);

        Assert.True(result.Success);
        Assert.Equal(result.RequestSignature, result.SnapshotSignature);
        Assert.Equal(result.RequestSignature, result.ProgramSourceSignature);
    }

    [Fact]
    public async Task LoadAsync_ShouldReturnFailureOnException()
    {
        var coordinator = new VNextSeriesLoadCoordinator(
            () => throw new InvalidOperationException("Factory failure"));
        var series = new MetricSeriesSelection("Weight", "morning");

        var result = await coordinator.LoadAsync(series, From, To, "HealthMetrics", ChartProgramKind.BarPie);

        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.Equal(ChartProgramKind.BarPie, result.ProgramKind);
        Assert.Contains("Factory failure", result.FailureReason);
    }

    [Fact]
    public async Task LoadAsync_ShouldReturnFailureForEmptyMetricType()
    {
        var coordinator = CreateCoordinator();
        var series = new MetricSeriesSelection("", "morning");

        var result = await coordinator.LoadAsync(series, From, To, "HealthMetrics", ChartProgramKind.WeekdayTrend);

        Assert.False(result.Success);
        Assert.Contains("metric type", result.FailureReason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_ShouldUseFreshCoordinatorPerCall()
    {
        var callCount = 0;
        var coordinator = new VNextSeriesLoadCoordinator(() =>
        {
            callCount++;
            return CreateStubSessionCoordinator();
        });

        var series = new MetricSeriesSelection("Weight", "morning");
        await coordinator.LoadAsync(series, From, To, "HealthMetrics", ChartProgramKind.WeekdayTrend);
        await coordinator.LoadAsync(series, From, To, "HealthMetrics", ChartProgramKind.BarPie);

        Assert.Equal(2, callCount);
    }

    private static VNextSeriesLoadCoordinator CreateCoordinator()
    {
        return new VNextSeriesLoadCoordinator(CreateStubSessionCoordinator);
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
            MetricSeriesRequest request, DateTime from, DateTime to,
            string resolutionTableName, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new LoadedMetricSeries(
                [new MetricData { NormalizedTimestamp = from, Value = 1m }], null));
        }
    }
}
