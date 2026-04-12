using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;
using DataVisualiser.VNext.Application;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.UI.MainHost;

public sealed record VNextMainChartLoadResult(
    bool Success,
    ChartDataContext? ProjectedContext,
    string? RequestSignature,
    string? SnapshotSignature,
    ChartProgramKind? ProgramKind,
    string? ProgramSourceSignature,
    string? ProjectedContextSignature,
    string? FailureReason);

public sealed class VNextMainChartIntegrationCoordinator
{
    private readonly Func<ReasoningSessionCoordinator> _coordinatorFactory;
    private readonly LegacyChartProgramProjector _projector = new();

    public VNextMainChartIntegrationCoordinator(MetricSelectionService metricSelectionService)
    {
        ArgumentNullException.ThrowIfNull(metricSelectionService);
        _coordinatorFactory = () => ReasoningEngineFactory.CreateCoordinator(metricSelectionService);
    }

    internal VNextMainChartIntegrationCoordinator(Func<ReasoningSessionCoordinator> coordinatorFactory)
    {
        _coordinatorFactory = coordinatorFactory ?? throw new ArgumentNullException(nameof(coordinatorFactory));
    }

    public async Task<VNextMainChartLoadResult> LoadMainChartAsync(
        MetricLoadRequest request,
        MainChartDisplayMode displayMode,
        CancellationToken cancellationToken = default)
    {
        return await LoadProgramAsync(
            request,
            ChartProgramRequest.MainProgram(TranslateDisplayMode(displayMode)),
            cancellationToken);
    }

    public async Task<VNextMainChartLoadResult> LoadProgramAsync(
        MetricLoadRequest request,
        ChartProgramRequest programRequest,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(programRequest);

        try
        {
            var coordinator = _coordinatorFactory();

            coordinator.ApplyMetricType(request.MetricType);
            coordinator.ApplySeries(TranslateSelections(request.SelectedSeries));
            coordinator.ApplyDateRange(request.From, request.To);
            coordinator.ApplyResolution(request.ResolutionTableName);

            if (programRequest.Kind == ChartProgramKind.Main)
                coordinator.ApplyMainDisplayMode(programRequest.DisplayMode);

            if (programRequest.SeriesOperations.Count > 0)
                coordinator.ApplyWorkflowPlan(new WorkflowPlanRequest(
                    programRequest.SeriesOperations,
                    programRequest.Kind.ToString(),
                    programRequest.TitleOverride));

            var snapshot = await coordinator.LoadAsync(cancellationToken);
            var program = coordinator.BuildProgram(programRequest);
            var projectedContext = _projector.ProjectToChartContext(program);

            return new VNextMainChartLoadResult(
                Success: true,
                ProjectedContext: projectedContext,
                RequestSignature: snapshot.Request.Signature,
                SnapshotSignature: snapshot.Signature,
                ProgramKind: program.Kind,
                ProgramSourceSignature: program.SourceSignature,
                ProjectedContextSignature: projectedContext.LoadRequestSignature,
                FailureReason: null);
        }
        catch (Exception ex)
        {
            return new VNextMainChartLoadResult(
                Success: false,
                ProjectedContext: null,
                RequestSignature: request.Signature,
                SnapshotSignature: null,
                ProgramKind: null,
                ProgramSourceSignature: null,
                ProjectedContextSignature: null,
                FailureReason: ex.Message);
        }
    }

    internal static IReadOnlyList<MetricSeriesRequest> TranslateSelections(IReadOnlyList<MetricSeriesSelection> selections)
    {
        return selections.Select(MetricSeriesRequest.FromLegacy).ToList();
    }

    internal static ChartDisplayMode TranslateDisplayMode(MainChartDisplayMode mode)
    {
        return mode switch
        {
            MainChartDisplayMode.Regular => ChartDisplayMode.Regular,
            MainChartDisplayMode.Summed => ChartDisplayMode.Summed,
            MainChartDisplayMode.Stacked => ChartDisplayMode.Stacked,
            _ => ChartDisplayMode.Regular
        };
    }
}
