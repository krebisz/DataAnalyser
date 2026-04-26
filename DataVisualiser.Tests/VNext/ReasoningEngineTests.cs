using DataVisualiser.Shared.Models;
using DataVisualiser.VNext.Application;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Kernel;

namespace DataVisualiser.Tests.VNext;

public sealed class ReasoningEngineTests
{
    [Fact]
    public async Task LoadAsync_ThenBuildMainProgram_ShouldStayOnSingleRequestSignature()
    {
        var engine = new ReasoningEngine(
            new LegacyMetricViewGateway(new StubMetricSeriesLoader()),
            new ChartProgramPlanner(new TimeSeriesAlignmentKernel(), new OperationKernel()));

        var request = new MetricSelectionRequest(
            "Weight",
            [new MetricSeriesRequest("Weight", "morning"), new MetricSeriesRequest("Weight", "evening")],
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 2),
            "HealthMetrics");

        var snapshot = await engine.LoadAsync(request);
        var program = engine.BuildMainProgram(snapshot);

        Assert.Equal(request.Signature, snapshot.Signature);
        Assert.Equal(request.Signature, program.SourceSignature);
        Assert.Equal(2, program.Series.Count);
    }

    [Fact]
    public async Task BuildProgram_ShouldUseExplicitProgramRequest()
    {
        var engine = new ReasoningEngine(
            new LegacyMetricViewGateway(new StubMetricSeriesLoader()),
            new ChartProgramPlanner(new TimeSeriesAlignmentKernel(), new OperationKernel()));

        var request = new MetricSelectionRequest(
            "Weight",
            [new MetricSeriesRequest("Weight", "morning"), new MetricSeriesRequest("Weight", "evening")],
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 2),
            "HealthMetrics");

        var snapshot = await engine.LoadAsync(request);
        var program = engine.BuildProgram(snapshot, ChartProgramRequest.Ratio());

        Assert.Equal(ChartProgramKind.Ratio, program.Kind);
        Assert.Equal(request.Signature, program.SourceSignature);
    }

    [Fact]
    public async Task BuildProgram_Transform_ShouldPreserveSnapshotSignatureAndBuildMultipleSeries()
    {
        var engine = new ReasoningEngine(
            new LegacyMetricViewGateway(new StubMetricSeriesLoader()),
            new ChartProgramPlanner(new TimeSeriesAlignmentKernel(), new OperationKernel()));

        var request = new MetricSelectionRequest(
            "Weight",
            [new MetricSeriesRequest("Weight", "morning"), new MetricSeriesRequest("Weight", "evening")],
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 2),
            "HealthMetrics");

        var snapshot = await engine.LoadAsync(request);
        var program = engine.BuildProgram(
            snapshot,
            ChartProgramRequest.Transform(
                "Weight transform",
                [
                    SeriesOperationRequest.Normalize(0, "morning-normalized", "Morning normalized"),
                    SeriesOperationRequest.Difference(0, 1, "Delta")
                ]));

        Assert.Equal(ChartProgramKind.Transform, program.Kind);
        Assert.Equal(request.Signature, program.SourceSignature);
        Assert.Equal("Weight transform", program.Title);
        Assert.Equal(2, program.Series.Count);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLoadAndBuildProgramFromAnalyticalIntent()
    {
        var engine = new ReasoningEngine(
            new LegacyMetricViewGateway(new StubMetricSeriesLoader()),
            new ChartProgramPlanner(new TimeSeriesAlignmentKernel(), new OperationKernel()));
        var request = new MetricSelectionRequest(
            "Weight",
            [new MetricSeriesRequest("Weight", "morning"), new MetricSeriesRequest("Weight", "evening")],
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 2),
            "HealthMetrics");
        var intent = AnalyticalIntent.FromRequests(
            request,
            ChartProgramRequest.Normalized(),
            ConsumerDeliveryContract.Chart(ChartProgramKind.Normalized, "NormalizedChart"));

        var result = await engine.ExecuteAsync(intent);

        Assert.Same(intent, result.Intent);
        Assert.Equal(request.Signature, result.Snapshot.Signature);
        Assert.Equal(ChartProgramKind.Normalized, result.Program.Kind);
        Assert.Equal(request.Signature, result.Program.SourceSignature);
        Assert.Contains(intent.Signature, result.Signature, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExecuteAsync_WithIntentSet_ShouldLoadOnceAndBuildMultiplePrograms()
    {
        var loader = new StubMetricSeriesLoader();
        var engine = new ReasoningEngine(
            new LegacyMetricViewGateway(loader),
            new ChartProgramPlanner(new TimeSeriesAlignmentKernel(), new OperationKernel()));
        var request = new MetricSelectionRequest(
            "Weight",
            [new MetricSeriesRequest("Weight", "morning"), new MetricSeriesRequest("Weight", "evening")],
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 2),
            "HealthMetrics");
        var intentSet = AnalyticalIntentSet.FromIntents(
        [
            AnalyticalIntent.FromRequests(request, ChartProgramRequest.MainProgram()),
            AnalyticalIntent.FromRequests(request, ChartProgramRequest.Normalized())
        ]);

        var resultSet = await engine.ExecuteAsync(intentSet);

        Assert.Equal(request.Signature, resultSet.Selection.Signature);
        Assert.Equal([ChartProgramKind.Main, ChartProgramKind.Normalized], resultSet.ProgramKinds);
        Assert.All(resultSet.Results, result => Assert.Equal(request.Signature, result.Snapshot.Signature));
        Assert.Equal(2, loader.LoadCallCount);
    }

    private sealed class StubMetricSeriesLoader : IMetricSeriesLoader
    {
        public int LoadCallCount { get; private set; }

        public Task<LoadedMetricSeries> LoadAsync(
            MetricSeriesRequest request,
            DateTime from,
            DateTime to,
            string resolutionTableName,
            CancellationToken cancellationToken = default)
        {
            LoadCallCount++;
            var value = string.Equals(request.QuerySubtype, "evening", StringComparison.OrdinalIgnoreCase) ? 2m : 1m;
            return Task.FromResult(new LoadedMetricSeries(
                [new MetricData { NormalizedTimestamp = from, Value = value }],
                null));
        }
    }
}
