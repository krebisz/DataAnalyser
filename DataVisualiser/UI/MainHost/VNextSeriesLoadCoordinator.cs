using DataFileReader.Canonical;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.VNext.Application;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.UI.MainHost;

public sealed record VNextSeriesLoadResult(
    bool Success,
    IReadOnlyList<MetricData>? Data,
    ICanonicalMetricSeries? CmsSeries,
    string? DisplayName,
    string? RequestSignature,
    string? SnapshotSignature,
    ChartProgramKind? ProgramKind,
    string? ProgramSourceSignature,
    string? FailureReason);

public sealed class VNextSeriesLoadCoordinator
{
    private readonly Func<ReasoningSessionCoordinator> _coordinatorFactory;
    private readonly LegacyChartProgramProjector _projector = new();

    public VNextSeriesLoadCoordinator(MetricSelectionService metricSelectionService)
    {
        ArgumentNullException.ThrowIfNull(metricSelectionService);
        _coordinatorFactory = () => ReasoningEngineFactory.CreateCoordinator(metricSelectionService);
    }

    internal VNextSeriesLoadCoordinator(Func<ReasoningSessionCoordinator> coordinatorFactory)
    {
        _coordinatorFactory = coordinatorFactory ?? throw new ArgumentNullException(nameof(coordinatorFactory));
    }

    public async Task<VNextSeriesLoadResult> LoadAsync(
        MetricSeriesSelection series,
        DateTime from,
        DateTime to,
        string resolutionTableName,
        ChartProgramKind programKind,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(series);

        if (string.IsNullOrWhiteSpace(series.MetricType))
            return CreateFailure(programKind, null, "Series has no metric type.");

        try
        {
            var coordinator = _coordinatorFactory();

            coordinator.ApplyMetricType(series.MetricType);
            coordinator.ApplySeries([MetricSeriesRequest.FromLegacy(series)]);
            coordinator.ApplyDateRange(from, to);
            coordinator.ApplyResolution(resolutionTableName);

            var snapshot = await coordinator.LoadAsync(cancellationToken);
            var request = new ChartProgramRequest(programKind);
            var program = coordinator.BuildProgram(request);
            var projectedContext = _projector.ProjectToChartContext(program);

            var seriesSnapshot = snapshot.Series.Count > 0 ? snapshot.Series[0] : null;

            return new VNextSeriesLoadResult(
                Success: true,
                Data: projectedContext.Data1,
                CmsSeries: seriesSnapshot?.CanonicalSeries,
                DisplayName: series.DisplayName,
                RequestSignature: snapshot.Request.Signature,
                SnapshotSignature: snapshot.Signature,
                ProgramKind: program.Kind,
                ProgramSourceSignature: program.SourceSignature,
                FailureReason: null);
        }
        catch (Exception ex)
        {
            var requestSignature = $"{programKind}::{series.MetricType}::{resolutionTableName}::{from:O}->{to:O}::{series.MetricType}:{series.QuerySubtype ?? "<none>"}";
            return CreateFailure(programKind, requestSignature, ex.Message);
        }
    }

    private static VNextSeriesLoadResult CreateFailure(ChartProgramKind programKind, string? requestSignature, string failureReason)
    {
        return new VNextSeriesLoadResult(
            Success: false,
            Data: null,
            CmsSeries: null,
            DisplayName: null,
            RequestSignature: requestSignature,
            SnapshotSignature: null,
            ProgramKind: programKind,
            ProgramSourceSignature: null,
            FailureReason: failureReason);
    }
}
