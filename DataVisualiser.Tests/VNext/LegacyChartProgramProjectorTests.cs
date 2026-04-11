using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Application;

namespace DataVisualiser.Tests.VNext;

public sealed class LegacyChartProgramProjectorTests
{
    [Fact]
    public void ProjectToChartContext_ShouldPreserveSignatureAndSeries()
    {
        var projector = new LegacyChartProgramProjector();
        var program = new ChartProgram(
            ChartProgramKind.Main,
            ChartDisplayMode.Regular,
            "Weight",
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 2),
            [new DateTime(2026, 1, 1), new DateTime(2026, 1, 2)],
            [
                new ChartSeriesProgram("a", "Morning", [1d, 2d], [1d, 2d]),
                new ChartSeriesProgram("b", "Evening", [3d, 4d], [3d, 4d])
            ],
            "sig-1");

        var context = projector.ProjectToChartContext(program);

        Assert.Equal("sig-1", context.LoadRequestSignature);
        Assert.Equal(2, context.ActualSeriesCount);
        Assert.Equal(2, context.Data1?.Count);
        Assert.Equal(2, context.Data2?.Count);
        Assert.Equal("Morning", context.DisplayName1);
        Assert.Equal("Evening", context.DisplayName2);
    }
}
