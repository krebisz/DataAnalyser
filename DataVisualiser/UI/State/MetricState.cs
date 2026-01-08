namespace DataVisualiser.UI.State;

public class MetricState
{
    public string? SelectedMetricType { get; set; }
    public List<string> SelectedSubtypes { get; } = new();

    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    public string? ResolutionTableName { get; set; }

    public void SetSubtypes(IEnumerable<string?> subs)
    {
        SelectedSubtypes.Clear();
        foreach (var s in subs.Where(x => !string.IsNullOrWhiteSpace(x)))
            SelectedSubtypes.Add(s!);
    }
}