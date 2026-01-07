namespace DataVisualiser.UI.ViewModels;

public class SubtypesLoadedEventArgs : EventArgs
{
    public IEnumerable<string> Subtypes { get; set; } = Enumerable.Empty<string>();
}