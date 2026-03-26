using DataVisualiser.UI.Syncfusion;

namespace DataVisualiser.Tests.UI.Syncfusion;

public sealed class SyncfusionTooltipReflectionHelperTests
{
    [Fact]
    public void DisableThirdPartyTooltip_DisablesDirectAndNestedTooltipProperties()
    {
        var target = new FakeTooltipTarget();

        SyncfusionTooltipReflectionHelper.DisableThirdPartyTooltip(target);

        Assert.False(target.ShowTooltip);
        Assert.False(target.EnableToolTip);
        Assert.Null(target.ToolTipTemplate);
        Assert.Equal(FakeTooltipMode.None, target.TooltipMode);
        Assert.False(target.TooltipBehavior.TooltipEnabled);
        Assert.Equal(FakeTooltipMode.None, target.TooltipBehavior.ToolTipState);
        Assert.Null(target.TooltipBehavior.TooltipTemplate);
    }

    [Theory]
    [InlineData("Category", "Alpha")]
    [InlineData("Submetric", "Beta")]
    [InlineData("Group", "Gamma")]
    public void TryExtractCategoryKey_ResolvesKnownPropertyNames(string propertyName, string expected)
    {
        object source = propertyName switch
        {
            "Category" => new { Category = expected },
            "Submetric" => new { Submetric = expected },
            _ => new { Group = expected }
        };

        var result = SyncfusionTooltipReflectionHelper.TryExtractCategoryKey(source, out var key);

        Assert.True(result);
        Assert.Equal(expected, key);
    }

    private enum FakeTooltipMode
    {
        Off,
        None,
        Enabled
    }

    private sealed class FakeTooltipBehavior
    {
        public bool TooltipEnabled { get; set; } = true;
        public FakeTooltipMode ToolTipState { get; set; } = FakeTooltipMode.Enabled;
        public object? TooltipTemplate { get; set; } = new();
    }

    private sealed class FakeTooltipTarget
    {
        public bool ShowTooltip { get; set; } = true;
        public bool EnableToolTip { get; set; } = true;
        public object? ToolTipTemplate { get; set; } = new();
        public FakeTooltipMode TooltipMode { get; set; } = FakeTooltipMode.Enabled;
        public FakeTooltipBehavior TooltipBehavior { get; } = new();
    }
}
