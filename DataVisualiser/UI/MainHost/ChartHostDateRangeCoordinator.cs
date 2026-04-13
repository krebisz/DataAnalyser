namespace DataVisualiser.UI.MainHost;

public sealed class ChartHostDateRangeCoordinator
{
    public sealed record Actions(
        Action<DateTime?, DateTime?> SetDateRange,
        Action<DateTime?> SetFromDate,
        Action<DateTime?> SetToDate,
        Func<DateTime?> GetFromDate,
        Func<DateTime?> GetToDate);

    public void ApplyDefaultRange(DateTime utcNow, Actions actions)
    {
        ArgumentNullException.ThrowIfNull(actions);

        var fromDate = utcNow.AddDays(-30);
        var toDate = utcNow;

        actions.SetDateRange(fromDate, toDate);
        actions.SetFromDate(actions.GetFromDate());
        actions.SetToDate(actions.GetToDate());
    }
}
