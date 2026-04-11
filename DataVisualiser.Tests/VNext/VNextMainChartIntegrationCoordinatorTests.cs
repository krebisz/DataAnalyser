using DataVisualiser.Core.Orchestration;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.State;
using DataVisualiser.VNext.Application;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Kernel;

namespace DataVisualiser.Tests.VNext;

public sealed class VNextMainChartIntegrationCoordinatorTests
{
    private static readonly DateTime From = new(2026, 1, 1);
    private static readonly DateTime To = new(2026, 1, 2);

    [Fact]
    public async Task LoadMainChartAsync_ShouldTranslateRequestAndProjectToLegacyContext()
    {
        var coordinator = CreateCoordinator();
        var request = CreateLoadRequest("Weight", "morning", "evening");

        var result = await coordinator.LoadMainChartAsync(request, MainChartDisplayMode.Regular);

        Assert.True(result.Success);
        Assert.NotNull(result.ProjectedContext);
        Assert.Equal(2, result.ProjectedContext!.ActualSeriesCount);
        Assert.NotNull(result.ProjectedContext.Data1);
        Assert.NotNull(result.ProjectedContext.Data2);
        Assert.Equal("Weight", result.ProjectedContext.MetricType);
        Assert.Equal(From, result.ProjectedContext.From);
        Assert.Equal(To, result.ProjectedContext.To);
    }

    [Fact]
    public async Task LoadMainChartAsync_ShouldPreserveSignaturesThroughProjection()
    {
        var coordinator = CreateCoordinator();
        var request = CreateLoadRequest("Weight", "morning", "evening");

        var result = await coordinator.LoadMainChartAsync(request, MainChartDisplayMode.Regular);

        Assert.True(result.Success);
        Assert.NotNull(result.RequestSignature);
        Assert.NotNull(result.SnapshotSignature);
        Assert.NotNull(result.ProgramSourceSignature);
        Assert.NotNull(result.ProjectedContextSignature);

        // Request and snapshot signatures should match (same request loaded)
        Assert.Equal(result.RequestSignature, result.SnapshotSignature);
        // Program source should trace back to the same request
        Assert.Equal(result.RequestSignature, result.ProgramSourceSignature);
        // Projected context carries the source signature
        Assert.Equal(result.ProgramSourceSignature, result.ProjectedContextSignature);
    }

    [Fact]
    public async Task LoadMainChartAsync_ShouldReturnMainProgramKind()
    {
        var coordinator = CreateCoordinator();
        var request = CreateLoadRequest("Weight", "morning");

        var result = await coordinator.LoadMainChartAsync(request, MainChartDisplayMode.Regular);

        Assert.True(result.Success);
        Assert.Equal(ChartProgramKind.Main, result.ProgramKind);
    }

    [Fact]
    public async Task LoadMainChartAsync_WithSummedMode_ShouldBuildSummedProgram()
    {
        var coordinator = CreateCoordinator();
        var request = CreateLoadRequest("Weight", "morning", "evening");

        var result = await coordinator.LoadMainChartAsync(request, MainChartDisplayMode.Summed);

        Assert.True(result.Success);
        Assert.NotNull(result.ProjectedContext);
        // Summed collapses series to 1
        Assert.Equal(1, result.ProjectedContext!.ActualSeriesCount);
    }

    [Fact]
    public async Task LoadMainChartAsync_ShouldReturnFailureOnException()
    {
        var coordinator = new VNextMainChartIntegrationCoordinator(() => throw new InvalidOperationException("Factory failure"));
        var request = CreateLoadRequest("Weight", "morning");

        var result = await coordinator.LoadMainChartAsync(request, MainChartDisplayMode.Regular);

        Assert.False(result.Success);
        Assert.Null(result.ProjectedContext);
        Assert.NotNull(result.FailureReason);
        Assert.Equal(request.Signature, result.RequestSignature);
        Assert.Contains("Factory failure", result.FailureReason);
    }

