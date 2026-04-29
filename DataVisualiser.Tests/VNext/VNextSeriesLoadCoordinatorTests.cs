using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.Tests.Helpers.Infrastructure;
using DataVisualiser.UI.MainHost;
using DataVisualiser.VNext.Application;
using DataVisualiser.VNext.Contracts;

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
    [InlineData(ChartProgramKind.SyncfusionSunburst)]
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
    public async Task LoadAsync_ForDistribution_ShouldPreserveProgramKindAndDataLineage()
    {
        var coordinator = CreateCoordinator();
        var series = new MetricSeriesSelection("Weight", "morning", "Weight", "Morning");

        var result = await coordinator.LoadAsync(series, From, To, "HealthMetrics", ChartProgramKind.Distribution);

        Assert.True(result.Success);
        Assert.Equal(ChartProgramKind.Distribution, result.ProgramKind);
        Assert.NotNull(result.ProgramSourceSignature);
        Assert.Contains("Weight::HealthMetrics", result.ProgramSourceSignature, StringComparison.Ordinal);
        Assert.Contains("Weight:morning", result.ProgramSourceSignature, StringComparison.Ordinal);
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
            return VNextTestStubs.CreateSessionCoordinator();
        });

        var series = new MetricSeriesSelection("Weight", "morning");
        await coordinator.LoadAsync(series, From, To, "HealthMetrics", ChartProgramKind.WeekdayTrend);
        await coordinator.LoadAsync(series, From, To, "HealthMetrics", ChartProgramKind.BarPie);

        Assert.Equal(2, callCount);
    }

    private static VNextSeriesLoadCoordinator CreateCoordinator()
    {
        return new VNextSeriesLoadCoordinator(VNextTestStubs.CreateSessionCoordinator);
    }
}
