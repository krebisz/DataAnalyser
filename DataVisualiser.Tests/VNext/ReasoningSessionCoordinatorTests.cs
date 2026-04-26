using DataVisualiser.Shared.Models;
using DataVisualiser.VNext.Application;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.State;

namespace DataVisualiser.Tests.VNext;

public sealed class ReasoningSessionCoordinatorTests
{
    [Fact]
    public async Task LoadAsync_ShouldPromoteLoadedSnapshotIntoSessionState()
    {
        var coordinator = new ReasoningSessionCoordinator(new StubReasoningEngine());
        coordinator.ApplyMetricType("Weight");
        coordinator.ApplyResolution("HealthMetrics");
        coordinator.ApplyDateRange(new DateTime(2026, 1, 1), new DateTime(2026, 1, 2));
        coordinator.ApplySeries([new MetricSeriesRequest("Weight", "morning")]);

        var snapshot = await coordinator.LoadAsync();

        Assert.Equal(snapshot.Signature, coordinator.State.Load.Snapshot?.Signature);
        Assert.Equal(LoadLifecycle.Loaded, coordinator.State.Load.Lifecycle);
    }

    [Fact]
    public async Task BuildMainProgram_ShouldUseCurrentPresentationMode()
    {
        var coordinator = new ReasoningSessionCoordinator(new StubReasoningEngine());
        coordinator.ApplyMetricType("Weight");
        coordinator.ApplyResolution("HealthMetrics");
        coordinator.ApplyDateRange(new DateTime(2026, 1, 1), new DateTime(2026, 1, 2));
        coordinator.ApplySeries(
        [
            new MetricSeriesRequest("Weight", "morning"),
            new MetricSeriesRequest("Weight", "evening")
        ]);
        coordinator.ApplyMainDisplayMode(ChartDisplayMode.Summed);

        await coordinator.LoadAsync();
        var program = coordinator.BuildMainProgram();

        Assert.Equal(ChartDisplayMode.Summed, program.DisplayMode);
        Assert.Single(program.Series);
    }

    [Fact]
    public async Task BuildProgram_ShouldRouteThroughExplicitProgramRequest()
    {
        var coordinator = new ReasoningSessionCoordinator(new StubReasoningEngine());
        coordinator.ApplyMetricType("Weight");
        coordinator.ApplyResolution("HealthMetrics");
        coordinator.ApplyDateRange(new DateTime(2026, 1, 1), new DateTime(2026, 1, 2));
        coordinator.ApplySeries(
        [
            new MetricSeriesRequest("Weight", "morning"),
            new MetricSeriesRequest("Weight", "evening")
        ]);

        await coordinator.LoadAsync();
        var program = coordinator.BuildProgram(ChartProgramRequest.Difference());

        Assert.Equal(ChartProgramKind.Difference, program.Kind);
        Assert.Single(program.Series);
    }

    [Fact]
    public async Task BuildWorkflowProgram_ShouldUsePlannedOperationsAndCurrentDisplayMode()
    {
        var coordinator = new ReasoningSessionCoordinator(new StubReasoningEngine());
        coordinator.ApplyMetricType("Weight");
        coordinator.ApplyResolution("HealthMetrics");
        coordinator.ApplyDateRange(new DateTime(2026, 1, 1), new DateTime(2026, 1, 2));
        coordinator.ApplySeries(
        [
            new MetricSeriesRequest("Weight", "morning"),
            new MetricSeriesRequest("Weight", "evening")
        ]);
        coordinator.ApplyMainDisplayMode(ChartDisplayMode.Summed);
        coordinator.ApplyWorkflowPlan(new WorkflowPlanRequest(
        [
            SeriesOperationRequest.Normalize(0, "morning-normalized", "Morning normalized"),
            SeriesOperationRequest.Difference(0, 1, "Delta")
        ],
        "transform",
        "Weight transform"));

        await coordinator.LoadAsync();
        var program = coordinator.BuildWorkflowProgram();

        Assert.Equal(ChartProgramKind.Transform, program.Kind);
        Assert.Equal(ChartDisplayMode.Summed, program.DisplayMode);
        Assert.Equal("Weight transform", program.Title);
        Assert.Equal(2, program.Series.Count);
    }

    [Fact]
    public void Clear_ShouldResetSelectionAndLoad()
    {
        var coordinator = new ReasoningSessionCoordinator(new StubReasoningEngine());
        coordinator.ApplyMetricType("Weight");
        coordinator.ApplyResolution("HealthMetrics");
        coordinator.ApplyDateRange(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);
        coordinator.ApplySeries([new MetricSeriesRequest("Weight", "morning")]);

        coordinator.Clear();

        Assert.Equal(LoadLifecycle.Empty, coordinator.State.Load.Lifecycle);
        Assert.False(coordinator.State.Selection.IsComplete);
    }

