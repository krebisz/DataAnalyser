using DataVisualiser.Shared.Models;

namespace DataVisualiser.UI.State;

public class MetricState
{
    public string? SelectedMetricType { get; set; }
    public List<MetricSeriesSelection> SelectedSeries { get; } = new();

    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    public string? ResolutionTableName { get; set; }

    public void SetSeriesSelections(IEnumerable<MetricSeriesSelection> selections)
    {
        SelectedSeries.Clear();
        foreach (var selection in selections)
            SelectedSeries.Add(selection);
    }
}