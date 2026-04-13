using DataFileReader.Canonical;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.VNext.Application;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.UI.MainHost;

public sealed record VNextDistributionLoadResult(
    bool Success,
    IReadOnlyList<MetricData>? Data,
    ICanonicalMetricSeries? CmsSeries,
    string? DisplayName,
    string? RequestSignature,
    string? SnapshotSignature,
    ChartProgramKind? ProgramKind,
    string? ProgramSourceSignature,
    string? FailureReason);

public sealed class VNextDistributionIntegrationCoordinator
{
    private readonly Func<ReasoningSessionCoordinator> _coordinatorFactory;
    private readonly LegacyChartProgramProjector _projector = new();

    public VNextDistributionIntegrationCoordinator(MetricSelectionService metricSelectionService)
    {
        ArgumentNullException.ThrowIfNull(metricSelectionService);
        _coordinatorFactory = () => ReasoningEngineFactory.CreateCoordinator(metricSelectionService);
    }

    internal VNextDistributionIntegrationCoordinator(Func<ReasoningSessionCoordinator> coordinatorFactory)
    {
        _coordinatorFactory = coordinatorFactory ?? throw new ArgumentNullException(nameof(coordinatorFactory));
    }

    public async Task<VNextDistributionLoadResult> LoadDistributionAsync(
        MetricSeriesSelection series,
        DateTime from,
        DateTime to,
        string resolutionTableName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(series);

        if (string.IsNullOrWhiteSpace(series.MetricType))
            return CreateFailure(null, "Distribution series has no metric type.");

        try
        {
            var coordinator = _coordinatorFactory();

            coordinator.ApplyMetricType(series.MetricType);
            coordinator.ApplySeries([MetricSeriesRequest.FromLegacy(series)]);
            coordinator.ApplyDateRange(from, to);
            coordinator.ApplyResolution(resolutionTableName);

            var snapshot = await coordinator.LoadAsync(cancellationToken);
            var program = coordinator.BuildProgram(ChartProgramRequest.Distribution());
            var projectedContext = _projector.ProjectToChartContext(program);

            var seriesSnapshot = snapshot.Series.Count > 0 ? snapshot.Series[0] : null;

            return new VNextDistributionLoadResult(
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
            var requestSignature = BuildRequestSignature(series, from, to, resolutionTableName);
            return CreateFailure(requestSignature, ex.Message);
        }
    }

    private static VNextDistributionLoadResult CreateFailure(string? requestSignature, string failureReason)
    {
        return new VNextDistributionLoadResult(
            Success: false,
            Data: null,
            CmsSeries: null,
            DisplayName: null,
            RequestSignature: requestSignature,
            SnapshotSignature: null,
            ProgramKind: null,
            ProgramSourceSignature: null,
            FailureReason: failureReason);
    }

    private static string BuildRequestSignature(MetricSeriesSelection series, DateTime from, DateTime to, string tableName)
    {
        return $"dist::{series.MetricType}::{tableName}::{from:O}->{to:O}::{series.MetricType}:{series.QuerySubtype ?? "<none>"}";
    }
}
