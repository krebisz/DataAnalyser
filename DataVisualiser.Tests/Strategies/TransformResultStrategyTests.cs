using System;
using System.Collections.Generic;
using System.Linq;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Models;
using Xunit;

namespace DataVisualiser.Tests.Strategies
{
    public sealed class TransformResultStrategyTests
    {
        private static readonly DateTime From = new(2024, 01, 01);
        private static readonly DateTime To = new(2024, 01, 05);

        private static List<HealthMetricData> BuildSourceData()
        {
            return new List<HealthMetricData>
            {
                new() { NormalizedTimestamp = From, Value = 10m, Unit = "kg" },
                new() { NormalizedTimestamp = From.AddDays(1), Value = 20m, Unit = "kg" },
                new() { NormalizedTimestamp = From.AddDays(2), Value = 30m, Unit = "kg" }
            };
        }

        [Fact]
        public void Compute_ShouldReturnNull_WhenSourceDataEmpty()
        {
            var strategy = new TransformResultStrategy(
                new List<HealthMetricData>(),
                new List<double>(),
                "X",
                From,
                To);

            var result = strategy.Compute();

            Assert.Null(result);
        }

        [Fact]
        public void Compute_ShouldPreserveTimestamps()
        {
            var data = BuildSourceData();
            var values = new List<double> { 1, 2, 3 };

            var strategy = new TransformResultStrategy(
                data,
                values,
                "X",
                From,
                To);

            var result = strategy.Compute();

            Assert.NotNull(result);
            Assert.Equal(
                data.Select(d => d.NormalizedTimestamp),
                result!.Timestamps);
        }

        [Fact]
        public void Compute_ShouldUseProvidedTransformedValues()
        {
            var data = BuildSourceData();
            var values = new List<double> { 100, 200, 300 };

            var strategy = new TransformResultStrategy(
                data,
                values,
                "X",
                From,
                To);

            var result = strategy.Compute();

            Assert.NotNull(result);
            Assert.Equal(values, result!.PrimaryRawValues);
        }

        [Fact]
        public void Compute_ShouldGenerateSmoothedSeries()
        {
            var data = BuildSourceData();
            var values = new List<double> { 10, 20, 30 };

            var strategy = new TransformResultStrategy(
                data,
                values,
                "X",
                From,
                To);

            var result = strategy.Compute();

            Assert.NotNull(result);
            Assert.Equal(values.Count, result!.PrimarySmoothed.Count);
        }

        [Fact]
        public void Compute_ShouldSetPrimaryLabel()
        {
            var data = BuildSourceData();
            var values = new List<double> { 1, 2, 3 };

            var strategy = new TransformResultStrategy(
                data,
                values,
                "Transformed(A)",
                From,
                To);

            var result = strategy.Compute();

            Assert.NotNull(result);
            Assert.Equal("Transformed(A)", strategy.PrimaryLabel);
        }

        [Fact]
        public void Compute_ShouldNotProduceSecondarySeries()
        {
            var data = BuildSourceData();
            var values = new List<double> { 1, 2, 3 };

            var strategy = new TransformResultStrategy(
                data,
                values,
                "X",
                From,
                To);

            var result = strategy.Compute();

            Assert.NotNull(result);
            Assert.Null(result!.SecondaryRawValues);
            Assert.Null(result.SecondarySmoothed);
        }

        [Fact]
        public void Strategy_ShouldHaveNullUnit()
        {
            var data = BuildSourceData();
            var values = new List<double> { 1, 2, 3 };

            var strategy = new TransformResultStrategy(
                data,
                values,
                "X",
                From,
                To);

            Assert.Null(strategy.Unit);
        }

    }
}
