using DataFileReader.Models;
using DataFileReader.Services;

namespace DataFileReader.Tests;

public sealed class MetricAggregatorTests
{
    [Fact]
    public void Aggregate_SkipsWhenNoSubtypesExist()
    {
        var repository = new FakeMetricCatalogRepository();
        var writer = new FakeMetricAggregationWriter();
        var aggregator = new MetricAggregator(repository, writer);

        aggregator.Aggregate("weight", AggregationPeriod.Day);

        Assert.Empty(writer.DayCalls);
        Assert.Equal(["weight"], repository.SubtypeRequests);
    }

    [Fact]
    public void Aggregate_ProcessesEachSubtypeWithAvailableDateRange()
    {
        var repository = new FakeMetricCatalogRepository
        {
            SubtypesByMetricType =
            {
                ["weight"] = [null, "fat_mass", "skeletal_mass"]
            },
            DateRanges =
            {
                [("weight", null)] = (new DateTime(2026, 1, 1), new DateTime(2026, 1, 31)),
                [("weight", "fat_mass")] = (new DateTime(2026, 2, 1), new DateTime(2026, 2, 28)),
                [("weight", "skeletal_mass")] = null
            }
        };
        var writer = new FakeMetricAggregationWriter();
        var aggregator = new MetricAggregator(repository, writer);

        aggregator.Aggregate("weight", AggregationPeriod.Week);

        Assert.Equal(
        [
            ("weight", null, new DateTime(2026, 1, 1), new DateTime(2026, 1, 31)),
            ("weight", "fat_mass", new DateTime(2026, 2, 1), new DateTime(2026, 2, 28))
        ], writer.WeekCalls);
    }

    [Fact]
    public void Aggregate_UsesPeriodSpecificWriter()
    {
        var repository = new FakeMetricCatalogRepository
        {
            SubtypesByMetricType =
            {
                ["sleep"] = ["rem"]
            },
            DateRanges =
            {
                [("sleep", "rem")] = (new DateTime(2026, 3, 1), new DateTime(2026, 3, 2))
            }
        };
        var writer = new FakeMetricAggregationWriter();
        var aggregator = new MetricAggregator(repository, writer);

        aggregator.Aggregate("sleep", AggregationPeriod.Month);

        Assert.Single(writer.MonthCalls);
        Assert.Empty(writer.DayCalls);
        Assert.Empty(writer.WeekCalls);
    }

    private sealed class FakeMetricCatalogRepository : IMetricCatalogRepository
    {
        public Dictionary<string, List<string?>> SubtypesByMetricType { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<(string MetricType, string? MetricSubtype), (DateTime MinDate, DateTime MaxDate)?> DateRanges { get; } = new();
        public List<string> SubtypeRequests { get; } = [];

        public List<string> GetAllMetricTypes() => [];

        public List<string?> GetSubtypesForMetricType(string metricType)
        {
            SubtypeRequests.Add(metricType);
            return SubtypesByMetricType.TryGetValue(metricType, out var subtypes) ? subtypes : [];
        }

        public (DateTime MinDate, DateTime MaxDate)? GetDateRangeForMetric(string metricType, string? metricSubtype = null)
        {
            return DateRanges.TryGetValue((metricType, metricSubtype), out var range) ? range : null;
        }
    }

    private sealed class FakeMetricAggregationWriter : IMetricAggregationWriter
    {
        public List<(string MetricType, string? MetricSubtype, DateTime FromDate, DateTime ToDate)> DayCalls { get; } = [];
        public List<(string MetricType, string? MetricSubtype, DateTime FromDate, DateTime ToDate)> WeekCalls { get; } = [];
        public List<(string MetricType, string? MetricSubtype, DateTime FromDate, DateTime ToDate)> MonthCalls { get; } = [];

        public void InsertDay(string metricType, string? metricSubtype, DateTime fromDate, DateTime toDate)
        {
            DayCalls.Add((metricType, metricSubtype, fromDate, toDate));
        }

        public void InsertWeek(string metricType, string? metricSubtype, DateTime fromDate, DateTime toDate)
        {
            WeekCalls.Add((metricType, metricSubtype, fromDate, toDate));
        }

        public void InsertMonth(string metricType, string? metricSubtype, DateTime fromDate, DateTime toDate)
        {
            MonthCalls.Add((metricType, metricSubtype, fromDate, toDate));
        }
    }
}
