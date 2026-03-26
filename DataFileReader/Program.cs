using DataFileReader.App;
using DataFileReader.Parsers;
using DataFileReader.Services;

namespace DataFileReader;

internal class Program
{
    private static void Main(string[] args)
    {
        var parsers = new IHealthFileParser[]
        {
                new SamsungJsonParser(),
                new SamsungCsvParser(),
                new LegacyJsonParser()
        };

        var metricCatalogRepository = new SqlMetricCatalogRepository();
        var processedFileRegistry = new SqlProcessedFileRegistry();
        var maintenanceService = new SqlHealthMetricsMaintenanceService();
        var healthMetricWriter = new SqlHealthMetricWriter();
        var aggregationWriter = new SqlMetricAggregationWriter();
        var aggregator = new MetricAggregator(metricCatalogRepository, aggregationWriter);
        var fileProcessor = new FileProcessingService(parsers, healthMetricWriter, processedFileRegistry);

        var app = new HealthDataApp(aggregator, fileProcessor, metricCatalogRepository, maintenanceService, processedFileRegistry);

        app.Run();

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
