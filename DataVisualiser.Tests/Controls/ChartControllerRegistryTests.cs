using System.Windows.Controls.Primitives;
using DataVisualiser.Core.Orchestration;
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

    [Fact]
    public void Register_RejectsMissingKey()
    {
        var registry = new ChartControllerRegistry();

        Assert.Throws<ArgumentException>(() => registry.Register(new StubController(" ")));
    }

    [Fact]
    public void Get_IsCaseInsensitive()
    {
        var registry = new ChartControllerRegistry();
        registry.Register(new StubController(ChartControllerKeys.Main));

        var resolved = registry.Get(ChartControllerKeys.Main.ToUpperInvariant());

        Assert.Equal(ChartControllerKeys.Main, resolved.Key);
    }

    [Fact]
    public void All_ReturnsRegistrationOrder()
    {
        var registry = new ChartControllerRegistry();
        var main = new StubController(ChartControllerKeys.Main);
        var normalized = new StubController(ChartControllerKeys.Normalized);
        var diffRatio = new StubController(ChartControllerKeys.DiffRatio);

        registry.Register(main);
        registry.Register(normalized);
        registry.Register(diffRatio);

        var ordered = registry.All().ToList();

        Assert.Equal(new[]
                {
                        main,
                        normalized,
                        diffRatio
                },
                ordered);
    }

    [Fact]
    public void ChartControllerKeys_All_AreUnique()
    {
        var keys = ChartControllerKeys.All;

        Assert.Equal(keys.Length, keys.Distinct(StringComparer.OrdinalIgnoreCase).Count());
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
        public ButtonBase ToggleButton => null!;

        public void Initialize()
        {
        }

        public Task RenderAsync(ChartDataContext context)
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