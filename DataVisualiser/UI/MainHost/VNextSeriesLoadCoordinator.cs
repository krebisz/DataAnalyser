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
    private readonly AnalyticalIntentFactory _intentFactory = new();

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
            var request = new ChartProgramRequest(programKind);
            var selectionRequest = new MetricSelectionRequest(
                series.MetricType,
                [MetricSeriesRequest.FromLegacy(series)],
                from,
                to,
                resolutionTableName);
            var intent = _intentFactory.Create(
                selectionRequest,
                request,
                ChartProgramDeliveryTargetResolver.ResolveDefaultTarget(programKind));
            var execution = await coordinator.ExecuteAsync(intent, cancellationToken);
            var snapshot = execution.Snapshot;
            var program = execution.Program;

            var seriesSnapshot = snapshot.Series.Count > 0 ? snapshot.Series[0] : null;

            return new VNextSeriesLoadResult(
                Success: true,
                Data: BuildMetricData(program),
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

    private static IReadOnlyList<MetricData> BuildMetricData(ChartProgram program)
    {
        var primarySeries = program.Series.Count > 0 ? program.Series[0] : null;
        if (primarySeries == null)
            return Array.Empty<MetricData>();

        var result = new List<MetricData>(Math.Min(program.Timeline.Count, primarySeries.RawValues.Count));
        for (var index = 0; index < Math.Min(program.Timeline.Count, primarySeries.RawValues.Count); index++)
        {
            result.Add(new MetricData
            {
                NormalizedTimestamp = program.Timeline[index],
                Value = double.IsNaN(primarySeries.RawValues[index]) ? null : Convert.ToDecimal(primarySeries.RawValues[index])
            });
        }

        return result;
    }

}
