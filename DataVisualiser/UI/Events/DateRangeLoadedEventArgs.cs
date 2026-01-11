namespace DataVisualiser.UI.Events;

public class DateRangeLoadedEventArgs : EventArgs
{
    public DateTime? MinDate { get; set; }
    public DateTime? MaxDate { get; set; }
}
