using DataVisualiser.UI.State;

namespace DataVisualiser.UI.MainHost;

public sealed class MainChartsViewEvidenceExportCoordinator
{
    public sealed class Actions(
        Func<ChartState, MetricState, DateTime, Task<ReachabilityEvidenceExportResult>> exportAsync,
        Action clearEvidence,
        Action<string, string> showInfo,
        Action<string, string> showWarning,
        Action<string, string> showError)
    {
        public Func<ChartState, MetricState, DateTime, Task<ReachabilityEvidenceExportResult>> ExportAsync { get; } = exportAsync ?? throw new ArgumentNullException(nameof(exportAsync));
        public Action ClearEvidence { get; } = clearEvidence ?? throw new ArgumentNullException(nameof(clearEvidence));
        public Action<string, string> ShowInfo { get; } = showInfo ?? throw new ArgumentNullException(nameof(showInfo));
        public Action<string, string> ShowWarning { get; } = showWarning ?? throw new ArgumentNullException(nameof(showWarning));
        public Action<string, string> ShowError { get; } = showError ?? throw new ArgumentNullException(nameof(showError));
    }

    public void ClearEvidence(Actions actions)
    {
        ArgumentNullException.ThrowIfNull(actions);
        actions.ClearEvidence();
    }

    public async Task ExportAsync(ChartState chartState, MetricState metricState, DateTime utcNow, Actions actions)
    {
        ArgumentNullException.ThrowIfNull(chartState);
        ArgumentNullException.ThrowIfNull(metricState);
        ArgumentNullException.ThrowIfNull(actions);

        try
        {
            var result = await actions.ExportAsync(chartState, metricState, utcNow);

            if (!result.HadReachabilityRecords)
                actions.ShowInfo("Reachability Export", "No reachability records captured yet. Export will include parity data if available.");

            if (result.Warnings.Count > 0)
                actions.ShowWarning("Parity Warnings", $"Export will include parity warnings:\n- {string.Join("\n- ", result.Warnings)}");

            actions.ShowInfo("Reachability Export", $"Reachability snapshot exported to:\n{result.FilePath}");
        }
        catch (Exception ex)
        {
            actions.ShowError("Reachability Export", $"Failed to export reachability snapshot:\n{ex.Message}");
        }
    }
}
