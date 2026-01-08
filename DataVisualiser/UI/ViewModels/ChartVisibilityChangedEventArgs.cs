namespace DataVisualiser.UI.ViewModels;

public class ChartVisibilityChangedEventArgs : EventArgs
{
    public string ChartName { get; set; } = "";
    public bool IsVisible { get; set; }
}