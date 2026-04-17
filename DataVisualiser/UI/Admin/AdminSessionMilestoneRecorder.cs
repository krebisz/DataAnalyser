using DataVisualiser.UI.MainHost.Evidence;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.UI.Admin;

internal sealed class AdminSessionMilestoneRecorder(Func<SharedMainWindowViewModelContext?> getContext)
{
    public AdminSessionMilestoneRecorder()
        : this(() => SharedMainWindowViewModelProvider.Current)
    {
    }

    public void RecordMetricTypeChanged(string? selectedMetricType)
    {
        Record("AdminMetricTypeChanged", "Info", $"Admin metric type changed to {FormatValue(selectedMetricType)}.");
    }

    public void RecordHideDisabledToggled(bool hideDisabled)
    {
        Record("AdminHideDisabledToggled", "Info", hideDisabled ? "Disabled rows hidden." : "Disabled rows shown.");
    }

    public void RecordReloadRequested(string? selectedMetricType)
    {
        Record("AdminReloadRequested", "Info", $"Admin reload requested for {FormatValue(selectedMetricType)}.");
    }

    public void RecordReloadCompleted(string? selectedMetricType, int rowCount)
    {
        Record("AdminReloadCompleted", "Success", $"Loaded {rowCount} admin row(s) for {FormatValue(selectedMetricType)}.");
    }

    public void RecordReloadFailed(string? selectedMetricType, string message)
    {
        Record("AdminReloadFailed", "Error", $"Admin reload failed for {FormatValue(selectedMetricType)}: {message}");
    }

    public void RecordGridEdited(string metricType, string metricSubtype, string? propertyName)
    {
        Record("AdminGridEdited", "Info", $"Edited {metricType}:{metricSubtype} ({propertyName ?? "unknown property"}).");
    }

    public void RecordSaveRequested(int dirtyRowCount)
    {
        Record("AdminSaveRequested", "Info", $"Admin save requested for {dirtyRowCount} dirty row(s).");
    }

    public void RecordSaveSkipped()
    {
        Record("AdminSaveSkipped", "Info", "Admin save skipped because there were no dirty rows.");
    }

    public void RecordSaveCompleted(int dirtyRowCount, int affectedRowCount)
    {
        Record("AdminSaveCompleted", "Success", $"Saved {dirtyRowCount} dirty row(s); affected rows: {affectedRowCount}.");
    }

    public void RecordSaveFailed(int dirtyRowCount, string message)
    {
        Record("AdminSaveFailed", "Error", $"Admin save failed for {dirtyRowCount} dirty row(s): {message}");
    }

    private void Record(string kind, string outcome, string note)
    {
        var context = getContext();
        if (context == null)
            return;

        context.ChartState.RecordSessionMilestone(new SessionMilestoneSnapshot
        {
            TimestampUtc = DateTime.UtcNow,
            Kind = kind,
            Outcome = outcome,
            MetricType = context.MetricState.SelectedMetricType,
            SelectedSeriesCount = context.MetricState.SelectedSeries.Count,
            SelectedDisplayKeys = context.MetricState.SelectedSeries.Select(series => series.DisplayKey).ToList(),
            RuntimePath = context.ChartState.LastLoadRuntime?.RuntimePath,
            LoadedSeriesCount = context.ChartState.LastContext?.ActualSeriesCount ?? 0,
            ContextSignature = EvidenceDiagnosticsBuilder.BuildContextSignature(context.ChartState.LastContext),
            Note = note
        });
    }

    private static string FormatValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "(All)" : value;
    }
}
