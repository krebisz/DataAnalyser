namespace DataVisualiser.VNext.Contracts;

public sealed record CapabilityRequest(
    AnalyticalCapabilityKind CapabilityKind,
    CompositionKind CompositionKind,
    IReadOnlyList<SeriesOperationRequest>? Operations = null)
{
    public IReadOnlyList<SeriesOperationRequest> ResolvedOperations { get; } =
        Operations?.ToArray() ?? Array.Empty<SeriesOperationRequest>();

    public string Signature =>
        $"{CapabilityKind}:{CompositionKind}:{string.Join("|", ResolvedOperations.Select(operation => $"{operation.Kind}:{operation.Id}"))}";

    public static CapabilityRequest FromProgramRequest(ChartProgramRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.Kind switch
        {
            ChartProgramKind.Main => new CapabilityRequest(
                AnalyticalCapabilityKind.Identity,
                request.DisplayMode == ChartDisplayMode.Summed ? CompositionKind.SingleSeries : CompositionKind.MultiSeries,
                request.SeriesOperations),
            ChartProgramKind.Normalized => new CapabilityRequest(
                AnalyticalCapabilityKind.Normalization,
                CompositionKind.MultiSeries,
                request.SeriesOperations),
            ChartProgramKind.Difference or ChartProgramKind.Ratio => new CapabilityRequest(
                AnalyticalCapabilityKind.Comparison,
                CompositionKind.DerivedSeries,
                request.SeriesOperations),
            ChartProgramKind.Transform => new CapabilityRequest(
                AnalyticalCapabilityKind.Transform,
                request.SeriesOperations.Count == 0 ? CompositionKind.MultiSeries : CompositionKind.DerivedSeries,
                request.SeriesOperations),
            ChartProgramKind.Distribution => new CapabilityRequest(
                AnalyticalCapabilityKind.Distribution,
                CompositionKind.SingleSeries,
                request.SeriesOperations),
            ChartProgramKind.WeekdayTrend => new CapabilityRequest(
                AnalyticalCapabilityKind.TemporalTrend,
                CompositionKind.SingleSeries,
                request.SeriesOperations),
            ChartProgramKind.BarPie => new CapabilityRequest(
                AnalyticalCapabilityKind.Identity,
                CompositionKind.MultiSeries,
                request.SeriesOperations),
            ChartProgramKind.SyncfusionSunburst => new CapabilityRequest(
                AnalyticalCapabilityKind.Hierarchy,
                CompositionKind.Hierarchy,
                request.SeriesOperations),
            ChartProgramKind.MovingAverage => new CapabilityRequest(
                AnalyticalCapabilityKind.Smoothing,
                request.SeriesOperations.Count == 0 ? CompositionKind.SingleSeries : CompositionKind.DerivedSeries,
                request.SeriesOperations),
            _ => throw new InvalidOperationException($"Unsupported program kind '{request.Kind}'.")
        };
    }
}
