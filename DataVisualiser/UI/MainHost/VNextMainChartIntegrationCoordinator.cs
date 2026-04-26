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
    private readonly AnalyticalIntentFactory _intentFactory = new();
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
            var selectionRequest = BuildSelectionRequest(request);
            var intent = _intentFactory.Create(
                selectionRequest,
                programRequest,
                ResolveDeliveryTarget(programRequest.Kind));
            var execution = await coordinator.ExecuteAsync(intent, cancellationToken);
            var snapshot = execution.Snapshot;
            var program = execution.Program;
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

    private static MetricSelectionRequest BuildSelectionRequest(MetricLoadRequest request)
    {
        return new MetricSelectionRequest(
            request.MetricType,
            TranslateSelections(request.SelectedSeries),
            request.From,
            request.To,
            request.ResolutionTableName);
    }

    private static string? ResolveDeliveryTarget(ChartProgramKind kind)
    {
        return kind switch
        {
            ChartProgramKind.Main => "MainChart",
            ChartProgramKind.Normalized => "NormalizedChart",
            ChartProgramKind.Difference or ChartProgramKind.Ratio => "DiffRatioChart",
            ChartProgramKind.Transform => "TransformChart",
            ChartProgramKind.Distribution => "DistributionChart",
            ChartProgramKind.WeekdayTrend => "WeekdayTrendChart",
            ChartProgramKind.BarPie => "BarPieChart",
            _ => null
        };
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