    [Fact]
    public async Task ExecuteAsync_WithIntentSet_ShouldDelegateToReasoningEngine()
    {
        var coordinator = new ReasoningSessionCoordinator(new StubReasoningEngine());
        var request = new MetricSelectionRequest(
            "Weight",
            [new MetricSeriesRequest("Weight", "morning"), new MetricSeriesRequest("Weight", "evening")],
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 2),
            "HealthMetrics");
        var intentSet = AnalyticalIntentSet.FromIntents(
        [
            AnalyticalIntent.FromRequests(request, ChartProgramRequest.MainProgram()),
            AnalyticalIntent.FromRequests(request, ChartProgramRequest.Difference())
        ]);

        var resultSet = await coordinator.ExecuteAsync(intentSet);

        Assert.Equal(request.Signature, resultSet.Selection.Signature);
        Assert.Equal([ChartProgramKind.Main, ChartProgramKind.Difference], resultSet.ProgramKinds);
    }

    private sealed class StubReasoningEngine : IReasoningEngine
    {
        public Task<MetricLoadSnapshot> LoadAsync(MetricSelectionRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new MetricLoadSnapshot(
                request,
                request.Series.Select(series => new MetricSeriesSnapshot(
                    series,
                    [new MetricData { NormalizedTimestamp = request.From, Value = 1m }],
                    null)).ToArray(),
                DateTime.UtcNow));
        }

        public async Task<AnalyticalExecutionResult> ExecuteAsync(AnalyticalIntent intent, CancellationToken cancellationToken = default)
        {
            var snapshot = await LoadAsync(intent.Selection, cancellationToken);
            var program = BuildProgram(snapshot, intent);
            return new AnalyticalExecutionResult(intent, snapshot, program);
        }

        public async Task<AnalyticalResultSet> ExecuteAsync(AnalyticalIntentSet intentSet, CancellationToken cancellationToken = default)
        {
            var snapshot = await LoadAsync(intentSet.Selection, cancellationToken);
            var results = intentSet.Intents
                .Select(intent => new AnalyticalExecutionResult(intent, snapshot, BuildProgram(snapshot, intent)))
                .ToArray();

            return new AnalyticalResultSet(intentSet.Selection, results);
        }

        public ChartProgram BuildProgram(MetricLoadSnapshot snapshot, ChartProgramRequest request)
        {
            return request.Kind switch
            {
                ChartProgramKind.Main => BuildMainProgram(snapshot, request.DisplayMode),
                ChartProgramKind.Normalized => BuildNormalizedProgram(snapshot),
                ChartProgramKind.Difference => BuildDifferenceProgram(snapshot),
                ChartProgramKind.Ratio => BuildRatioProgram(snapshot),
                ChartProgramKind.Transform => new ChartProgram(
                    ChartProgramKind.Transform,
                    request.DisplayMode,
                    request.TitleOverride ?? "Derived",
                    snapshot.Request.From,
                    snapshot.Request.To,
                    [snapshot.Request.From],
                    request.SeriesOperations.Select(operation => new ChartSeriesProgram(
                        operation.Id,
                        operation.Label,
                        [1d],
                        [1d])).ToArray(),
                    snapshot.Signature),
                _ => throw new InvalidOperationException()
            };
        }

        public ChartProgram BuildProgram(MetricLoadSnapshot snapshot, AnalyticalIntent intent)
        {
            return BuildProgram(snapshot, intent.ProgramRequest);
        }

        public ChartProgram BuildMainProgram(MetricLoadSnapshot snapshot, ChartDisplayMode displayMode = ChartDisplayMode.Regular)
        {
            var series = displayMode == ChartDisplayMode.Summed
                ? new[] { new ChartSeriesProgram("sum", "Sum", [1d], [1d]) }
                : snapshot.Series.Select(series => new ChartSeriesProgram(series.Request.SignatureToken, series.Request.DisplayName, [1d], [1d])).ToArray();

            return new ChartProgram(ChartProgramKind.Main, displayMode, snapshot.Request.MetricType, snapshot.Request.From, snapshot.Request.To, [snapshot.Request.From], series, snapshot.Signature);
        }

        public ChartProgram BuildNormalizedProgram(MetricLoadSnapshot snapshot)
        {
            return new ChartProgram(ChartProgramKind.Normalized, ChartDisplayMode.Regular, snapshot.Request.MetricType, snapshot.Request.From, snapshot.Request.To, [snapshot.Request.From], [new ChartSeriesProgram("n", "N", [1d], [1d])], snapshot.Signature);
        }

        public ChartProgram BuildDifferenceProgram(MetricLoadSnapshot snapshot)
        {
            return new ChartProgram(ChartProgramKind.Difference, ChartDisplayMode.Regular, snapshot.Request.MetricType, snapshot.Request.From, snapshot.Request.To, [snapshot.Request.From], [new ChartSeriesProgram("d", "D", [1d], [1d])], snapshot.Signature);
        }

        public ChartProgram BuildRatioProgram(MetricLoadSnapshot snapshot)
        {
            return new ChartProgram(ChartProgramKind.Ratio, ChartDisplayMode.Regular, snapshot.Request.MetricType, snapshot.Request.From, snapshot.Request.To, [snapshot.Request.From], [new ChartSeriesProgram("r", "R", [1d], [1d])], snapshot.Signature);
        }
    }
}
