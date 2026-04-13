using DataVisualiser.Shared.Models;
using DataVisualiser.UI.SyncfusionViews;

namespace DataVisualiser.Tests.UI.Syncfusion;

public sealed class SyncfusionChartsViewLoadCoordinatorTests
{
    [Fact]
    public void ValidateAndPrepareLoad_ShouldWarnWhenMetricTypeMissing()
    {
        var coordinator = new SyncfusionChartsViewLoadCoordinator();
        var warnings = new List<string>();

        var isValid = coordinator.ValidateAndPrepareLoad(
            new SyncfusionChartsViewLoadCoordinator.LoadValidationInput(null, new DateTime(2026, 1, 1), new DateTime(2026, 1, 2)),
            new SyncfusionChartsViewLoadCoordinator.ValidationActions(
                () => NoOpDisposable.Instance,
                _ => { },
                () => { },
                (_, _) => { },
                () => (true, null),
                (title, message) => warnings.Add($"{title}|{message}")));

        Assert.False(isValid);
        Assert.Equal(["No Selection|Please select a Metric Type"], warnings);
    }

    [Fact]
    public void ValidateAndPrepareLoad_ShouldBatchSelectionStateAndValidateRequirements()
    {
        var coordinator = new SyncfusionChartsViewLoadCoordinator();
        var calls = new List<string>();

        var isValid = coordinator.ValidateAndPrepareLoad(
            new SyncfusionChartsViewLoadCoordinator.LoadValidationInput("Weight", new DateTime(2026, 1, 1), new DateTime(2026, 1, 2)),
            new SyncfusionChartsViewLoadCoordinator.ValidationActions(
                () =>
                {
                    calls.Add("begin");
                    return new CallbackDisposable(() => calls.Add("end"));
                },
                metricType => calls.Add($"metric:{metricType}"),
                () => calls.Add("subtypes"),
                (from, to) => calls.Add($"dates:{from:yyyyMMdd}-{to:yyyyMMdd}"),
                () =>
                {
                    calls.Add("validate");
                    return (true, null);
                },
                (_, _) => calls.Add("warn")));

        Assert.True(isValid);
        Assert.Equal(
            ["begin", "metric:Weight", "subtypes", "dates:20260101-20260102", "end", "validate"],
            calls);
    }

    [Fact]
    public async Task ExecuteLoadAsync_ShouldPublishOnSuccess()
    {
        var coordinator = new SyncfusionChartsViewLoadCoordinator();
        var calls = new List<string>();

        await coordinator.ExecuteLoadAsync(
            new SyncfusionChartsViewLoadCoordinator.LoadExecutionActions(
                () =>
                {
                    calls.Add("load");
                    return Task.FromResult(true);
                },
                () => calls.Add("reset"),
                () => calls.Add("publish"),
                (_, _) => calls.Add("error")));

        Assert.Equal(["load", "publish"], calls);
    }

    [Fact]
    public async Task ExecuteLoadAsync_ShouldResetStateAndShowErrorOnException()
    {
        var coordinator = new SyncfusionChartsViewLoadCoordinator();
        var calls = new List<string>();

        await coordinator.ExecuteLoadAsync(
            new SyncfusionChartsViewLoadCoordinator.LoadExecutionActions(
                () => throw new InvalidOperationException("boom"),
                () => calls.Add("reset"),
                () => calls.Add("publish"),
                (title, message) => calls.Add($"{title}|{message}")));

        Assert.Equal(["Error|Error loading data: boom", "reset"], calls);
    }

    [Fact]
    public void ClearSelection_ShouldResetStateAndResolution()
    {
        var coordinator = new SyncfusionChartsViewLoadCoordinator();
        var calls = new List<string>();

        coordinator.ClearSelection(
            "All",
            isDefaultResolutionSelected: false,
            new SyncfusionChartsViewLoadCoordinator.ClearActions(
                () => calls.Add("clearEvidence"),
                selections => calls.Add($"series:{selections.Count}"),
                () => calls.Add("resetLastContext"),
                () => calls.Add("clearChart"),
                () => calls.Add("updateToggle"),
                resolution => calls.Add($"reset:{resolution}"),
                resolution => calls.Add($"select:{resolution}")));

        Assert.Equal(
            ["clearEvidence", "series:0", "resetLastContext", "clearChart", "updateToggle", "select:All"],
            calls);
    }

    private sealed class CallbackDisposable(Action callback) : IDisposable
    {
        public void Dispose()
        {
            callback();
        }
    }

    private sealed class NoOpDisposable : IDisposable
    {
        public static readonly NoOpDisposable Instance = new();

        public void Dispose()
        {
        }
    }
}
