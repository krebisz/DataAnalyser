using DataVisualiser.Shared.Models;
using DataVisualiser.VNext.Application;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Kernel;

namespace DataVisualiser.Tests.VNext;

public sealed class ChartProgramPlannerTests
{
    [Fact]
    public void BuildMainProgram_ShouldReturnOneSeriesPerSnapshotSeries()
    {
        var planner = new ChartProgramPlanner(new TimeSeriesAlignmentKernel(), new OperationKernel());
        var snapshot = CreateSnapshot();

        var program = planner.BuildMainProgram(snapshot);

        Assert.Equal(ChartProgramKind.Main, program.Kind);
        Assert.Equal(2, program.Series.Count);
        Assert.Equal(snapshot.Signature, program.SourceSignature);
    }

    [Fact]
    public void BuildMainProgram_Summed_ShouldCollapseToOneSeries()
    {
        var planner = new ChartProgramPlanner(new TimeSeriesAlignmentKernel(), new OperationKernel());
        var snapshot = CreateSnapshot();

        var program = planner.BuildMainProgram(snapshot, ChartDisplayMode.Summed);

        Assert.Single(program.Series);
        Assert.Equal("sum", program.Series[0].Id);
        Assert.Equal([3d, 5d], program.Series[0].RawValues);
    }

    [Fact]
    public void BuildDifferenceProgram_ShouldSubtractFirstTwoSeries()
    {
        var planner = new ChartProgramPlanner(new TimeSeriesAlignmentKernel(), new OperationKernel());
        var snapshot = CreateSnapshot();

        var program = planner.BuildDifferenceProgram(snapshot);

        Assert.Equal(ChartProgramKind.Difference, program.Kind);
        Assert.Single(program.Series);
        Assert.Equal([-1d, -1d], program.Series[0].RawValues);
    }

    [Fact]
    public void BuildProgram_Transform_ShouldSupportMultipleDerivedSeries()
    {
        var planner = new ChartProgramPlanner(new TimeSeriesAlignmentKernel(), new OperationKernel());
        var snapshot = CreateSnapshot();
        var request = ChartProgramRequest.Transform(
            "Weight transform",
            [
                SeriesOperationRequest.Difference(0, 1, "Delta"),
                SeriesOperationRequest.Ratio(0, 1, "Ratio")
            ]);

        var program = planner.BuildProgram(snapshot, request);

        Assert.Equal(ChartProgramKind.Transform, program.Kind);
        Assert.Equal("Weight transform", program.Title);
        Assert.Equal(2, program.Series.Count);
        Assert.Equal("Delta", program.Series[0].Label);
        Assert.Equal([-1d, -1d], program.Series[0].RawValues);
        Assert.Equal([0.5d, 2d / 3d], program.Series[1].RawValues);
    }

    private static MetricLoadSnapshot CreateSnapshot()
    {
        var request = new MetricSelectionRequest(
            "Weight",
            [new MetricSeriesRequest("Weight", "morning"), new MetricSeriesRequest("Weight", "evening")],
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 2),
            "HealthMetrics");

        return new MetricLoadSnapshot(
            request,
            [
                new MetricSeriesSnapshot(
                    request.Series[0],
                    [
                        new MetricData { NormalizedTimestamp = new DateTime(2026, 1, 1), Value = 1m },
                        new MetricData { NormalizedTimestamp = new DateTime(2026, 1, 2), Value = 2m }
                    ],
                    null),
                new MetricSeriesSnapshot(
                    request.Series[1],
                    [
                        new MetricData { NormalizedTimestamp = new DateTime(2026, 1, 1), Value = 2m },
                        new MetricData { NormalizedTimestamp = new DateTime(2026, 1, 2), Value = 3m }
                    ],
                    null)
            ],
            DateTime.UtcNow);
    }
}
