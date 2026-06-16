using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Computation.TimeSeries;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Tests.Core.Computation;

public sealed class TimeSeriesComputationTests
{
    [Fact]
    public void TimeSeriesPreparation_ShouldAlignSmoothAndComputeDerivedSeries()
    {
        var from = new DateTime(2026, 1, 1);
        var primary = new List<MetricData>
        {
            new() { NormalizedTimestamp = from, Value = 10m },
            new() { NormalizedTimestamp = from.AddDays(2), Value = 30m }
        };
        var secondary = new List<MetricData>
        {
            new() { NormalizedTimestamp = from.AddDays(1), Value = 5m }
        };

        var result = TimeSeriesPreparation.Prepare(primary, secondary, smoothingWindow: 1);

        Assert.Equal([from.Date, from.AddDays(1).Date, from.AddDays(2).Date], result.Timestamps);
        Assert.Equal([10d, 10d, 30d], result.RawValues1);
        Assert.Equal([0d, 5d, 5d], result.RawValues2);
        Assert.Equal([10d, 5d, 25d], result.DifferenceValues);
        Assert.Equal([0d, 2d, 6d], result.RatioValues);
        Assert.Equal([10d / 30d, 10d / 30d, 1d], result.NormalizedValues1);
    }

    [Fact]
    public void RunningAverageCalculator_ShouldCalculateCumulativeAndWindowedAverages()
    {
        var from = new DateTime(2026, 1, 1);
        var points = new[]
        {
            new TimeSeriesPoint(from, 2d),
            new TimeSeriesPoint(from.AddDays(1), 4d),
            new TimeSeriesPoint(from.AddDays(10), 10d)
        };

        var cumulative = RunningAverageCalculator.Calculate(points, window: null);
        var windowed = RunningAverageCalculator.Calculate(points, TimeSpan.FromDays(2));

        Assert.Equal([2d, 3d, 16d / 3d], cumulative.Select(point => point.Value));
        Assert.Equal([2d, 3d, 10d], windowed.Select(point => point.Value));
    }

    [Fact]
    public void CorrelationCalculator_ShouldCalculatePearsonAndConfidenceInterval()
    {
        var result = CorrelationCalculator.Pearson([1d, 2d, 3d, 4d], [2d, 4d, 6d, 8d]);

        Assert.Equal(1d, result.Correlation, precision: 8);
        Assert.Equal(4, result.SampleCount);
        Assert.True(double.IsFinite(result.ConfidenceLower));
        Assert.True(double.IsFinite(result.ConfidenceUpper));
    }

    [Fact]
    public void SeriesMath_AverageFinite_ShouldIgnoreInvalidValues()
    {
        var result = SeriesMath.AverageFinite([1d, double.NaN, double.PositiveInfinity, 3d]);

        Assert.Equal(2d, result);
    }

    [Fact]
    public void CumulativeSeriesCalculator_ShouldAccumulateSeries()
    {
        var timeline = new List<DateTime> { new(2026, 1, 1), new(2026, 1, 2) };
        var series = new List<SeriesResult>
        {
            new() { SeriesId = "a", DisplayName = "A", Timestamps = timeline, RawValues = [1d, 2d], Smoothed = [1d, 2d] },
            new() { SeriesId = "b", DisplayName = "B", Timestamps = timeline, RawValues = [3d, 4d], Smoothed = [3d, 4d] }
        };

        var (renderSeries, originalSeries) = CumulativeSeriesCalculator.BuildFromSeries(series);

        Assert.NotNull(renderSeries);
        Assert.NotNull(originalSeries);
        Assert.Equal([1d, 2d], renderSeries![0].RawValues);
        Assert.Equal([4d, 6d], renderSeries[1].RawValues);
        Assert.Equal([3d, 4d], originalSeries![1].RawValues);
    }

    [Fact]
    public void ComputedSeriesResultBuilder_ShouldBuildResultFromMetricTimeline()
    {
        var timeline = new[]
        {
            new MetricData { NormalizedTimestamp = new DateTime(2026, 1, 1), Value = 1m },
            new MetricData { NormalizedTimestamp = new DateTime(2026, 1, 2), Value = 2m }
        };

        var result = ComputedSeriesResultBuilder.Build("id", "Label", timeline, [1d, 2d], [1d, 2d], [timeline], "op");

        Assert.Equal("id", result.Id);
        Assert.Equal("Label", result.Label);
        Assert.Equal(timeline.Select(point => point.NormalizedTimestamp), result.Timeline);
        Assert.Equal(["input-0"], result.SourceSeriesSignatures);
        Assert.Equal("op", result.OperationSignature);
    }
}
