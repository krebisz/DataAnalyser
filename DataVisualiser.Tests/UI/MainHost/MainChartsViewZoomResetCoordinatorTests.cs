using System.Windows;
using System.Windows.Controls;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.MainHost.Coordination;
using DataVisualiser.UI.State;

namespace DataVisualiser.Tests.UI.MainHost;

public sealed class MainChartsViewZoomResetCoordinatorTests
{
    [Fact]
    public void ResetRegisteredCharts_ShouldSkipUnattachedWpfControllers()
    {
        StaTestHelper.Run(() =>
        {
            var coordinator = new MainChartsViewZoomResetCoordinator();
            var unattached = new FakeWpfChartController("DiffRatio");
            var warnings = new List<string>();

            coordinator.ResetRegisteredCharts(
                [unattached],
                new MainChartsViewZoomResetCoordinator.Actions((title, message, _) => warnings.Add($"{title}:{message}")));

            Assert.Equal(0, unattached.ResetCalls);
            Assert.Empty(warnings);
        });
    }

    [Fact]
    public void ResetRegisteredCharts_ShouldResetNonWpfControllers()
    {
        var coordinator = new MainChartsViewZoomResetCoordinator();
        var controller = new FakeChartController("BarPie");

        coordinator.ResetRegisteredCharts(
            [controller],
            new MainChartsViewZoomResetCoordinator.Actions((_, _, _) => { }));

        Assert.Equal(1, controller.ResetCalls);
    }

    [Fact]
    public void ResetRegisteredCharts_ShouldTrackWarningWhenControllerThrows()
    {
        var coordinator = new MainChartsViewZoomResetCoordinator();
        var controller = new FakeChartController("Transform", shouldThrow: true);
        var warnings = new List<string>();

        coordinator.ResetRegisteredCharts(
            [controller],
            new MainChartsViewZoomResetCoordinator.Actions((title, message, image) => warnings.Add($"{title}|{message}|{image}")));

        Assert.Single(warnings);
        Assert.Contains("Transform", warnings[0], StringComparison.Ordinal);
        Assert.Contains("boom", warnings[0], StringComparison.Ordinal);
    }

    [Fact]
    public void ShouldReset_ShouldReturnFalseForUnattachedWpfController()
    {
        StaTestHelper.Run(() =>
        {
            Assert.False(MainChartsViewZoomResetCoordinator.ShouldReset(new FakeWpfChartController("DiffRatio")));
        });
    }

    private class FakeChartController : IChartController
    {
        private readonly bool _shouldThrow;

        public FakeChartController(string key, bool shouldThrow = false)
        {
            Key = key;
            _shouldThrow = shouldThrow;
        }

        public int ResetCalls { get; private set; }

        public string Key { get; }
        public bool RequiresPrimaryData => false;
        public bool RequiresSecondaryData => false;
        public void Initialize() { }
        public Task RenderAsync(ChartDataContext context) => Task.CompletedTask;
        public void Clear(ChartState state) { }
        public void ResetZoom()
        {
            ResetCalls++;
            if (_shouldThrow)
                throw new InvalidOperationException("boom");
        }
        public bool HasSeries(ChartState state) => false;
        public void UpdateSubtypeOptions() { }
        public void ClearCache() { }
        public void SetVisible(bool isVisible) { }
        public void SetTitle(string? title) { }
        public void SetToggleEnabled(bool isEnabled) { }
    }

    private sealed class FakeWpfChartController : FakeChartController, IWpfChartPanelHost
    {
        public FakeWpfChartController(string key)
            : base(key)
        {
        }

        public Panel ChartContentPanel { get; } = new StackPanel();
    }
}
