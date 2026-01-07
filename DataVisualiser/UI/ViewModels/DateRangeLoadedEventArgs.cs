namespace DataVisualiser.UI.ViewModels;

public class DateRangeLoadedEventArgs : EventArgs
{
    public DateTime? MinDate { get; set; }
    public DateTime? MaxDate { get; set; }
}