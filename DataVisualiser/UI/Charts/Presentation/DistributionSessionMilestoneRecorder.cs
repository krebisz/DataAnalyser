using DataVisualiser.Shared.Models;
using DataVisualiser.UI.MainHost.Evidence;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.UI.Charts.Presentation;

internal sealed class DistributionSessionMilestoneRecorder
{
    private readonly MainWindowViewModel _viewModel;

    public DistributionSessionMilestoneRecorder(MainWindowViewModel viewModel)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }

    public void RecordFrequencyShadingToggle(DistributionMode mode, bool useFrequencyShading)
    {
        RecordMilestone(
            "DistributionFrequencyShadingToggled",
            useFrequencyShading
                ? $"Frequency shading enabled for {mode} mode."
                : $"Simple range enabled for {mode} mode.");
    }

    public void RecordIntervalCountChange(DistributionMode mode, int intervalCount)
    {
        RecordMilestone(
            "DistributionIntervalCountChanged",
            $"Interval count set to {intervalCount} for {mode} mode.");
    }

    public void RecordModeChange(DistributionMode mode)
    {
        RecordMilestone(
            "DistributionModeChanged",
            $"Distribution mode changed to {mode}.");
    }

    public void RecordChartTypeToggle(bool isPolarMode)
    {
        RecordMilestone(
            "DistributionChartTypeToggled",
            isPolarMode ? "Switched to Polar." : "Switched to Cartesian.");
    }

    public void RecordSubtypeChange(MetricSeriesSelection? selection)
    {
        RecordMilestone(
            "DistributionSubtypeChanged",
            selection != null ? $"Distribution series set to {selection.DisplayName}." : "Distribution series cleared.");
    }

    private void RecordMilestone(string kind, string note)
    {
        var chartState = _viewModel.ChartState;
        var context = chartState.LastContext;

        chartState.RecordSessionMilestone(new SessionMilestoneSnapshot
        {
            TimestampUtc = DateTime.UtcNow,
            Kind = kind,
            Outcome = "Info",
            MetricType = _viewModel.MetricState.SelectedMetricType,
            SelectedSeriesCount = _viewModel.MetricState.SelectedSeries.Count,
            SelectedDisplayKeys = _viewModel.MetricState.SelectedSeries.Select(series => series.DisplayKey).ToList(),
            RuntimePath = chartState.LastDistributionLoadRuntime?.RuntimePath ?? chartState.LastLoadRuntime?.RuntimePath,
            LoadedSeriesCount = context?.ActualSeriesCount ?? 0,
            ContextSignature = EvidenceDiagnosticsBuilder.BuildContextSignature(context),
            Note = note
        });
    }
}
