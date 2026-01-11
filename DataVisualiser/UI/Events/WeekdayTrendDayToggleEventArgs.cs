namespace DataVisualiser.UI.Events;

public sealed class WeekdayTrendDayToggleEventArgs : EventArgs
{
    public WeekdayTrendDayToggleEventArgs(DayOfWeek day, bool isChecked)
    {
        Day = day;
        IsChecked = isChecked;
    }

    public DayOfWeek Day { get; }

    public bool IsChecked { get; }
}