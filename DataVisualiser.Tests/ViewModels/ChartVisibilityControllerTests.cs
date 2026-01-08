using DataVisualiser.Core.Orchestration;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.Tests.ViewModels;

public class ChartVisibilityControllerTests
{
    [Fact]
    public void BuildChartUpdateRequest_ReturnsNull_WhenInitializing()
    {
        var chartState = new ChartState();
        var controller = new ChartVisibilityController(chartState,
                () => true,
                _ =>
                {
                });

        var result = controller.BuildChartUpdateRequest();

        Assert.Null(result);
    }

    [Fact]
    public void ToggleChartVisibility_TogglesStateAndReturnsArgs()
    {
        var chartState = new ChartState
        {
                IsMainVisible = true,
                LastContext = new ChartDataContext()
        };
        var observedPropertyName = string.Empty;
        var controller = new ChartVisibilityController(chartState, () => false, name => observedPropertyName = name ?? string.Empty);

        var (visibilityArgs, updateArgs) = controller.ToggleChartVisibility("Main", () => chartState.IsMainVisible, v => chartState.IsMainVisible = v);

        Assert.False(chartState.IsMainVisible);
        Assert.Equal(nameof(ChartState), observedPropertyName);
        Assert.Equal("Main", visibilityArgs.ChartName);
        Assert.False(visibilityArgs.IsVisible);
        Assert.NotNull(updateArgs);
        Assert.True(updateArgs!.ShouldRenderCharts);
        Assert.True(updateArgs.IsVisibilityOnlyToggle);
        Assert.Equal("Main", updateArgs.ToggledChartName);
    }

    [Fact]
    public void ToggleDiffRatioOperation_ReturnsUpdate_WhenVisibleWithContext()
    {
        var chartState = new ChartState
        {
                IsDiffRatioVisible = true,
                LastContext = new ChartDataContext()
        };
        var controller = new ChartVisibilityController(chartState,
                () => false,
                _ =>
                {
                });

        var result = controller.ToggleDiffRatioOperation();

        Assert.NotNull(result);
    }

    [Fact]
    public void ToggleDiffRatioOperation_ReturnsNull_WhenNotVisible()
    {
        var chartState = new ChartState
        {
                IsDiffRatioVisible = false,
                LastContext = new ChartDataContext()
        };
        var controller = new ChartVisibilityController(chartState,
                () => false,
                _ =>
                {
                });

        var result = controller.ToggleDiffRatioOperation();

        Assert.Null(result);
    }
}