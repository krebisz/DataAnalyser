using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.VNext.Application;

public sealed class InterpretiveOverlayPlanner
{
    public IReadOnlyList<OverlayPlan> PlanAverageLines(ChartProgram program)
    {
        ArgumentNullException.ThrowIfNull(program);

        return program.Series
            .Select(series => TryBuildReferenceLine(program, series, OverlayKind.AverageLine, ResolveAverage))
            .Where(plan => plan != null)
            .Select(plan => plan!)
            .ToArray();
    }

    public IReadOnlyList<OverlayPlan> PlanMedianLines(ChartProgram program)
    {
        ArgumentNullException.ThrowIfNull(program);

        return program.Series
            .Select(series => TryBuildReferenceLine(program, series, OverlayKind.MedianLine, ResolveMedian))
            .Where(plan => plan != null)
            .Select(plan => plan!)
            .ToArray();
    }

    private static OverlayPlan? TryBuildReferenceLine(
        ChartProgram program,
        ChartSeriesProgram series,
        OverlayKind kind,
        Func<IReadOnlyList<double>, double?> resolver)
    {
        var value = resolver(series.RawValues);
        if (value == null)
            return null;

        var labelKind = kind == OverlayKind.AverageLine ? "Average" : "Median";
        return new OverlayPlan(
            kind,
            $"{series.Label} {labelKind}",
            new Dictionary<string, string>
            {
                ["ProgramKind"] = program.Kind.ToString(),
                ["SourceSignature"] = program.SourceSignature,
                ["SeriesId"] = series.Id,
                ["SeriesLabel"] = series.Label,
                ["Value"] = value.Value.ToString("R"),
                ["Authoritative"] = "False"
            });
    }

    private static double? ResolveAverage(IReadOnlyList<double> values)
    {
        var finite = values.Where(double.IsFinite).ToArray();
        return finite.Length == 0 ? null : finite.Average();
    }

    private static double? ResolveMedian(IReadOnlyList<double> values)
    {
        var finite = values.Where(double.IsFinite).Order().ToArray();
        if (finite.Length == 0)
            return null;

        var mid = finite.Length / 2;
        return finite.Length % 2 == 0
            ? (finite[mid - 1] + finite[mid]) / 2d
            : finite[mid];
    }
}
