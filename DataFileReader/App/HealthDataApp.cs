using System.Configuration;
using DataFileReader.Class;
using DataFileReader.Helper;
using DataFileReader.Services;

namespace DataFileReader.App;

public sealed class HealthDataApp
{
    private readonly MetricAggregator _aggregator;
    private readonly FileProcessingService _fileProcessor;

    public HealthDataApp(
        MetricAggregator aggregator,
        FileProcessingService fileProcessor)
    {
        _aggregator = aggregator ?? throw new ArgumentNullException(nameof(aggregator));
        _fileProcessor = fileProcessor ?? throw new ArgumentNullException(nameof(fileProcessor));
    }

    public void Run()
    {
        WriteHeader();
        EnsureDatabaseTables();

        RunMainLoop();
    }

    // ----------------------------
    // Main control flow
    // ----------------------------

    private void RunMainLoop()
    {
        while (true)
        {
            switch (ReadOption())
            {
                case AppOption.AggregateMetrics:
                    AggregateData();
                    break;

                case AppOption.ProcessFiles:
                    ProcessFiles();
                    break;

                case AppOption.RemoveJunkFiles:
                    DeleteJunkFiles();
                    break;

                case AppOption.Exit:
                    Console.WriteLine("Exiting application.");
                    return;

                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }

    // ----------------------------
    // Menu & input
    // ----------------------------

    private static void WriteHeader()
    {
        Console.WriteLine("HEALTH DATA READER - Starting...");
        Console.WriteLine("================================");
    }

    private static AppOption ReadOption()
    {
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  1. Aggregate Metrics");
        Console.WriteLine("  2. Process New Files");
        Console.WriteLine("  3. Remove Junk Files");
        Console.WriteLine("  4. Exit");
        Console.Write("> ");

        return Console.ReadLine()?.Trim() switch
        {
            "1" => AppOption.AggregateMetrics,
            "2" => AppOption.ProcessFiles,
            "3" => AppOption.RemoveJunkFiles,
            "4" => AppOption.Exit,
            _ => AppOption.Invalid
        };
    }

    // ----------------------------
    // Operations
    // ----------------------------

    private void EnsureDatabaseTables()
    {
        try
        {
            SQLHelper.EnsureHealthMetricsTableExists();
            Console.WriteLine("✓ HealthMetrics table verified/created");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal database error: {ex.Message}");
            throw;
        }
    }

    private void AggregateData()
    {
        var metricTypes = SQLHelper.GetAllMetricTypes();

        if (metricTypes.Count == 0)
        {
            Console.WriteLine("No metric types found. Skipping aggregation.");
            return;
        }

        Console.WriteLine($"Found {metricTypes.Count} metric type(s):");
        foreach (var metricType in metricTypes)
            Console.WriteLine($"  - {metricType}");

        foreach (var metricType in metricTypes)
        {
            Console.WriteLine($"\n=== Aggregating {metricType} ===");

            _aggregator.Aggregate(metricType, AggregationPeriod.Day);
            _aggregator.Aggregate(metricType, AggregationPeriod.Week);
            _aggregator.Aggregate(metricType, AggregationPeriod.Month);
        }

        Console.WriteLine("\n=== Aggregation Complete ===");
    }

    private void ProcessFiles()
    {
        var rootDirectory = GetRootDirectory();
        if (rootDirectory is null)
            return;

        var allFiles = FileHelper.GetFileList(rootDirectory);
        var processedFiles = SQLHelper.GetProcessedFiles();

        var newFiles = allFiles
            .Where(f => !processedFiles.Contains(f))
            .ToList();

        var skipped = allFiles.Count - newFiles.Count;
        if (skipped > 0)
            Console.WriteLine($"Skipping {skipped} already processed file(s)");

        Console.WriteLine($"Processing {newFiles.Count} new file(s)...");

        var result = _fileProcessor.ProcessFiles(newFiles);

        Console.WriteLine("\n================================");
        Console.WriteLine("Processing complete!");
        Console.WriteLine($"Files processed: {result.FilesProcessed}");
        Console.WriteLine($"Total metrics inserted: {result.MetricsInserted}");
    }

    private void DeleteJunkFiles()
    {
        var rootDirectory = GetRootDirectory();
        if (rootDirectory is null)
            return;

        var files = FileHelper.GetFileList(rootDirectory);
        var deleted = FileHelper.DeleteEmptyFiles(files);

        Console.WriteLine($"Removed: {deleted} empty file(s).");
    }

    // ----------------------------
    // Helpers
    // ----------------------------

    private static string? GetRootDirectory()
    {
        var rootDirectory = ConfigurationManager.AppSettings["RootDirectory"];

        if (string.IsNullOrWhiteSpace(rootDirectory))
        {
            Console.WriteLine("RootDirectory is not configured.");
            return null;
        }

        return rootDirectory;
    }

    private enum AppOption
    {
        Invalid,
        AggregateMetrics,
        ProcessFiles,
        RemoveJunkFiles,
        Exit
    }
}
