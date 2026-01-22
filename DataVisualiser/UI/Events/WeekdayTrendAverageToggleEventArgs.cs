namespace DataVisualiser.UI.Events;

public sealed class WeekdayTrendAverageToggleEventArgs : EventArgs
{
    public WeekdayTrendAverageToggleEventArgs(bool isChecked)
    {
        IsChecked = isChecked;
    }

    public bool IsChecked { get; }
}