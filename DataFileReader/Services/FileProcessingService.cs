using DataFileReader.Helper;
using DataFileReader.Parsers;
using Microsoft.Extensions.Logging;

namespace DataFileReader.Services;

public class FileProcessingService
{
    private readonly IEnumerable<IHealthFileParser> _parsers;
    private readonly ILogger<MetricAggregator> _logger;

    public FileProcessingService(IEnumerable<IHealthFileParser> parsers)
    {
        _parsers = parsers;
    }

    public FileProcessingResult ProcessFiles(IEnumerable<string> fileList)
    {
        var processed = 0;
        var metricsInserted = 0;

        foreach (var file in fileList)
        {
            try
            {
                var parser = _parsers.FirstOrDefault(p => p.CanParse(new FileInfo(file)));

                if (parser == null)
                {
                    Console.WriteLine($"Unsupported file type: {FileHelper.GetFileName(file)}");
                    continue;
                }

                var content = File.ReadAllText(file);
                var metrics = parser.Parse(file, content);

                ShadowValidate_SamsungHealthCsv.Run(file, content);

                if (metrics.Count > 0)
                {
                    SQLHelper.InsertHealthMetrics(metrics);
                    metricsInserted += metrics.Count;
                    Console.WriteLine($"Inserted {metrics.Count} metrics from {FileHelper.GetFileName(file)}");
                }

                processed++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing {file}: {ex.Message}");
            }
        }

        return new FileProcessingResult(processed, metricsInserted);
    }
}

public record FileProcessingResult(int FilesProcessed, int MetricsInserted);
