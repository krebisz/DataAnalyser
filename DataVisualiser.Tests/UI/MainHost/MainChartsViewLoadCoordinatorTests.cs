using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.Charts.Presentation;

namespace DataVisualiser.Tests.UI.MainHost;

public sealed class MainChartsViewLoadCoordinatorTests
{
    [Fact]
    public void ValidateAndPrepareLoad_ShouldWarnWhenMetricTypeMissing()
    {
        var coordinator = new MainChartsViewLoadCoordinator();
        var warnings = new List<string>();

        var isValid = coordinator.ValidateAndPrepareLoad(
            new MainChartsViewLoadCoordinator.LoadValidationInput(null, new DateTime(2026, 1, 1), new DateTime(2026, 1, 2)),
            new MainChartsViewLoadCoordinator.ValidationActions(
                () => NoOpDisposable.Instance,
                _ => { },
                () => { },
                (_, _) => { },
                () => { },
                () => (true, null),
                (title, message) => warnings.Add($"{title}|{message}")));

        Assert.False(isValid);
        Assert.Equal(["No Selection|Please select a Metric Type"], warnings);
    }

    [Fact]
    public void ValidateAndPrepareLoad_ShouldBatchSelectionStateAndValidateRequirements()
    {
        var coordinator = new MainChartsViewLoadCoordinator();
        var calls = new List<string>();

        var isValid = coordinator.ValidateAndPrepareLoad(
            new MainChartsViewLoadCoordinator.LoadValidationInput("Weight", new DateTime(2026, 1, 1), new DateTime(2026, 1, 2)),
            new MainChartsViewLoadCoordinator.ValidationActions(
                () =>
                {
                    calls.Add("begin");
                    return new CallbackDisposable(() => calls.Add("end"));
                },
                metricType => calls.Add($"metric:{metricType}"),
                () => calls.Add("subtypes"),
                (from, to) => calls.Add($"dates:{from:yyyyMMdd}-{to:yyyyMMdd}"),
                () => calls.Add("titles"),
                () =>
                {
                    calls.Add("validate");
                    return (true, null);
                },
                (_, _) => calls.Add("warn")));

        Assert.True(isValid);
        Assert.Equal(
            ["begin", "metric:Weight", "subtypes", "dates:20260101-20260102", "end", "titles", "validate"],
            calls);
    }

    [Fact]
    public async Task ExecuteLoadAsync_ShouldClearRelevantCachesAndPublishOnSuccess()
    {
        var coordinator = new MainChartsViewLoadCoordinator();
        var calls = new List<string>();

        await coordinator.ExecuteLoadAsync(
            new MainChartsViewLoadCoordinator.LoadExecutionActions(
                key => calls.Add($"clear:{key}"),
                () => calls.Add("resetTransform"),
                () =>
                {
                    calls.Add("load");
                    return Task.FromResult(true);
                },
                () => calls.Add("clearAll"),
                () => calls.Add("clearHidden"),
                () => calls.Add("publish"),
                (_, _) => calls.Add("error")));

        Assert.Equal(
            [
                $"clear:{ChartControllerKeys.Distribution}",
                $"clear:{ChartControllerKeys.WeeklyTrend}",
                $"clear:{ChartControllerKeys.Normalized}",
                $"clear:{ChartControllerKeys.DiffRatio}",
                $"clear:{ChartControllerKeys.Transform}",
                "resetTransform",
                "load",
                "clearHidden",
                "publish"
            ],
            calls);
    }

    [Fact]
    public async Task ExecuteLoadAsync_ShouldClearAllChartsAndShowErrorOnException()
    {
        var coordinator = new MainChartsViewLoadCoordinator();
        var calls = new List<string>();

        await coordinator.ExecuteLoadAsync(
            new MainChartsViewLoadCoordinator.LoadExecutionActions(
                _ => { },
                () => { },
                () => throw new InvalidOperationException("boom"),
                () => calls.Add("clearAll"),
                () => calls.Add("clearHidden"),
                () => calls.Add("publish"),
                (title, message) => calls.Add($"{title}|{message}")));

        Assert.Equal(["Error|Error loading data: boom", "clearAll"], calls);
    }

    [Fact]
    public void ClearSelection_ShouldResetStateAndResolution()
    {
        var coordinator = new MainChartsViewLoadCoordinator();
        var calls = new List<string>();

        coordinator.ClearSelection(
            "All",
            isDefaultResolutionSelected: false,
            new MainChartsViewLoadCoordinator.ClearActions(
                (kind, level, message) => calls.Add($"{kind}|{level}|{message}"),
                () => calls.Add("clearEvidence"),
                selections => calls.Add($"series:{selections.Count}"),
                () => calls.Add("resetLastContext"),
                count => calls.Add($"primary:{count}"),
                count => calls.Add($"secondary:{count}"),
                resolution => calls.Add($"reset:{resolution}"),
                resolution => calls.Add($"select:{resolution}")));

        Assert.Equal(
            [
                "ClearInvoked|Info|User cleared current selection and chart state.",
                "clearEvidence",
                "series:0",
                "resetLastContext",
                "primary:0",
                "secondary:0",
                "select:All"
            ],
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
