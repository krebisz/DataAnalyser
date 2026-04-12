using System.Windows;
using DataVisualiser.Core.Data;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.Tests.UI.MainHost;

public sealed class MainChartsSessionDiagnosticsRecorderTests
{
    [Fact]
    public void TrackHostMessage_ShouldRecordBoundedHostMessagesAndSessionMilestone()
    {
        var viewModel = CreateViewModel();
        viewModel.MetricState.SelectedMetricType = "Weight";
        viewModel.MetricState.SetSeriesSelections([new MetricSeriesSelection("Weight", "weight", "Weight", "Weight")]);

        var recorder = new MainChartsSessionDiagnosticsRecorder(viewModel);

        for (var i = 0; i < 25; i++)
            recorder.TrackHostMessage($"Title{i}", $"Message{i}", MessageBoxImage.Warning);

        Assert.Equal(20, recorder.RecentHostMessages.Count);
        Assert.Equal("Title24", recorder.RecentHostMessages[^1].Title);
        Assert.Equal(25, viewModel.ChartState.SessionMilestones.Count);
        Assert.Equal("HostMessage", viewModel.ChartState.SessionMilestones[^1].Kind);
        Assert.Equal("Warning", viewModel.ChartState.SessionMilestones[^1].Outcome);
    }

    [Fact]
    public void RecordSessionMilestone_ShouldProjectCurrentSelectionAndRuntimeState()
    {
        var viewModel = CreateViewModel();
        viewModel.MetricState.SelectedMetricType = "Weight";
        viewModel.MetricState.SetSeriesSelections(
        [
            new MetricSeriesSelection("Weight", "body_fat_mass", "Weight", "Fat (mass)"),
            new MetricSeriesSelection("Weight", "fat_free_mass", "Weight", "Fat Free (mass)")
        ]);
        viewModel.ChartState.LastLoadRuntime = new LoadRuntimeState(EvidenceRuntimePath.VNextMain, "req", "snap", null, null, null, null, true);
        viewModel.ChartState.LastContext = new ChartDataContext
        {
            MetricType = "Weight",
            PrimaryMetricType = "Weight",
            PrimarySubtype = "body_fat_mass",
            SecondaryMetricType = "Weight",
            SecondarySubtype = "fat_free_mass"
        };

        var recorder = new MainChartsSessionDiagnosticsRecorder(viewModel);
        recorder.RecordSessionMilestone("DataLoaded", "Success", "loaded");

        var milestone = Assert.Single(viewModel.ChartState.SessionMilestones);
        Assert.Equal("DataLoaded", milestone.Kind);
        Assert.Equal("Success", milestone.Outcome);
        Assert.Equal("Weight", milestone.MetricType);
        Assert.Equal(2, milestone.SelectedSeriesCount);
        Assert.Equal(EvidenceRuntimePath.VNextMain, milestone.RuntimePath);
        Assert.Equal("loaded", milestone.Note);
    }

    private static MainWindowViewModel CreateViewModel()
    {
        var chartState = new ChartState();
        var metricState = new MetricState();
        var uiState = new UiState();
        var metricService = new MetricSelectionService(new StubMetricSelectionDataQueries(), "TestConnection");
        return new MainWindowViewModel(chartState, metricState, uiState, metricService);
    }

    private sealed class StubMetricSelectionDataQueries : IMetricSelectionDataQueries
    {
        public Task<long> GetRecordCount(string metricType, string? metricSubtype = null) => Task.FromResult(0L);
        public Task<IEnumerable<MetricData>> GetHealthMetricsDataByBaseType(string baseType, string? subtype, DateTime? from, DateTime? to, string tableName, int? maxRecords = null, SamplingMode samplingMode = SamplingMode.None, int? targetSamples = null) => Task.FromResult<IEnumerable<MetricData>>([]);
        public Task<IEnumerable<MetricNameOption>> GetBaseMetricTypeOptions(string tableName) => Task.FromResult<IEnumerable<MetricNameOption>>([]);
        public Task<IEnumerable<MetricNameOption>> GetSubtypeOptionsForBaseType(string baseType, string tableName) => Task.FromResult<IEnumerable<MetricNameOption>>([]);
        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRange(string baseType, string? subtype, string tableName) => Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRangeFromCounts(string baseType, IReadOnlyCollection<string>? subtypes = null) => Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRangeForSubtypes(string baseType, IReadOnlyCollection<string>? subtypes, string tableName) => Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
    }
}
