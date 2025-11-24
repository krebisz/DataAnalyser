using DataFileReader.Class;
using DataFileReader.Helper;

namespace DataFileReader.Services;

public class MetricAggregator
{
    public void Aggregate(string metricType, AggregationPeriod period)
    {
        var metricSubtypes = new List<string?>
        {
            string.Empty,
            "basal_metabolic_rate",
            "body_fat",
            "body_fat_mass",
            "fat_free",
            "fat_free_mass",
            "height",
            "skeletal_muscle",
            "skeletal_muscle_mass",
            "total_body_water",
            "weight"
        };

        var action = GetAggregationAction(period);

        foreach (var subtype in metricSubtypes)
        {
            var normalized = string.IsNullOrEmpty(subtype) ? null : subtype;
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
            AggregationPeriod.Week =>
                (t, s, f, e) =>
                    SQLHelper.InsertHealthMetricsWeek(t, s, f, e, overwriteExisting: true),

            AggregationPeriod.Month =>
                (t, s, f, e) =>
                    SQLHelper.InsertHealthMetricsMonth(t, s, f, e, overwriteExisting: true),

            _ => throw new NotSupportedException($"{period} not supported.")
        };
    }
}
