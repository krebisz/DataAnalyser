using System.Windows.Controls;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.UI.OperationChain;

public partial class OperationChainWorkbenchView : UserControl
{
    public OperationChainWorkbenchView()
    {
        InitializeComponent();
    }

    public void DisplayResult(OperationChainResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        SummaryText.Text =
            $"{result.DerivedDatasets.Count} derived dataset(s) from {result.Plan.SourceSeriesSignatures.Count} source series.";
        TraceItems.ItemsSource = result.Trace.Entries
            .Select(entry => new TraceRow(
                $"Step {entry.StepIndex + 1}",
                $"{entry.OperationKind} -> {entry.OutputDatasetId}; lossiness: {entry.Lossiness}"))
            .ToArray();
        DatasetGrid.ItemsSource = result.DerivedDatasets
            .SelectMany(dataset => dataset.Timeline.Select((timestamp, index) => new DatasetRow(
                dataset.Label,
                timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                FormatValue(dataset.RawValues[index]),
                FormatValue(dataset.SmoothedValues[index]))))
            .ToArray();
        EvidenceText.Text =
            $"Plan: {result.Evidence.PlanSignature} | Trace: {result.Evidence.TraceSignature} | Contract: {result.Evidence.ContractSignature}";
    }

    private static string FormatValue(double value) =>
        double.IsNaN(value) ? "NaN" : value.ToString("G6");

    private sealed record TraceRow(string StepLabel, string Detail);

    private sealed record DatasetRow(string Dataset, string Timestamp, string Raw, string Smoothed);
}
