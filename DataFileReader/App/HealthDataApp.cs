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

        if (AskUserYesNo("Aggregate Weight metrics by week and month?"))
        {
            _aggregator.Aggregate("Weight", AggregationPeriod.Week);
            _aggregator.Aggregate("Weight", AggregationPeriod.Month);
        }

        var rootDirectory = ConfigurationManager.AppSettings["RootDirectory"];
        var files = FileHelper.GetFileList(rootDirectory);

        var result = _fileProcessor.ProcessFiles(files);

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
