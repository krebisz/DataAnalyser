using DataFileReader.Class;
using DataFileReader.Helper;

namespace DataFileReader.Services;

public class MetricAggregator
{
    public void Aggregate(string metricType, AggregationPeriod period)
    {
        // Dynamically get all subtypes for this metric type from the database
        var metricSubtypes = SQLHelper.GetSubtypesForMetricType(metricType);

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
                var dateRange = SQLHelper.GetDateRangeForMetric(metricType, normalized);

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
            AggregationPeriod.Day => (t, s, f, e) => SQLHelper.InsertHealthMetricsDay(t, s, f, e, false),

            AggregationPeriod.Week => (t, s, f, e) => SQLHelper.InsertHealthMetricsWeek(t, s, f, e, false),

            AggregationPeriod.Month => (t, s, f, e) => SQLHelper.InsertHealthMetricsMonth(t, s, f, e, false),

            _ => throw new NotSupportedException($"{period} not supported.")
        };
    }
}