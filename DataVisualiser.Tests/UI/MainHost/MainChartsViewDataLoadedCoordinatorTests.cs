using DataVisualiser.Core.Orchestration;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.MainHost.Coordination;

namespace DataVisualiser.Tests.UI.MainHost;

public sealed class MainChartsViewDataLoadedCoordinatorTests
{
    [Fact]
    public async Task HandleAsync_ShouldDoNothingWithoutPrimaryData()
    {
        var invoked = new List<string>();
        var coordinator = new MainChartsViewDataLoadedCoordinator();

        await coordinator.HandleAsync(
            new ChartDataContext(),
            1,
            CreateActions(invoked));

        Assert.Empty(invoked);
    }

    [Fact]
    public async Task HandleAsync_ShouldRefreshUiStateAndRenderInExpectedOrder()
    {
        var invoked = new List<string>();
        var coordinator = new MainChartsViewDataLoadedCoordinator();

        await coordinator.HandleAsync(
            CreateContext(),
            2,
            CreateActions(invoked));

        Assert.Equal(
            [
                "CompleteTransformSelectionsPendingLoad",
                $"UpdateSubtypeOptions:{ChartControllerKeys.Normalized}",
                $"UpdateSubtypeOptions:{ChartControllerKeys.DiffRatio}",
                $"UpdateSubtypeOptions:{ChartControllerKeys.Main}",
                "UpdateTransformSubtypeOptions",
                "UpdateTransformComputeButtonState",
                "UpdatePrimaryDataRequiredButtonStates:2",
                "UpdateSecondaryDataRequiredButtonStates:2",
                $"RenderChartAsync:{ChartControllerKeys.BarPie}",
                "RenderChartsFromLastContextAsync"
            ],
            invoked);
    }

    [Fact]
    public async Task HandleAsync_ShouldRefreshUiStateAndRenderInExpectedOrder_ForSingleSeriesLoad()
    {
        var invoked = new List<string>();
        var coordinator = new MainChartsViewDataLoadedCoordinator();

        await coordinator.HandleAsync(
            CreateContext(includeSecondary: false),
            1,
            CreateActions(invoked));

        Assert.Equal(
            [
                "CompleteTransformSelectionsPendingLoad",
                $"UpdateSubtypeOptions:{ChartControllerKeys.Normalized}",
                $"UpdateSubtypeOptions:{ChartControllerKeys.DiffRatio}",
                $"UpdateSubtypeOptions:{ChartControllerKeys.Main}",
                "UpdateTransformSubtypeOptions",
                "UpdateTransformComputeButtonState",
                "UpdatePrimaryDataRequiredButtonStates:1",
                "UpdateSecondaryDataRequiredButtonStates:1",
                $"RenderChartAsync:{ChartControllerKeys.BarPie}",
                "RenderChartsFromLastContextAsync"
            ],
            invoked);
    }

    [Fact]
    public async Task HandleAsync_ShouldRefreshUiStateAndRenderInExpectedOrder_ForSecondaryDataLoad()
    {
        var invoked = new List<string>();
        var coordinator = new MainChartsViewDataLoadedCoordinator();

        await coordinator.HandleAsync(
            CreateContext(includeSecondary: true),
            2,
            CreateActions(invoked));

        Assert.Equal(
            [
                "CompleteTransformSelectionsPendingLoad",
                $"UpdateSubtypeOptions:{ChartControllerKeys.Normalized}",
                $"UpdateSubtypeOptions:{ChartControllerKeys.DiffRatio}",
                $"UpdateSubtypeOptions:{ChartControllerKeys.Main}",
                "UpdateTransformSubtypeOptions",
                "UpdateTransformComputeButtonState",
                "UpdatePrimaryDataRequiredButtonStates:2",
                "UpdateSecondaryDataRequiredButtonStates:2",
                $"RenderChartAsync:{ChartControllerKeys.BarPie}",
                "RenderChartsFromLastContextAsync"
            ],
            invoked);
    }

    private static ChartDataContext CreateContext(bool includeSecondary = false)
    {
        return new ChartDataContext
        {
            Data1 =
            [
                new MetricData { NormalizedTimestamp = DateTime.Today, Value = 1m }
            ],
            Data2 = includeSecondary
                ? [new MetricData { NormalizedTimestamp = DateTime.Today, Value = 2m }]
                : null
        };
    }

    private static MainChartsViewDataLoadedCoordinator.Actions CreateActions(List<string> invoked)
    {
        return new MainChartsViewDataLoadedCoordinator.Actions(
            () => invoked.Add("CompleteTransformSelectionsPendingLoad"),
            key => invoked.Add($"UpdateSubtypeOptions:{key}"),
            () => invoked.Add("UpdateTransformSubtypeOptions"),
            () => invoked.Add("UpdateTransformComputeButtonState"),
            count => invoked.Add($"UpdatePrimaryDataRequiredButtonStates:{count}"),
            count => invoked.Add($"UpdateSecondaryDataRequiredButtonStates:{count}"),
            (key, _) =>
            {
                invoked.Add($"RenderChartAsync:{key}");
                return Task.CompletedTask;
            },
            () =>
            {
                invoked.Add("RenderChartsFromLastContextAsync");
                return Task.CompletedTask;
            });
    }
}
