using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.Tests.VNext;

public sealed class DistributionSessionMilestoneRecorderTests
{
    [Fact]
    public void RecordFrequencyShadingToggle_OnEnabled_RecordsMilestoneWithCorrectKindAndNote()
    {
        var (recorder, chartState) = CreateRecorder();

        recorder.RecordFrequencyShadingToggle(DistributionMode.Weekly, true);

        var milestone = Assert.Single(chartState.SessionMilestones);
        Assert.Equal("DistributionFrequencyShadingToggled", milestone.Kind);
        Assert.Equal("Info", milestone.Outcome);
        Assert.Contains("Frequency shading enabled", milestone.Note);
        Assert.Contains("Weekly", milestone.Note);
    }

    [Fact]
    public void RecordFrequencyShadingToggle_OnDisabled_RecordsSimpleRangeNote()
    {
        var (recorder, chartState) = CreateRecorder();

        recorder.RecordFrequencyShadingToggle(DistributionMode.Hourly, false);

        var milestone = Assert.Single(chartState.SessionMilestones);
        Assert.Contains("Simple range enabled", milestone.Note);
        Assert.Contains("Hourly", milestone.Note);
    }

    [Fact]
    public void RecordIntervalCountChange_RecordsMilestoneWithIntervalValue()
    {
        var (recorder, chartState) = CreateRecorder();

        recorder.RecordIntervalCountChange(DistributionMode.Weekly, 15);

        var milestone = Assert.Single(chartState.SessionMilestones);
        Assert.Equal("DistributionIntervalCountChanged", milestone.Kind);
        Assert.Contains("15", milestone.Note);
        Assert.Contains("Weekly", milestone.Note);
    }

    [Fact]
    public void RecordModeChange_RecordsMilestoneWithNewMode()
    {
        var (recorder, chartState) = CreateRecorder();

        recorder.RecordModeChange(DistributionMode.Hourly);

        var milestone = Assert.Single(chartState.SessionMilestones);
        Assert.Equal("DistributionModeChanged", milestone.Kind);
        Assert.Contains("Hourly", milestone.Note);
    }

    [Fact]
    public void RecordChartTypeToggle_ToPolar_RecordsMilestone()
    {
        var (recorder, chartState) = CreateRecorder();

        recorder.RecordChartTypeToggle(true);

        var milestone = Assert.Single(chartState.SessionMilestones);
        Assert.Equal("DistributionChartTypeToggled", milestone.Kind);
        Assert.Contains("Polar", milestone.Note);
    }

    [Fact]
    public void RecordChartTypeToggle_ToCartesian_RecordsMilestone()
    {
        var (recorder, chartState) = CreateRecorder();

        recorder.RecordChartTypeToggle(false);

        var milestone = Assert.Single(chartState.SessionMilestones);
        Assert.Contains("Cartesian", milestone.Note);
    }

    [Fact]
    public void RecordSubtypeChange_WithSelection_RecordsDisplayName()
    {
        var (recorder, chartState) = CreateRecorder();

        recorder.RecordSubtypeChange(new MetricSeriesSelection("Weight", "water", "Weight", "Water"));

        var milestone = Assert.Single(chartState.SessionMilestones);
        Assert.Equal("DistributionSubtypeChanged", milestone.Kind);
        Assert.Contains("Weight - Water", milestone.Note);
    }

    [Fact]
    public void RecordSubtypeChange_WithNull_RecordsClearedNote()
    {
        var (recorder, chartState) = CreateRecorder();

        recorder.RecordSubtypeChange(null);

        var milestone = Assert.Single(chartState.SessionMilestones);
        Assert.Contains("cleared", milestone.Note);
    }

    [Fact]
    public void RecordMilestone_IncludesMetricStateContext()
    {
        var chartState = new ChartState();
        var metricState = new MetricState();
        metricState.SelectedMetricType = "Weight";
        metricState.SetSeriesSelections([new MetricSeriesSelection("Weight", "fat_free_mass")]);
        var viewModel = new MainWindowViewModel(chartState, metricState, new UiState(), new MetricSelectionService("Test"));
        var recorder = new DistributionSessionMilestoneRecorder(viewModel);

        recorder.RecordModeChange(DistributionMode.Weekly);

        var milestone = Assert.Single(chartState.SessionMilestones);
        Assert.Equal("Weight", milestone.MetricType);
        Assert.Equal(1, milestone.SelectedSeriesCount);
        Assert.Contains("Weight:fat_free_mass", milestone.SelectedDisplayKeys);
    }

    private static (DistributionSessionMilestoneRecorder Recorder, ChartState ChartState) CreateRecorder()
    {
        var chartState = new ChartState();
        var metricState = new MetricState();
        var viewModel = new MainWindowViewModel(chartState, metricState, new UiState(), new MetricSelectionService("Test"));
        return (new DistributionSessionMilestoneRecorder(viewModel), chartState);
    }
}
