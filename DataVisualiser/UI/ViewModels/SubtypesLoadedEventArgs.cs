namespace DataVisualiser.UI.ViewModels.Events;

public class SubtypesLoadedEventArgs : EventArgs
{
    public IEnumerable<string> Subtypes { get; set; } = Enumerable.Empty<string>();
}
