using DataFileReader.Class;
using DataFileReader.Helper;
using DataFileReader.Services;
using Microsoft.Extensions.Logging;
using System.Configuration;

namespace DataFileReader.App;

public class HealthDataApp
{
    private readonly MetricAggregator _aggregator;
    private readonly FileProcessingService _fileProcessor;
    private readonly ILogger<MetricAggregator> _logger;

    public HealthDataApp(
        MetricAggregator aggregator,
        FileProcessingService fileProcessor)
    {
        _aggregator = aggregator;
        _fileProcessor = fileProcessor;
    }

    public void Run()
    {
        Console.WriteLine("Health Data Reader - Starting...");
        Console.WriteLine("================================");

        EnsureDatabaseTables();

        if (AskUserYesNo("Aggregate all metrics by week and month?"))
        {
            var allMetricTypes = SQLHelper.GetAllMetricTypes();

            if (allMetricTypes.Count == 0)
            {
                Console.WriteLine("No metric types found in database. Skipping aggregation.");
            }
            else
            {
                Console.WriteLine($"Found {allMetricTypes.Count} metric type(s) to aggregate:");
                foreach (var metricType in allMetricTypes)
                {
                    Console.WriteLine($"  - {metricType}");
                }
                Console.WriteLine();

                foreach (var metricType in allMetricTypes)
                {
                    Console.WriteLine($"\n=== Aggregating {metricType} ===");
                    _aggregator.Aggregate(metricType, AggregationPeriod.Week);
                    _aggregator.Aggregate(metricType, AggregationPeriod.Month);
                }

                Console.WriteLine("\n=== Aggregation Complete ===");
            }
        }

        var rootDirectory = ConfigurationManager.AppSettings["RootDirectory"];
        var allFiles = FileHelper.GetFileList(rootDirectory);

        // Get list of already processed files to avoid duplicates
        var processedFiles = SQLHelper.GetProcessedFiles();
        var newFiles = allFiles.Where(file => !processedFiles.Contains(file)).ToList();

        var totalFiles = allFiles.Count;
        var skippedFiles = totalFiles - newFiles.Count;

        if (skippedFiles > 0)
        {
            Console.WriteLine($"Skipping {skippedFiles} already processed file(s)");
        }

        Console.WriteLine($"Processing {newFiles.Count} new file(s)...");

        var result = _fileProcessor.ProcessFiles(newFiles);

        Console.WriteLine("\n================================");
        Console.WriteLine("Processing complete!");
        Console.WriteLine($"Files processed: {result.FilesProcessed}");
        Console.WriteLine($"Total metrics inserted: {result.MetricsInserted}");
    }

    private void EnsureDatabaseTables()
    {
        try
        {
            SQLHelper.EnsureHealthMetricsTableExists();
            Console.WriteLine("✓ HealthMetrics table verified/created");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting up database: {ex.Message}");
            throw;
        }
    }

    private bool AskUserYesNo(string prompt)
    {
        Console.WriteLine($"{prompt} (y/n): ");
        var input = Console.ReadLine()?.Trim().ToLower();

        return input == "y" || input == "yes" || input == "1";
    }
}
