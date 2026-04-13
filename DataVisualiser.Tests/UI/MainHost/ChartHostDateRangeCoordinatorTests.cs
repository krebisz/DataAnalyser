using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.MainHost.Coordination;

namespace DataVisualiser.Tests.UI.MainHost;

public sealed class ChartHostDateRangeCoordinatorTests
{
    [Fact]
    public void ApplyDefaultRange_ShouldSetThirtyDayRangeAndProjectStateBackToUi()
    {
        var coordinator = new ChartHostDateRangeCoordinator();
        DateTime? stateFrom = null;
        DateTime? stateTo = null;
        DateTime? uiFrom = null;
        DateTime? uiTo = null;
        var now = new DateTime(2026, 4, 13, 10, 30, 0, DateTimeKind.Utc);

        coordinator.ApplyDefaultRange(
            now,
            new ChartHostDateRangeCoordinator.Actions(
                (from, to) =>
                {
                    stateFrom = from;
                    stateTo = to;
                },
                value => uiFrom = value,
                value => uiTo = value,
                () => stateFrom,
                () => stateTo));

        Assert.Equal(now.AddDays(-30), stateFrom);
        Assert.Equal(now, stateTo);
        Assert.Equal(stateFrom, uiFrom);
        Assert.Equal(stateTo, uiTo);
    }
}
