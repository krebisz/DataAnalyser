using DataVisualiser.Core.Orchestration;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;

namespace DataVisualiser.Tests.UI.State;

public sealed class ChartStateLoadedDataSnapshotTests
{
    [Fact]
    public void LastContext_ShouldMaintainLoadedDataSnapshotForNonRenderingConsumers()
    {
        var state = new ChartState();
        var context = new ChartDataContext
        {
            LoadRequestSignature = "request",
            MetricType = "Weight",
            PrimaryMetricType = "Weight",
            SecondaryMetricType = "Steps",
            PrimarySubtype = "Morning",
            SecondarySubtype = "Evening",
            DisplayName1 = "Morning Weight",
            DisplayName2 = "Evening Steps",
            ActualSeriesCount = 2,
            Data1 = [new MetricData()],
            Data2 = [new MetricData(), new MetricData()],
            From = new DateTime(2026, 1, 1),
            To = new DateTime(2026, 1, 2)
        };

        state.LastContext = context;

        Assert.Same(context, state.LastContext);
        Assert.True(state.LastLoadedData.Present);
        Assert.Equal("request", state.LastLoadedData.LoadRequestSignature);
        Assert.Equal("Weight", state.LastLoadedData.PrimaryMetricType);
        Assert.Equal("Steps", state.LastLoadedData.SecondaryMetricType);
        Assert.Equal(2, state.LastLoadedData.ActualSeriesCount);
        Assert.Equal(1, state.LastLoadedData.Data1Count);
        Assert.Equal(2, state.LastLoadedData.Data2Count);
        Assert.Equal(
            "Weight:Morning|Steps:Evening::2026-01-01T00:00:00.0000000->2026-01-02T00:00:00.0000000::series=2",
            state.LastLoadedData.ContextSignature);
    }

    [Fact]
    public void LastContext_ShouldClearLoadedDataSnapshotWhenBridgeContextIsCleared()
    {
        var state = new ChartState
        {
            LastContext = new ChartDataContext
            {
                Data1 = [new MetricData()],
                ActualSeriesCount = 1
            }
        };

        state.LastContext = null;

        Assert.Null(state.LastContext);
        Assert.False(state.LastLoadedData.Present);
        Assert.Equal(0, state.LastLoadedData.ActualSeriesCount);
        Assert.Null(state.LastLoadedData.ContextSignature);
    }
}
