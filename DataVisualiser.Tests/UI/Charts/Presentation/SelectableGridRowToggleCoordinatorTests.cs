using DataVisualiser.UI.Charts.Presentation;

namespace DataVisualiser.Tests.UI.Charts.Presentation;

public sealed class SelectableGridRowToggleCoordinatorTests
{
    [Fact]
    public void ApplyRange_ShouldApplyTargetStateBetweenAnchorAndTarget()
    {
        var rows = new[]
        {
            new SelectableRow(true),
            new SelectableRow(true),
            new SelectableRow(true),
            new SelectableRow(true)
        };

        SelectableGridRowToggleCoordinator.ApplyRange(rows, 1, 3, false);

        Assert.True(rows[0].IsIncluded);
        Assert.False(rows[1].IsIncluded);
        Assert.False(rows[2].IsIncluded);
        Assert.False(rows[3].IsIncluded);
    }

    [Fact]
    public void ApplyRange_ShouldSupportReverseRange()
    {
        var rows = new[]
        {
            new SelectableRow(true),
            new SelectableRow(true),
            new SelectableRow(true),
            new SelectableRow(true)
        };

        SelectableGridRowToggleCoordinator.ApplyRange(rows, 3, 1, false);

        Assert.True(rows[0].IsIncluded);
        Assert.False(rows[1].IsIncluded);
        Assert.False(rows[2].IsIncluded);
        Assert.False(rows[3].IsIncluded);
    }

    [Fact]
    public void ToggleRows_ShouldExcludeAllWhenAllSelectedRowsAreIncluded()
    {
        var rows = new[]
        {
            new SelectableRow(true),
            new SelectableRow(true)
        };

        var changed = SelectableGridRowToggleCoordinator.ToggleRows(rows);

        Assert.True(changed);
        Assert.All(rows, row => Assert.False(row.IsIncluded));
    }

    [Fact]
    public void ToggleRows_ShouldIncludeAllWhenAnySelectedRowIsExcluded()
    {
        var rows = new[]
        {
            new SelectableRow(true),
            new SelectableRow(false)
        };

        var changed = SelectableGridRowToggleCoordinator.ToggleRows(rows);

        Assert.True(changed);
        Assert.All(rows, row => Assert.True(row.IsIncluded));
    }

    private sealed class SelectableRow(bool isIncluded) : ISelectableGridRow
    {
        public bool IsIncluded { get; set; } = isIncluded;
    }
}
