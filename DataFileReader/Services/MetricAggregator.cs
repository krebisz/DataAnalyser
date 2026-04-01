using DataFileReader.Models;

namespace DataFileReader.Services;

public class MetricAggregator
{
    private readonly IMetricCatalogRepository _metricCatalogRepository;
    private readonly IMetricAggregationWriter _metricAggregationWriter;

    public MetricAggregator(IMetricCatalogRepository metricCatalogRepository, IMetricAggregationWriter metricAggregationWriter)
    {
        _metricCatalogRepository = metricCatalogRepository ?? throw new ArgumentNullException(nameof(metricCatalogRepository));
        _metricAggregationWriter = metricAggregationWriter ?? throw new ArgumentNullException(nameof(metricAggregationWriter));
    }

    public MetricAggregator()
        : this(new SqlMetricCatalogRepository(), new SqlMetricAggregationWriter())
    {
    }

    public void Aggregate(string metricType, AggregationPeriod period)
    {
        // Dynamically get all subtypes for this metric type from the database
        var metricSubtypes = _metricCatalogRepository.GetSubtypesForMetricType(metricType);

        if (metricSubtypes.Count == 0)
        {
            Console.WriteLine($"No subtypes found for metric type {metricType}, skipping...");
            return;
        }

        var action = GetAggregationAction(period);

        foreach (var subtype in metricSubtypes)
        {
            var normalized = subtype; // Already normalized by SQLHelper (null for empty)
            var label = normalized ?? "(no subtype)";

            try
            {
                var dateRange = _metricCatalogRepository.GetDateRangeForMetric(metricType, normalized);

                if (!dateRange.HasValue)
                {
                    Console.WriteLine($"No data for {metricType}/{label}, skipping...");
                    continue;
                }

                var (from, to) = dateRange.Value;

                Console.WriteLine($"Processing {metricType}/{label} [{period}]...");
                Console.WriteLine($"  Range: {from:yyyy-MM-dd} to {to:yyyy-MM-dd}");

                action(metricType, normalized, from, to);

                Console.WriteLine($"  ✓ Completed {metricType}/{label} [{period}]");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing {metricType}/{label}: {ex.Message}");
            }
        }
    }

    private Action<string, string?, DateTime, DateTime> GetAggregationAction(AggregationPeriod period)
    {
        return period switch
        {
                AggregationPeriod.Day => (t, s, f, e) => _metricAggregationWriter.InsertDay(t, s, f, e),

                AggregationPeriod.Week => (t, s, f, e) => _metricAggregationWriter.InsertWeek(t, s, f, e),

                AggregationPeriod.Month => (t, s, f, e) => _metricAggregationWriter.InsertMonth(t, s, f, e),

                _ => throw new NotSupportedException($"{period} not supported.")
        };
    }
}
