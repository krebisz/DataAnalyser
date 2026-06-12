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
                new AgnosticJsonParser(processHierarchy: true),
                new SamsungCsvParser(),
        };

        var metricOnlyParsers = new IHealthFileParser[]
        {
                new AgnosticJsonParser(processHierarchy: false),
                new SamsungCsvParser(),
        };

        var metricCatalogRepository = new SqlMetricCatalogRepository();
        var processedFileRegistry = new SqlProcessedFileRegistry();
        var maintenanceService = new SqlHealthMetricsMaintenanceService();
        var healthMetricWriter = new SqlHealthMetricWriter();
        var aggregationWriter = new SqlMetricAggregationWriter();
        var aggregator = new MetricAggregator(metricCatalogRepository, aggregationWriter);
        var issueLogger = new DataFileReaderIssueFileLogger();
        var metricOnlyIssueLogger = new DataFileReaderIssueFileLogger();
        var fileProcessor = new FileProcessingService(parsers, healthMetricWriter, processedFileRegistry, issueLogger);
        var metricOnlyFileProcessor = new FileProcessingService(metricOnlyParsers, healthMetricWriter, processedFileRegistry, metricOnlyIssueLogger);

        var app = new HealthDataApp(aggregator, fileProcessor, metricOnlyFileProcessor, metricCatalogRepository, maintenanceService, processedFileRegistry);

        app.Run();

        if (!Console.IsInputRedirected)
        {
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
