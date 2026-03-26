using DataFileReader.Helper;
using DataFileReader.Parsers;
using DataFileReader.Services;

namespace DataFileReader.Tests;

public sealed class FileProcessingServiceTests
{
    [Fact]
    public void ProcessFiles_InsertsParsedMetrics_ThroughWriter()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), "DataFileReader.Tests", $"{Guid.NewGuid():N}.json");
        Directory.CreateDirectory(Path.GetDirectoryName(tempFile)!);
        File.WriteAllText(tempFile, "{\"ok\":true}");

        try
        {
            var writer = new FakeHealthMetricWriter();
            var processedRegistry = new FakeProcessedFileRegistry();
            var parser = new FakeParser(
                canParse: file => file.Extension.Equals(".json", StringComparison.OrdinalIgnoreCase),
                parse: (path, _) =>
                [
                    new HealthMetric
                    {
                        MetricType = "Weight",
                        SourceFile = path,
                        Value = 1m
                    }
                ]);
            var service = new FileProcessingService([parser], writer, processedRegistry);

            var result = service.ProcessFiles([tempFile]);

            Assert.Equal(1, result.FilesProcessed);
            Assert.Equal(1, result.MetricsInserted);
            Assert.Single(writer.InsertedBatches);
            Assert.Empty(processedRegistry.MarkedFiles);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ProcessFiles_MarksEmptyFilesAsProcessed_WithoutWritingMetrics()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), "DataFileReader.Tests", $"{Guid.NewGuid():N}.json");
        Directory.CreateDirectory(Path.GetDirectoryName(tempFile)!);
        File.WriteAllText(tempFile, "{\"ok\":true}");

        try
        {
            var writer = new FakeHealthMetricWriter();
            var processedRegistry = new FakeProcessedFileRegistry();
            var parser = new FakeParser(
                canParse: _ => true,
                parse: (_, _) => []);
            var service = new FileProcessingService([parser], writer, processedRegistry);

            var result = service.ProcessFiles([tempFile]);

            Assert.Equal(1, result.FilesProcessed);
            Assert.Equal(0, result.MetricsInserted);
            Assert.Empty(writer.InsertedBatches);
            Assert.Equal([tempFile], processedRegistry.MarkedFiles);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    private sealed class FakeParser : IHealthFileParser
    {
        private readonly Func<FileInfo, bool> _canParse;
        private readonly Func<string, string, List<HealthMetric>> _parse;

        public FakeParser(Func<FileInfo, bool> canParse, Func<string, string, List<HealthMetric>> parse)
        {
            _canParse = canParse;
            _parse = parse;
        }

        public bool CanParse(FileInfo file)
        {
            return _canParse(file);
        }

        public List<HealthMetric> Parse(string path, string content)
        {
            return _parse(path, content);
        }
    }

    private sealed class FakeHealthMetricWriter : IHealthMetricWriter
    {
        public List<IReadOnlyList<HealthMetric>> InsertedBatches { get; } = [];

        public void InsertHealthMetrics(IReadOnlyList<HealthMetric> metrics)
        {
            InsertedBatches.Add(metrics.ToList());
        }
    }

    private sealed class FakeProcessedFileRegistry : IProcessedFileRegistry
    {
        public List<string> MarkedFiles { get; } = [];

        public HashSet<string> GetProcessedFiles()
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public void MarkFileAsProcessed(string filePath)
        {
            MarkedFiles.Add(filePath);
        }
    }
}
