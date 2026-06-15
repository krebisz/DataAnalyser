using DataVisualiser.Core.Data;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.MainHost.Evidence;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.Tests.UI.Charts.Presentation;

public sealed class TransformSessionMilestoneRecorderOperationChainTests
{
    [Fact]
    public void RecordActions_ShouldUseStableOperationChainMilestoneKinds()
    {
        var context = CreateContext();
        var recorder = new TransformSessionMilestoneRecorder(() => context);

        recorder.RecordInitialized(1, "HealthMetrics");
        recorder.RecordInputAdded(2);
        recorder.RecordResolutionChanged("Daily");
        recorder.RecordComputeRequested("Add", 2, null);
        recorder.RecordComputeCompleted("Add", 2, 5, null);
        recorder.RecordEquationUpdated("(S\u2081) + S\u2082", 2);
        recorder.RecordInvalidEquation("First term is invalid.", "(S\u2081) + S\u2082");
        recorder.RecordResultRowsToggled(3, 5);
        recorder.RecordOutputChartResetZoom();
        recorder.RecordClearRequested();

        Assert.Equal(
        [
            "OperationChainInitialized",
            "OperationChainInputAdded",
            "OperationChainResolutionChanged",
            "OperationChainComputeRequested",
            "OperationChainComputeCompleted",
            "OperationChainEquationUpdated",
            "OperationChainEquationInvalid",
            "OperationChainResultRowsToggled",
            "OperationChainOutputChartZoomReset",
            "OperationChainClearRequested"
        ], context.ChartState.SessionMilestones.Select(milestone => milestone.Kind));
    }

    [Fact]
    public void RecordComputeCompleted_ShouldProjectOperationArityAndResultCount()
    {
        var context = CreateContext();
        context.MetricState.SelectedMetricType = "Weight";
        context.MetricState.SetSeriesSelections([new MetricSeriesSelection("Weight", "body_fat_mass", "Weight", "Fat")]);
        context.ChartState.LastLoadRuntime = new LoadRuntimeState(EvidenceRuntimePath.VNextMain, "request", "snapshot", null, null, null, null, false);
        var recorder = new TransformSessionMilestoneRecorder(() => context);

        recorder.RecordComputeCompleted("Divide", 3, 12, "(S\u2081 + S\u2082) / S\u2083");

        var milestone = Assert.Single(context.ChartState.SessionMilestones);
        Assert.Equal("OperationChainComputeCompleted", milestone.Kind);
        Assert.Equal("Success", milestone.Outcome);
        Assert.Equal("Divide", milestone.Operation);
        Assert.Equal(3, milestone.OperationArity);
        Assert.Equal(12, milestone.ResultPointCount);
        Assert.Equal("Weight", milestone.MetricType);
        Assert.Equal(1, milestone.SelectedSeriesCount);
        Assert.Equal(EvidenceRuntimePath.VNextMain, milestone.RuntimePath);
        Assert.Contains("equation", milestone.Note, StringComparison.Ordinal);
    }

    [Fact]
    public void Record_ShouldNoOpWhenSharedContextIsUnavailable()
    {
        var recorder = new TransformSessionMilestoneRecorder(() => null);

        recorder.RecordClearRequested();
        recorder.RecordComputeFailed("Add", 2, "blocked", null);
    }

    private static SharedMainWindowViewModelContext CreateContext()
    {
        var chartState = new ChartState();
        var metricState = new MetricState();
        var uiState = new UiState();
        var metricService = new MetricSelectionService(new StubMetricSelectionDataQueries(), "TestConnection");
        var viewModel = new MainWindowViewModel(chartState, metricState, uiState, metricService);

        return new SharedMainWindowViewModelContext(viewModel, chartState, metricState, uiState, metricService);
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