    [Fact]
    public async Task LoadMainChartAsync_UseFreshCoordinatorPerCall()
    {
        var callCount = 0;
        var coordinator = new VNextMainChartIntegrationCoordinator(() =>
        {
            callCount++;
            return CreateStubSessionCoordinator();
        });

        var request = CreateLoadRequest("Weight", "morning");
        await coordinator.LoadMainChartAsync(request, MainChartDisplayMode.Regular);
        await coordinator.LoadMainChartAsync(request, MainChartDisplayMode.Regular);

        Assert.Equal(2, callCount);
    }

    [Fact]
    public void TranslateSelections_ShouldConvertLegacySelectionsToVNextRequests()
    {
        var selections = new List<MetricSeriesSelection>
        {
            new("Weight", "morning", "Weight", "Morning"),
            new("Weight", "evening", "Weight", "Evening")
        };

        var requests = VNextMainChartIntegrationCoordinator.TranslateSelections(selections);

        Assert.Equal(2, requests.Count);
        Assert.Equal("Weight", requests[0].MetricType);
        Assert.Equal("morning", requests[0].Subtype);
        Assert.Equal("Weight", requests[0].DisplayMetricType);
        Assert.Equal("Morning", requests[0].DisplaySubtype);
    }

    [Fact]
    public void TranslateDisplayMode_ShouldMapAllModes()
    {
        Assert.Equal(ChartDisplayMode.Regular, VNextMainChartIntegrationCoordinator.TranslateDisplayMode(MainChartDisplayMode.Regular));
        Assert.Equal(ChartDisplayMode.Summed, VNextMainChartIntegrationCoordinator.TranslateDisplayMode(MainChartDisplayMode.Summed));
        Assert.Equal(ChartDisplayMode.Stacked, VNextMainChartIntegrationCoordinator.TranslateDisplayMode(MainChartDisplayMode.Stacked));
    }

    [Fact]
    public async Task LoadMainChartAsync_SingleSeries_ShouldProduceContextWithNullData2()
    {
        var coordinator = CreateCoordinator();
        var request = CreateLoadRequest("Weight", "morning");

        var result = await coordinator.LoadMainChartAsync(request, MainChartDisplayMode.Regular);

        Assert.True(result.Success);
        Assert.NotNull(result.ProjectedContext);
        Assert.Equal(1, result.ProjectedContext!.ActualSeriesCount);
        Assert.NotNull(result.ProjectedContext.Data1);
        Assert.NotNull(result.ProjectedContext.Data2);
        Assert.Empty(result.ProjectedContext.Data2!);
        Assert.NotNull(result.RequestSignature);
        Assert.Equal(result.RequestSignature, result.SnapshotSignature);
    }

    private static VNextMainChartIntegrationCoordinator CreateCoordinator()
    {
        return new VNextMainChartIntegrationCoordinator(CreateStubSessionCoordinator);
    }

    private static ReasoningSessionCoordinator CreateStubSessionCoordinator()
    {
        var loader = new StubMetricSeriesLoader();
        var gateway = new LegacyMetricViewGateway(loader);
        var planner = new ChartProgramPlanner(new TimeSeriesAlignmentKernel(), new OperationKernel());
        var engine = new ReasoningEngine(gateway, planner);
        return new ReasoningSessionCoordinator(engine);
    }

    private static MetricLoadRequest CreateLoadRequest(string metricType, params string[] subtypes)
    {
        var selections = subtypes
            .Select(subtype => new MetricSeriesSelection(metricType, subtype))
            .ToList();

        return new MetricLoadRequest(metricType, selections, From, To, "HealthMetrics");
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
            var value = string.Equals(request.QuerySubtype, "evening", StringComparison.OrdinalIgnoreCase) ? 2m : 1m;
            return Task.FromResult(new LoadedMetricSeries(
                [new MetricData { NormalizedTimestamp = from, Value = value }],
                null));
        }
    }
}
