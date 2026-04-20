using DataVisualiser.Core.Validation.Parity;

namespace DataVisualiser.Tests.Parity;

public sealed class ParitySeriesComparerTests
{
    [Fact]
    public void Compare_ShouldAllowNaNValuesOnBothSides()
    {
        var context = new StrategyParityContext();
        var series = CreateSeries(double.NaN);

        var result = ParitySeriesComparer.Compare(context, series, series);

        Assert.True(result.Passed);
    }

    [Fact]
    public void Compare_ShouldRespectFloatingPointTolerance()
    {
        var context = new StrategyParityContext
        {
            Tolerance = new ParityTolerance
            {
                AllowFloatingPointDrift = true,
                ValueEpsilon = 0.01
            }
        };

        var result = ParitySeriesComparer.Compare(
            context,
            CreateSeries(1.0),
            CreateSeries(1.005));

        Assert.True(result.Passed);
    }

    [Fact]
    public void Compare_ShouldThrowOnStrictModeFailure()
    {
        var context = new StrategyParityContext
        {
            Mode = ParityMode.Strict
        };

        Assert.Throws<InvalidOperationException>(() =>
            ParitySeriesComparer.Compare(context, CreateSeries(1.0), CreateSeries(2.0)));
    }

    private static IReadOnlyList<ParitySeries> CreateSeries(double value)
    {
        return
        [
            new ParitySeries
            {
                SeriesKey = "Primary",
                Points =
                [
                    new ParityPoint
                    {
                        Time = new DateTime(2026, 1, 1),
                        Value = value
                    }
                ]
            }
        ];
    }
}
