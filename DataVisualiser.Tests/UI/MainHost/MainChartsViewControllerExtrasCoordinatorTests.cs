using DataVisualiser.Core.Orchestration;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.State;

namespace DataVisualiser.Tests.UI.MainHost;

public sealed class MainChartsViewControllerExtrasCoordinatorTests
{
    [Fact]
    public void InitializeControls_ShouldDelegateToBarPieDistributionAndWeekdayControllers()
    {
        var coordinator = new MainChartsViewControllerExtrasCoordinator();
        var controllers = CreateControllers();

        coordinator.InitializeBarPieControls(CreateActions(key => controllers[key]));
        coordinator.InitializeDistributionControls(CreateActions(key => controllers[key]));
        coordinator.InitializeWeekdayTrendControls(CreateActions(key => controllers[key]));

        Assert.True(controllers[ChartControllerKeys.BarPie].InitializeControlsCalled);
        Assert.True(controllers[ChartControllerKeys.Distribution].InitializeControlsCalled);
        Assert.True(controllers[ChartControllerKeys.WeeklyTrend].InitializeControlsCalled);
    }

    [Fact]
    public void TransformActions_ShouldDelegateToTransformExtras()
    {
        var coordinator = new MainChartsViewControllerExtrasCoordinator();
        var controllers = CreateControllers();
        var context = new ChartDataContext
        {
            Data1 = [new MetricData { NormalizedTimestamp = DateTime.Today, Value = 1m }]
        };

        coordinator.CompleteTransformSelectionsPendingLoad(CreateActions(key => controllers[key]));
        coordinator.ResetTransformSelectionsPendingLoad(CreateActions(key => controllers[key]));
        coordinator.HandleTransformVisibilityOnlyToggle(context, CreateActions(key => controllers[key]));
        coordinator.UpdateTransformSubtypeOptions(CreateActions(key => controllers[key]));
        coordinator.UpdateTransformComputeButtonState(CreateActions(key => controllers[key]));

        var transform = controllers[ChartControllerKeys.Transform];
        Assert.True(transform.CompleteSelectionsPendingLoadCalled);
        Assert.True(transform.ResetSelectionsPendingLoadCalled);
        Assert.Same(context, transform.LastVisibilityOnlyContext);
        Assert.True(transform.UpdateTransformSubtypeOptionsCalled);
        Assert.True(transform.UpdateTransformComputeButtonStateCalled);
    }

    [Fact]
    public void MainAndDiffRatioActions_ShouldDelegateToSpecializedExtras()
    {
        var coordinator = new MainChartsViewControllerExtrasCoordinator();
        var controllers = CreateControllers();

        coordinator.SyncMainDisplayModeSelection(CreateActions(key => controllers[key]));
        coordinator.UpdateDiffRatioOperationButton(CreateActions(key => controllers[key]));
        coordinator.UpdateDistributionChartTypeVisibility(CreateActions(key => controllers[key]));
        coordinator.UpdateWeekdayTrendChartTypeVisibility(CreateActions(key => controllers[key]));

        Assert.True(controllers[ChartControllerKeys.Main].SyncDisplayModeSelectionCalled);
        Assert.True(controllers[ChartControllerKeys.DiffRatio].UpdateOperationButtonCalled);
        Assert.True(controllers[ChartControllerKeys.Distribution].UpdateChartTypeVisibilityCalled);
        Assert.True(controllers[ChartControllerKeys.WeeklyTrend].UpdateChartTypeVisibilityCalled);
    }

    [Fact]
    public void MissingExtras_ShouldBeNoOp()
    {
        var coordinator = new MainChartsViewControllerExtrasCoordinator();
        var controllers = new Dictionary<string, IChartController>
        {
            [ChartControllerKeys.BarPie] = new PlainChartController(),
            [ChartControllerKeys.Distribution] = new PlainChartController(),
            [ChartControllerKeys.WeeklyTrend] = new PlainChartController(),
            [ChartControllerKeys.Transform] = new PlainChartController(),
            [ChartControllerKeys.DiffRatio] = new PlainChartController(),
            [ChartControllerKeys.Main] = new PlainChartController()
        };

        coordinator.InitializeBarPieControls(CreateActions(key => controllers[key]));
        coordinator.InitializeDistributionControls(CreateActions(key => controllers[key]));
        coordinator.InitializeWeekdayTrendControls(CreateActions(key => controllers[key]));
        coordinator.CompleteTransformSelectionsPendingLoad(CreateActions(key => controllers[key]));
        coordinator.ResetTransformSelectionsPendingLoad(CreateActions(key => controllers[key]));
        coordinator.HandleTransformVisibilityOnlyToggle(null, CreateActions(key => controllers[key]));
        coordinator.UpdateTransformSubtypeOptions(CreateActions(key => controllers[key]));
        coordinator.UpdateTransformComputeButtonState(CreateActions(key => controllers[key]));
        coordinator.UpdateDiffRatioOperationButton(CreateActions(key => controllers[key]));
        coordinator.SyncMainDisplayModeSelection(CreateActions(key => controllers[key]));
        coordinator.UpdateDistributionChartTypeVisibility(CreateActions(key => controllers[key]));
        coordinator.UpdateWeekdayTrendChartTypeVisibility(CreateActions(key => controllers[key]));
    }

