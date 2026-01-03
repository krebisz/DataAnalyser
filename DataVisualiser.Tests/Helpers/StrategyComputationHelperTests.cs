using System;
using System.Collections.Generic;
using System.Linq;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;
using Xunit;

namespace DataVisualiser.Tests.Helpers
{
    public sealed class StrategyComputationHelperTests
    {
        private static readonly DateTime From = new(2024, 01, 01);
        private static readonly DateTime To = new(2024, 01, 05);

        [Fact]
        public void PrepareDataForComputation_ShouldReturnNull_WhenBothInputsEmpty()
        {
            var result = StrategyComputationHelper.PrepareDataForComputation(
                Enumerable.Empty<HealthMetricData>(),
                Enumerable.Empty<HealthMetricData>(),
                From,
                To);

            Assert.Null(result);
        }

        [Fact]
        public void PrepareDataForComputation_ShouldOrderAndFilterInputs()
        {
            var left = new List<HealthMetricData>
            {
                new() { NormalizedTimestamp = From.AddDays(2), Value = 30 },
                new() { NormalizedTimestamp = From, Value = null },
                new() { NormalizedTimestamp = From.AddDays(1), Value = 20 }
            };

            var right = new List<HealthMetricData>
            {
                new() { NormalizedTimestamp = From, Value = 5 },
                new() { NormalizedTimestamp = From.AddDays(1), Value = 10 }
            };

            var result = StrategyComputationHelper.PrepareDataForComputation(
                left,
                right,
                From,
                To);

            Assert.NotNull(result);

            var ordered1 = result!.Value.Item1;
            var ordered2 = result.Value.Item2;

            Assert.Equal(2, ordered1.Count);
            Assert.Equal(2, ordered2.Count);
            Assert.True(ordered1.Select(x => x.NormalizedTimestamp)
                                .SequenceEqual(ordered1.Select(x => x.NormalizedTimestamp)
                                                       .OrderBy(t => t)));
        }

        [Fact]
        public void CombineTimestamps_ShouldUnionAndOrder_FromHealthMetricData()
        {
            var left = new List<HealthMetricData>
            {
                new() { NormalizedTimestamp = From },
                new() { NormalizedTimestamp = From.AddDays(1) }
            };

            var right = new List<HealthMetricData>
            {
                new() { NormalizedTimestamp = From.AddDays(1) },
                new() { NormalizedTimestamp = From.AddDays(2) }
            };

            var combined = StrategyComputationHelper.CombineTimestamps(left, right);

            Assert.Equal(3, combined.Count);
            Assert.True(combined.SequenceEqual(combined.OrderBy(t => t)));
        }

        [Fact]
        public void ExtractAlignedRawValues_ShouldReturnPairedLists_WithNaNForMissing()
        {
            var timestamps = new List<DateTime> { From, From.AddDays(1) };

            var dict1 = new Dictionary<DateTime, double>
            {
                { From, 10 }
            };

            var dict2 = new Dictionary<DateTime, double>
            {
                { From.AddDays(1), 20 }
            };

            var (raw1, raw2) =
                StrategyComputationHelper.ExtractAlignedRawValues(
                    timestamps,
                    dict1,
                    dict2);

            Assert.Equal(2, raw1.Count);
            Assert.True(double.IsNaN(raw1[1]));
            Assert.True(double.IsNaN(raw2[0]));
        }

        [Fact]
        public void ProcessSmoothedData_ShouldReturnPairedSmoothedLists()
        {
            var left = new List<HealthMetricData>
            {
                new() { NormalizedTimestamp = From, Value = 10 },
                new() { NormalizedTimestamp = From.AddDays(1), Value = 20 }
            };

            var right = new List<HealthMetricData>
            {
                new() { NormalizedTimestamp = From, Value = 5 },
                new() { NormalizedTimestamp = From.AddDays(1), Value = 15 }
            };

            var timestamps = StrategyComputationHelper.CombineTimestamps(left, right);

            var (s1, s2) =
                StrategyComputationHelper.ProcessSmoothedData(
                    left,
                    right,
                    timestamps,
                    From,
                    To);

            Assert.Equal(2, s1.Count);
            Assert.Equal(2, s2.Count);
        }
    }
}
