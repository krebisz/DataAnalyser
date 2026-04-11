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

    public ChartProgram BuildMainProgram(MetricLoadSnapshot snapshot, ChartDisplayMode displayMode = ChartDisplayMode.Regular)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var aligned = _alignmentKernel.Align(snapshot);
        var series = displayMode == ChartDisplayMode.Summed
            ? new[] { _operationKernel.BuildSummedSeries(aligned) }
            : aligned.Series.Select(series => new ChartSeriesProgram(
                    series.Request.SignatureToken,
                    series.Request.DisplayName,
                    series.RawValues,
                    series.SmoothedValues))
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
}
