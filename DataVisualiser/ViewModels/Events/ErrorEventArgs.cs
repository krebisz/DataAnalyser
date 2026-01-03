namespace DataVisualiser.ViewModels.Events;

public class ErrorEventArgs : EventArgs
{
    public string Message { get; set; } = "";
}