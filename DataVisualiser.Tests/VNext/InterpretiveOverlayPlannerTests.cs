using DataVisualiser.VNext.Application;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.Tests.VNext;

public sealed class InterpretiveOverlayPlannerTests
{
    [Fact]
    public void PlanAverageLines_ShouldCreateNonAuthoritativeOverlayPerFiniteSeries()
    {
        var program = CreateProgram(
            new ChartSeriesProgram("morning", "Morning", [1d, 2d, 3d], [1d, 2d, 3d]),
            new ChartSeriesProgram("empty", "Empty", [double.NaN], [double.NaN]));

        var overlays = new InterpretiveOverlayPlanner().PlanAverageLines(program);

        var overlay = Assert.Single(overlays);
        Assert.Equal(OverlayKind.AverageLine, overlay.Kind);
        Assert.Equal("Morning Average", overlay.Label);
        Assert.Equal("morning", overlay.ResolvedParameters["SeriesId"]);
        Assert.Equal("2", overlay.ResolvedParameters["Value"]);
        Assert.Equal("False", overlay.ResolvedParameters["Authoritative"]);
        Assert.Equal(program.SourceSignature, overlay.ResolvedParameters["SourceSignature"]);
    }

    [Fact]
    public void PlanMedianLines_ShouldIgnoreNonFiniteValues()
    {
        var program = CreateProgram(
            new ChartSeriesProgram("morning", "Morning", [10d, double.NaN, 4d, 6d], [10d, double.NaN, 4d, 6d]));

        var overlay = Assert.Single(new InterpretiveOverlayPlanner().PlanMedianLines(program));

        Assert.Equal(OverlayKind.MedianLine, overlay.Kind);
        Assert.Equal("6", overlay.ResolvedParameters["Value"]);
    }

    [Fact]
    public void PlanAverageLines_ShouldNotMutateProgramSeries()
    {
        var series = new ChartSeriesProgram("morning", "Morning", [1d, 2d, 3d], [1d, 2d, 3d]);
        var program = CreateProgram(series);

        _ = new InterpretiveOverlayPlanner().PlanAverageLines(program);

        Assert.Equal([1d, 2d, 3d], program.Series[0].RawValues);
        Assert.Empty(program.Series[0].RawValues.OfType<object>().Except(series.RawValues.Cast<object>()));
    }

    private static ChartProgram CreateProgram(params ChartSeriesProgram[] series)
    {
        var from = new DateTime(2026, 1, 1);
        return new ChartProgram(
            ChartProgramKind.Main,
            ChartDisplayMode.Regular,
            "Weight",
            from,
            from.AddDays(2),
            [from, from.AddDays(1), from.AddDays(2)],
            series,
            "source-signature");
    }
}