    private static MainChartsViewControllerExtrasCoordinator.Actions CreateActions(
        Func<string, IChartController> resolveController)
    {
        return new MainChartsViewControllerExtrasCoordinator.Actions(resolveController);
    }

    private static Dictionary<string, FakeController> CreateControllers()
    {
        return new Dictionary<string, FakeController>
        {
            [ChartControllerKeys.BarPie] = new FakeController(),
            [ChartControllerKeys.Distribution] = new FakeController(),
            [ChartControllerKeys.WeeklyTrend] = new FakeController(),
            [ChartControllerKeys.Transform] = new FakeController(),
            [ChartControllerKeys.DiffRatio] = new FakeController(),
            [ChartControllerKeys.Main] = new FakeController()
        };
    }

    private sealed class FakeController :
        IChartController,
        IBarPieChartControllerExtras,
        IDistributionChartControllerExtras,
        IWeekdayTrendChartControllerExtras,
        ITransformPanelControllerExtras,
        IDiffRatioChartControllerExtras,
        IMainChartControllerExtras
    {
        public bool InitializeControlsCalled { get; private set; }
        public bool UpdateChartTypeVisibilityCalled { get; private set; }
        public bool CompleteSelectionsPendingLoadCalled { get; private set; }
        public bool ResetSelectionsPendingLoadCalled { get; private set; }
        public ChartDataContext? LastVisibilityOnlyContext { get; private set; }
        public bool UpdateTransformSubtypeOptionsCalled { get; private set; }
        public bool UpdateTransformComputeButtonStateCalled { get; private set; }
        public bool UpdateOperationButtonCalled { get; private set; }
        public bool SyncDisplayModeSelectionCalled { get; private set; }

        public string Key { get; init; } = string.Empty;
        public bool RequiresPrimaryData => false;
        public bool RequiresSecondaryData => false;
        public void Initialize() { }
        public Task RenderAsync(ChartDataContext context) => Task.CompletedTask;
        public void Clear(ChartState state) { }
        public void ResetZoom() { }
        public bool HasSeries(ChartState state) => false;
        public void UpdateSubtypeOptions() { }
        public void ClearCache() { }
        public void SetVisible(bool isVisible) { }
        public void SetTitle(string? title) { }
        public void SetToggleEnabled(bool enabled) { }

        public void InitializeControls() => InitializeControlsCalled = true;
        public Task RenderIfVisibleAsync() => Task.CompletedTask;
        public void SelectBucketCount(int bucketCount) { }
        public string GetDisplayMode() => string.Empty;
        public void UpdateChartTypeVisibility() => UpdateChartTypeVisibilityCalled = true;
        public void CompleteSelectionsPendingLoad() => CompleteSelectionsPendingLoadCalled = true;
        public void ResetSelectionsPendingLoad() => ResetSelectionsPendingLoadCalled = true;
        public void HandleVisibilityOnlyToggle(ChartDataContext? context) => LastVisibilityOnlyContext = context;
        public void UpdateTransformSubtypeOptions() => UpdateTransformSubtypeOptionsCalled = true;
        public void UpdateTransformComputeButtonState() => UpdateTransformComputeButtonStateCalled = true;
        public string? GetSelectedOperationTag() => null;
        public void UpdateOperationButton() => UpdateOperationButtonCalled = true;
        public void SyncDisplayModeSelection() => SyncDisplayModeSelectionCalled = true;
        public void SetStackedAvailability(bool canStack) { }
    }

    private sealed class PlainChartController : IChartController
    {
        public string Key { get; init; } = string.Empty;
        public bool RequiresPrimaryData => false;
        public bool RequiresSecondaryData => false;
        public void Initialize() { }
        public Task RenderAsync(ChartDataContext context) => Task.CompletedTask;
        public void Clear(ChartState state) { }
        public void ResetZoom() { }
        public bool HasSeries(ChartState state) => false;
        public void UpdateSubtypeOptions() { }
        public void ClearCache() { }
        public void SetVisible(bool isVisible) { }
        public void SetTitle(string? title) { }
        public void SetToggleEnabled(bool enabled) { }
    }
}
