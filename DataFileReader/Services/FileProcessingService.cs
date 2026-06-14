using DataFileReader.Helper;
using DataFileReader.Parsers;

namespace DataFileReader.Services;

public class FileProcessingService
{
    private readonly IHealthMetricWriter _healthMetricWriter;
    private readonly IFileProcessingIssueLogger _issueLogger;
    private readonly IEnumerable<IHealthFileParser> _parsers;
    private readonly IProcessedFileRegistry _processedFileRegistry;

    public FileProcessingService(IEnumerable<IHealthFileParser> parsers)
        : this(parsers, new SqlHealthMetricWriter(), new SqlProcessedFileRegistry(), new DataFileReaderIssueFileLogger())
    {
    }

    public FileProcessingService(IEnumerable<IHealthFileParser> parsers, IHealthMetricWriter healthMetricWriter, IProcessedFileRegistry processedFileRegistry)
        : this(parsers, healthMetricWriter, processedFileRegistry, NullFileProcessingIssueLogger.Instance)
    {
    }

    public FileProcessingService(
        IEnumerable<IHealthFileParser> parsers,
        IHealthMetricWriter healthMetricWriter,
        IProcessedFileRegistry processedFileRegistry,
        IFileProcessingIssueLogger issueLogger)
    {
        _parsers = parsers ?? throw new ArgumentNullException(nameof(parsers));
        _healthMetricWriter = healthMetricWriter ?? throw new ArgumentNullException(nameof(healthMetricWriter));
        _processedFileRegistry = processedFileRegistry ?? throw new ArgumentNullException(nameof(processedFileRegistry));
        _issueLogger = issueLogger ?? throw new ArgumentNullException(nameof(issueLogger));
    }

    public FileProcessingResult ProcessFiles(IEnumerable<string> fileList)
    {
        var processed = 0;
        var metricsInserted = 0;

        foreach (var file in fileList)
            try
            {
                var fileInfo = new FileInfo(file);
                var parser = _parsers.FirstOrDefault(p => p.CanParse(fileInfo));

                if (parser == null)
                {
                    _issueLogger.LogUnsupportedFile(file);
                    Console.WriteLine($"Unsupported file type: {FileHelper.GetFileName(file)}");
                    continue;
                }

                var content = File.ReadAllText(file);
                var preflight = FileProcessingPreflightValidator.Validate(fileInfo, content);
                if (!preflight.IsValid)
                {
                    _issueLogger.LogUnprocessableFile(file, preflight.Reason);
                    Console.WriteLine($"Unprocessable file: {FileHelper.GetFileName(file)} - {preflight.Reason}");
                    continue;
                }

                var metrics = parser.Parse(file, content);

                // Only run shadow validation for CSV files
                if (fileInfo.Extension.Equals(".csv", StringComparison.OrdinalIgnoreCase))
                    ShadowValidate_SamsungHealthCsv.Run(file, content);

                if (metrics.Count > 0)
                {
                    _healthMetricWriter.InsertHealthMetrics(metrics);
                    metricsInserted += metrics.Count;
                    Console.WriteLine($"Inserted {metrics.Count} metrics from {FileHelper.GetFileName(file)}");
                }
                else
                {
                    // Mark empty files as processed to avoid reprocessing them
                    _processedFileRegistry.MarkFileAsProcessed(file);
                    _issueLogger.LogNoMetrics(file);
                    Console.WriteLine($"No metrics found in {FileHelper.GetFileName(file)} - marked as processed");
                }

                processed++;
            }
            catch (Exception ex)
            {
                _issueLogger.LogProcessingError(file, ex);
                Console.WriteLine($"Error processing {file}: {ex.Message}");
            }

        return new FileProcessingResult(
            processed,
            metricsInserted,
            _issueLogger.HasEntries ? _issueLogger.LogFilePath : null,
            _issueLogger.HasEntries);
    }

    private sealed class NullFileProcessingIssueLogger : IFileProcessingIssueLogger
    {
        public static readonly NullFileProcessingIssueLogger Instance = new();

        private NullFileProcessingIssueLogger()
        {
        }

        public string LogFilePath => string.Empty;

        public bool HasEntries => false;

        public void LogUnsupportedFile(string filePath)
        {
        }

        public void LogNoMetrics(string filePath)
        {
        }

        public void LogUnprocessableFile(string filePath, string reason)
        {
        }

        public void LogProcessingError(string filePath, Exception exception)
        {
        }
    }
}
