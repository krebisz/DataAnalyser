using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Kernel;

namespace DataVisualiser.VNext.Application;

public sealed class ChartProgramPlanner
{
    private readonly TimeSeriesAlignmentKernel _alignmentKernel;
    private readonly OperationKernel _operationKernel;

    public ChartProgramPlanner(TimeSeriesAlignmentKernel alignmentKernel, OperationKernel operationKernel)
    {
        _alignmentKernel = alignmentKernel ?? throw new ArgumentNullException(nameof(alignmentKernel));
        _operationKernel = operationKernel ?? throw new ArgumentNullException(nameof(operationKernel));
    }

    public ChartProgram BuildProgram(MetricLoadSnapshot snapshot, ChartProgramRequest request)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(request);

        return request.Kind switch
        {
            ChartProgramKind.Main => BuildMainProgram(snapshot, request.DisplayMode),
            ChartProgramKind.Normalized => BuildNormalizedProgram(snapshot),
            ChartProgramKind.Difference => BuildDifferenceProgram(snapshot),
            ChartProgramKind.Ratio => BuildRatioProgram(snapshot),
            ChartProgramKind.Transform when request.SeriesOperations.Count > 0 => BuildDerivedProgram(snapshot, request),
            ChartProgramKind.Transform => BuildIdentityProgram(snapshot, ChartProgramKind.Transform),
            ChartProgramKind.Distribution => BuildDistributionProgram(snapshot),
            ChartProgramKind.WeekdayTrend => BuildIdentityProgram(snapshot, ChartProgramKind.WeekdayTrend),
            ChartProgramKind.BarPie => BuildIdentityProgram(snapshot, ChartProgramKind.BarPie),
            _ => throw new InvalidOperationException($"Unsupported program kind '{request.Kind}'.")
        };
    }

    public ChartProgram BuildMainProgram(MetricLoadSnapshot snapshot, ChartDisplayMode displayMode = ChartDisplayMode.Regular)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var aligned = _alignmentKernel.Align(snapshot);
        var series = displayMode == ChartDisplayMode.Summed
            ? new[] { _operationKernel.BuildSummedSeries(aligned) }
            : aligned.Series.Select((series, index) => _operationKernel.BuildSeries(
                    aligned,
                    SeriesOperationRequest.Identity(index, series.Request.SignatureToken, series.Request.DisplayName)))
                .ToArray();

        return new ChartProgram(
            ChartProgramKind.Main,
            displayMode,
            snapshot.Request.MetricType,
            snapshot.Request.From,
            snapshot.Request.To,
            aligned.Timeline,
            series,
            snapshot.Signature);
    }

    public ChartProgram BuildNormalizedProgram(MetricLoadSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var aligned = _alignmentKernel.Align(snapshot);
        var series = aligned.Series
            .Select(_operationKernel.BuildNormalizedSeries)
            .ToArray();

        return new ChartProgram(
            ChartProgramKind.Normalized,
            ChartDisplayMode.Regular,
            $"{snapshot.Request.MetricType} (normalized)",
            snapshot.Request.From,
            snapshot.Request.To,
            aligned.Timeline,
            series,
            snapshot.Signature);
    }

    public ChartProgram BuildDifferenceProgram(MetricLoadSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var aligned = _alignmentKernel.Align(snapshot);
        if (aligned.Series.Count < 2)
            throw new InvalidOperationException("Difference program requires at least two series.");

        return new ChartProgram(
            ChartProgramKind.Difference,
            ChartDisplayMode.Regular,
            $"{snapshot.Request.MetricType} difference",
            snapshot.Request.From,
            snapshot.Request.To,
            aligned.Timeline,
            new[] { _operationKernel.BuildDifferenceSeries(aligned.Series[0], aligned.Series[1]) },
            snapshot.Signature);
    }

    public ChartProgram BuildRatioProgram(MetricLoadSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var aligned = _alignmentKernel.Align(snapshot);
        if (aligned.Series.Count < 2)
            throw new InvalidOperationException("Ratio program requires at least two series.");

        return new ChartProgram(
            ChartProgramKind.Ratio,
            ChartDisplayMode.Regular,
            $"{snapshot.Request.MetricType} ratio",
            snapshot.Request.From,
            snapshot.Request.To,
            aligned.Timeline,
            new[] { _operationKernel.BuildRatioSeries(aligned.Series[0], aligned.Series[1]) },
            snapshot.Signature);
    }

    public ChartProgram BuildDistributionProgram(MetricLoadSnapshot snapshot)
    {
        return BuildIdentityProgram(snapshot, ChartProgramKind.Distribution);
    }

    public ChartProgram BuildIdentityProgram(MetricLoadSnapshot snapshot, ChartProgramKind kind)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var aligned = _alignmentKernel.Align(snapshot);
        var series = aligned.Series
            .Select((alignedSeries, index) => _operationKernel.BuildSeries(
                aligned,
                SeriesOperationRequest.Identity(index, alignedSeries.Request.SignatureToken, alignedSeries.Request.DisplayName)))
            .ToArray();

        return new ChartProgram(
            kind,
            ChartDisplayMode.Regular,
            snapshot.Request.MetricType,
            snapshot.Request.From,
            snapshot.Request.To,
            aligned.Timeline,
            series,
            snapshot.Signature);
    }

    private ChartProgram BuildDerivedProgram(MetricLoadSnapshot snapshot, ChartProgramRequest request)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(request);
        if (request.SeriesOperations.Count == 0)
            throw new InvalidOperationException("Derived program requires at least one series operation.");

        var aligned = _alignmentKernel.Align(snapshot);
        var series = request.SeriesOperations
            .Select(operation => _operationKernel.BuildSeries(aligned, operation))
            .ToArray();

        return new ChartProgram(
            request.Kind,
            request.DisplayMode,
            request.TitleOverride ?? $"{snapshot.Request.MetricType} derived",
            snapshot.Request.From,
            snapshot.Request.To,
            aligned.Timeline,
            series,
            snapshot.Signature);
    }
}
