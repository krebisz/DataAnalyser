using System;
using System.Collections.Generic;
using System.Linq;
using DataVisualiser.Charts.Strategies;
using DataVisualiser.Models;
using DataVisualiser.Tests.Helpers;
using Xunit;

namespace DataVisualiser.Tests.Strategies
{
    public sealed class DifferenceStrategyTests
    {
        private static readonly DateTime From = new(2024, 01, 01);
        private static readonly DateTime To = new(2024, 01, 10);

        [Fact]
        public void Compute_ShouldReturnNull_WhenBothSeriesEmpty()
        {
            var left = Enumerable.Empty<HealthMetricData>();
            var right = Enumerable.Empty<HealthMetricData>();

            var strategy = new DifferenceStrategy(left, right, "L", "R", From, To);

            var result = strategy.Compute();

            Assert.Null(result);
        }

        [Fact]
        public void Compute_ShouldReturnNull_WhenOneSeriesEmpty()
        {
            var left = TestDataBuilders.HealthMetricData()
                .WithTimestamp(From)
                .WithUnit("kg")
                .BuildSeries(5, TimeSpan.FromDays(1));

            var right = Enumerable.Empty<HealthMetricData>();

            var strategy = new DifferenceStrategy(left, right, "L", "R", From, To);

            var result = strategy.Compute();

            Assert.Null(result);
        }

        [Fact]
        public void Compute_ShouldAlignByIndex_UsingShortestSeries()
        {
            var left = TestDataBuilders.HealthMetricData()
                .WithTimestamp(From)
                .WithUnit("kg")
                .BuildSeries(8, TimeSpan.FromDays(1));

            var right = TestDataBuilders.HealthMetricData()
                .WithTimestamp(From)
                .WithUnit("kg")
                .BuildSeries(5, TimeSpan.FromDays(1));

            var strategy = new DifferenceStrategy(left, right, "L", "R", From, To);

            var result = strategy.Compute();

            Assert.NotNull(result);
            Assert.Equal(5, result!.PrimaryRawValues.Count);
            Assert.Equal(5, result.Timestamps.Count);
        }

        [Fact]
        public void Compute_ShouldCalculateLeftMinusRight()
        {
            var left = new List<HealthMetricData>
            {
                new() { NormalizedTimestamp = From, Value = 10m, Unit = "kg" },
                new() { NormalizedTimestamp = From.AddDays(1), Value = 20m, Unit = "kg" }
            };

            var right = new List<HealthMetricData>
            {
                new() { NormalizedTimestamp = From, Value = 3m, Unit = "kg" },
                new() { NormalizedTimestamp = From.AddDays(1), Value = 5m, Unit = "kg" }
            };

            var strategy = new DifferenceStrategy(left, right, "L", "R", From, To);

            var result = strategy.Compute();

            Assert.NotNull(result);
            Assert.Equal(new[] { 7.0, 15.0 }, result!.PrimaryRawValues);
        }

        [Fact]
        public void Compute_ShouldIgnoreNullValues_WhenFilteringAndAligning()
        {
            var left = new List<HealthMetricData>
    {
        new() { NormalizedTimestamp = From, Value = 10m, Unit = "kg" },
        new() { NormalizedTimestamp = From.AddDays(1), Value = null, Unit = "kg" },
        new() { NormalizedTimestamp = From.AddDays(2), Value = 20m, Unit = "kg" }
    };

            var right = new List<HealthMetricData>
    {
        new() { NormalizedTimestamp = From, Value = 3m, Unit = "kg" },
        new() { NormalizedTimestamp = From.AddDays(1), Value = 5m, Unit = "kg" },
        new() { NormalizedTimestamp = From.AddDays(2), Value = 7m, Unit = "kg" }
    };

            var strategy = new DifferenceStrategy(left, right, "L", "R", From, To);

            var result = strategy.Compute();

            Assert.NotNull(result);
            Assert.Equal(2, result!.PrimaryRawValues.Count);
            Assert.Equal(new[] { 7.0, 15.0 }, result.PrimaryRawValues);
            Assert.DoesNotContain(result.PrimaryRawValues, double.IsNaN);
        }


        [Fact]
        public void Compute_ShouldGenerateSmoothedValues()
        {
            var left = TestDataBuilders.HealthMetricData()
                .WithTimestamp(From)
                .WithUnit("kg")
                .BuildSeries(10, TimeSpan.FromDays(1));

            var right = TestDataBuilders.HealthMetricData()
                .WithTimestamp(From)
                .WithUnit("kg")
                .BuildSeries(10, TimeSpan.FromDays(1));

            var strategy = new DifferenceStrategy(left, right, "L", "R", From, To);

            var result = strategy.Compute();

            Assert.NotNull(result);
            Assert.NotNull(result!.PrimarySmoothed);
            Assert.Equal(result.PrimaryRawValues.Count, result.PrimarySmoothed.Count);
        }

        [Fact]
        public void Compute_ShouldResolveUnit_WhenUnitsMatch()
        {
            var left = TestDataBuilders.HealthMetricData()
                .WithTimestamp(From)
                .WithUnit("kg")
                .BuildSeries(5, TimeSpan.FromDays(1));

            var right = TestDataBuilders.HealthMetricData()
                .WithTimestamp(From)
                .WithUnit("kg")
                .BuildSeries(5, TimeSpan.FromDays(1));

            var strategy = new DifferenceStrategy(left, right, "L", "R", From, To);

            var result = strategy.Compute();

            Assert.NotNull(result);
            Assert.Equal("kg", result!.Unit);
        }

        [Fact]
        public void Compute_ShouldPreferLeftUnit_WhenUnitsDiffer()
        {
            var left = TestDataBuilders.HealthMetricData()
                .WithTimestamp(From)
                .WithUnit("kg")
                .BuildSeries(5, TimeSpan.FromDays(1));

            var right = TestDataBuilders.HealthMetricData()
                .WithTimestamp(From)
                .WithUnit("lb")
                .BuildSeries(5, TimeSpan.FromDays(1));

            var strategy = new DifferenceStrategy(left, right, "L", "R", From, To);

            var result = strategy.Compute();

            Assert.NotNull(result);
            Assert.Equal("kg", result!.Unit);
        }
    }
}
