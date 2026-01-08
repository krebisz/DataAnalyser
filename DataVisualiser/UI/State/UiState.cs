namespace DataVisualiser.UI.State;

public class UiState
{
    // Prevent re-entry during loads
    public bool IsLoadingMetricTypes { get; set; }
    public bool IsLoadingSubtypes { get; set; }
    public bool IsLoadingData { get; set; }

    // Whether the UI should block inputs (lockout during load)
    public bool IsUiBusy { get; set; }

    // Tracking dynamic subtype controls count
    public int DynamicSubtypeCount { get; set; }
}