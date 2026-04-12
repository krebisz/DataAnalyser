using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Data;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.Tests.Controls;

public sealed class TransformSessionMilestoneRecorderTests
{
    [Fact]
    public void RecordExecution_ShouldProjectResolvedSelectionsAndExecutionOutcome()
    {
        var viewModel = CreateViewModel();
        viewModel.MetricState.SelectedMetricType = "Weight";
        viewModel.MetricState.SetSeriesSelections(
        [
            new MetricSeriesSelection("Weight", "body_fat_mass", "Weight", "Fat (mass)"),
            new MetricSeriesSelection("Weight", "fat_free_mass", "Weight", "Fat Free (mass)")
        ]);
        viewModel.ChartState.LastLoadRuntime = new LoadRuntimeState(EvidenceRuntimePath.Legacy, "req", "snap", null, null, null, null, false);
        viewModel.ChartState.LastContext = new ChartDataContext
        {
            MetricType = "Weight",
            PrimaryMetricType = "Weight",
            PrimarySubtype = "body_fat_mass",
            SecondaryMetricType = "Weight",
            SecondarySubtype = "fat_free_mass",
            ActualSeriesCount = 2
        };

        var recorder = new TransformSessionMilestoneRecorder(viewModel);
        var resolution = new TransformResolutionResult(
            new TransformSelectionResolution(
                new MetricSeriesSelection("Weight", "body_fat_mass", "Weight", "Fat (mass)"),
                new MetricSeriesSelection("Weight", "fat_free_mass", "Weight", "Fat Free (mass)"),
                true),
            [new MetricData { NormalizedTimestamp = new DateTime(2026, 1, 1), Value = 1m }],
            [new MetricData { NormalizedTimestamp = new DateTime(2026, 1, 1), Value = 2m }],
            viewModel.ChartState.LastContext);
        var execution = new TransformExecutionResult(
            [new MetricData { NormalizedTimestamp = new DateTime(2026, 1, 1), Value = 1m }],
            [3d],
            "Add",
            2,
            [],
            null);

        recorder.RecordExecution(execution, resolution);

        var milestone = Assert.Single(viewModel.ChartState.SessionMilestones);
        Assert.Equal("TransformOperationRendered", milestone.Kind);
        Assert.Equal("Weight:body_fat_mass", milestone.PrimarySeriesDisplayKey);
        Assert.Equal("Weight:fat_free_mass", milestone.SecondarySeriesDisplayKey);
        Assert.Equal("Add", milestone.Operation);
        Assert.Equal(2, milestone.OperationArity);
        Assert.Equal(1, milestone.ResultPointCount);
    }

    [Fact]
    public void RecordToggle_ShouldCaptureCurrentVisibilityAndSelection()
    {
        var viewModel = CreateViewModel();
        viewModel.ChartState.IsTransformPanelVisible = true;
        viewModel.MetricState.SelectedMetricType = "Weight";
        viewModel.MetricState.SetSeriesSelections([new MetricSeriesSelection("Weight", "body_fat_mass", "Weight", "Fat (mass)")]);
        viewModel.ChartState.LastLoadRuntime = new LoadRuntimeState(EvidenceRuntimePath.VNextMain, "req", "snap", null, null, null, null, true);

        var recorder = new TransformSessionMilestoneRecorder(viewModel);
        recorder.RecordToggle();

        var milestone = Assert.Single(viewModel.ChartState.SessionMilestones);
        Assert.Equal("TransformToggleRequested", milestone.Kind);
        Assert.Equal("Info", milestone.Outcome);
        Assert.Equal(EvidenceRuntimePath.VNextMain, milestone.RuntimePath);
        Assert.Equal("Transform panel visible.", milestone.Note);
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
