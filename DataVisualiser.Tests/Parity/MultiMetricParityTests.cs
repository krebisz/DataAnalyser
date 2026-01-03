using System;
using System.Collections.Generic;
using System.Linq;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;
using Xunit;

namespace DataVisualiser.Tests.Parity
{
    public sealed class MultiMetricParityTests
    {
        private static readonly DateTime From = new(2024, 01, 01);
        private static readonly DateTime To = new(2024, 01, 10);

        [Fact]
        public void Parity_ShouldPass_WithThreeMetrics()
        {
            var legacySeries = CreateLegacySeries(3, 10);
            AssertParity(legacySeries, new[] { "A", "B", "C" });
        }

        [Fact]
        public void Parity_ShouldPass_WithEmptyData()
        {
            var legacySeries = new List<IEnumerable<HealthMetricData>>
            {
                Enumerable.Empty<HealthMetricData>(),
                Enumerable.Empty<HealthMetricData>()
            };

            AssertParity(legacySeries, new[] { "A", "B" });
        }

        [Fact]
        public void Parity_ShouldPass_WithMismatchedCounts()
        {
            var legacySeries = new List<IEnumerable<HealthMetricData>>
            {
                TestDataBuilders.HealthMetricData()
                    .WithUnit("kg")
                    .BuildSeries(12, TimeSpan.FromDays(1)),

                TestDataBuilders.HealthMetricData()
                    .WithUnit("kg")
                    .BuildSeries(8, TimeSpan.FromDays(1))
            };

            AssertParity(legacySeries, new[] { "A", "B" });
        }

        private static List<IEnumerable<HealthMetricData>> CreateLegacySeries(int seriesCount, int pointsPerSeries)
        {
            var result = new List<IEnumerable<HealthMetricData>>();

            for (int i = 0; i < seriesCount; i++)
            {
                result.Add(
                    TestDataBuilders.HealthMetricData()
                        .WithUnit("kg")
                        .BuildSeries(pointsPerSeries, TimeSpan.FromDays(1)));
            }

            return result;
        }

        private static void AssertParity(IReadOnlyList<IEnumerable<HealthMetricData>> legacySeries, IReadOnlyList<string> labels)
        {
            var legacyStrategy = new MultiMetricStrategy(
                legacySeries,
                labels,
                From,
                To);

            var legacyResult = legacyStrategy.Compute();

            const string sharedMetricId = "metric.test.multi";

            var cmsSeries = legacySeries
                .Select(series =>
                    TestDataBuilders.CanonicalMetricSeries()
                        .WithMetricId(sharedMetricId)
                        .WithUnit("kg")
                        .WithSampleCount(series.Count())
                        .Build())
                .ToList();

            var cmsStrategy = new MultiMetricStrategy(
                cmsSeries,
                labels,
                From,
                To);

            var cmsResult = cmsStrategy.Compute();

            if (legacyResult == null || cmsResult == null)
            {
                Assert.Null(legacyResult);
                Assert.Null(cmsResult);
                return;
            }

            Assert.Equal(legacyResult.Series.Count, cmsResult.Series.Count);

            for (int i = 0; i < legacyResult.Series.Count; i++)
            {
                var legacySeriesResult = legacyResult.Series[i];
                var cmsSeriesResult = cmsResult.Series[i];

                Assert.Equal(legacySeriesResult.Timestamps, cmsSeriesResult.Timestamps);
                Assert.Equal(legacySeriesResult.RawValues, cmsSeriesResult.RawValues);
                Assert.Equal(legacySeriesResult.Smoothed, cmsSeriesResult.Smoothed);
            }
        }
    }
}
