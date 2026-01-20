using DataVisualiser.UI.Controls;
using DataVisualiser.UI.State;

namespace DataVisualiser.Tests.Controls;

public sealed class ChartControllerRegistryTests
{
    [Fact]
    public void Register_AddsController_AndGetReturnsSameInstance()
    {
        var registry = new ChartControllerRegistry();
        var controller = new StubController(ChartControllerKeys.Main);

        registry.Register(controller);

        var resolved = registry.Get(ChartControllerKeys.Main);

        Assert.Same(controller, resolved);
        Assert.Single(registry.All());
    }

    [Fact]
    public void Register_RejectsDuplicateKeys()
    {
        var registry = new ChartControllerRegistry();
        registry.Register(new StubController(ChartControllerKeys.Main));

        Assert.Throws<InvalidOperationException>(() => registry.Register(new StubController(ChartControllerKeys.Main)));
    }

    [Fact]
    public void Get_ThrowsForMissingKey()
    {
        var registry = new ChartControllerRegistry();

        Assert.Throws<KeyNotFoundException>(() => registry.Get("Missing"));
    }

    private sealed class StubController : IChartController
    {
        public StubController(string key)
        {
            Key = key;
        }

        public string Key { get; }
        public bool RequiresPrimaryData => false;
        public bool RequiresSecondaryData => false;
        public ChartPanelController Panel => null!;
        public System.Windows.Controls.Primitives.ButtonBase ToggleButton => null!;

        public void Initialize()
        {
        }

        public Task RenderAsync(DataVisualiser.Core.Orchestration.ChartDataContext context)
        {
            return Task.CompletedTask;
        }

        public void Clear(ChartState state)
        {
        }

        public void ResetZoom()
        {
        }
    }
}
