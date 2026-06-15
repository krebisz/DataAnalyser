using DataVisualiser.UI.MainHost.Evidence;
namespace DataVisualiser.UI.Charts.Presentation;

internal sealed partial class TransformSessionMilestoneRecorder
{
    public void RecordInitialized(int inputCount, string resolution) =>
        Record("OperationChainInitialized", "Success", $"Initialized with {inputCount} input row(s); resolution {resolution}.", inputCount: inputCount);

    public void RecordInitializationFailed(string message) =>
        Record("OperationChainInitializationFailed", "Error", $"Initialization failed: {message}");

    public void RecordInputAdded(int inputCount) =>
        Record("OperationChainInputAdded", "Info", $"Input row added; total input rows: {inputCount}.", inputCount: inputCount);

    public void RecordInputRemoved(int inputCount) =>
        Record("OperationChainInputRemoved", "Info", $"Input row removed; total input rows: {inputCount}.", inputCount: inputCount);

    public void RecordInputRemoveBlocked() =>
        Record("OperationChainInputRemoveBlocked", "Warning", "Cannot remove the final Operation Chain input row.", inputCount: 1);

    public void RecordResolutionChanged(string resolution) =>
        Record("OperationChainResolutionChanged", "Info", $"Resolution changed to {resolution}.");

    public void RecordComputeRequested(string operation, int inputCount, string? equation) =>
        Record("OperationChainComputeRequested", "Info", BuildOperationNote("Compute requested", operation, equation), operation, inputCount);

    public void RecordComputeCompleted(string operation, int inputCount, int resultPointCount, string? equation) =>
        Record("OperationChainComputeCompleted", "Success", BuildOperationNote($"Computed {resultPointCount} result row(s)", operation, equation), operation, inputCount, resultPointCount);

    public void RecordComputeFailed(string operation, int inputCount, string message, string? equation) =>
        Record("OperationChainComputeFailed", "Error", BuildOperationNote($"Compute failed: {message}", operation, equation), operation, inputCount);

    public void RecordInvalidEquation(string message, string? equation) =>
        Record("OperationChainEquationInvalid", "Error", BuildOperationNote($"Invalid equation: {message}", "Equation", equation));

    public void RecordEquationUpdated(string expression, int termCount) =>
        Record("OperationChainEquationUpdated", "Info", $"Equation updated with {termCount} term(s): {expression}");

    public void RecordEquationCleared() =>
        Record("OperationChainEquationCleared", "Info", "Equation cleared.");

    public void RecordClearRequested() =>
        Record("OperationChainClearRequested", "Info", "Operation Chain clear requested.");

    public void RecordOutputChartResetZoom() =>
        Record("OperationChainOutputChartZoomReset", "Info", "Operation Chain output chart zoom reset.");

    public void RecordOutputChartVisibilityChanged(bool isVisible) =>
        Record("OperationChainOutputChartVisibilityChanged", "Info", isVisible ? "Output chart shown." : "Output chart hidden.");

    public void RecordResultRowsToggled(int includedCount, int totalCount) =>
        Record("OperationChainResultRowsToggled", "Info", $"Result row inclusion changed: {includedCount}/{totalCount} included.", resultPointCount: includedCount);

    public void RecordEvidenceExportRequested(string operation, int inputCount) =>
        Record("OperationChainEvidenceExportRequested", "Info", "Operation Chain evidence export requested.", operation, inputCount);

    public void RecordEvidenceExportCompleted(string operation, int inputCount, string filePath) =>
        Record("OperationChainEvidenceExportCompleted", "Success", $"Operation Chain evidence exported to {filePath}.", operation, inputCount);

    public void RecordEvidenceExportFailed(string message) =>
        Record("OperationChainEvidenceExportFailed", "Error", $"Operation Chain evidence export failed: {message}");

    private void Record(
        string kind,
        string outcome,
        string note,
        string? operation = null,
        int? inputCount = null,
        int? resultPointCount = null)
    {
        var context = _sharedContextProvider?.Invoke();
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
            LoadedSeriesCount = context.ChartState.LastLoadedData.ActualSeriesCount,
            ContextSignature = context.ChartState.LastLoadedData.ContextSignature,
            Operation = operation,
            OperationArity = inputCount,
            ResultPointCount = resultPointCount,
            Note = note
        });
    }

    private static string BuildOperationNote(string prefix, string operation, string? equation)
    {
        if (string.IsNullOrWhiteSpace(equation))
            return $"{prefix}; operation {operation}.";

        return $"{prefix}; operation {operation}; equation {equation}.";
    }
}
