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
