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
