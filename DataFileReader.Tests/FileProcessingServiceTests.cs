using DataFileReader.Helper;
using DataFileReader.Parsers;
using DataFileReader.Services;
using System.Reflection;

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

    [Fact]
    public void ProcessFiles_LogsNoMetricFilesToIssueLogger()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), "DataFileReader.Tests", $"{Guid.NewGuid():N}.json");
        Directory.CreateDirectory(Path.GetDirectoryName(tempFile)!);
        File.WriteAllText(tempFile, "{\"ok\":true}");

        try
        {
            var logger = new FakeIssueLogger("issue-log-path.log");
            var service = new FileProcessingService(
                [new FakeParser(canParse: _ => true, parse: (_, _) => [])],
                new FakeHealthMetricWriter(),
                new FakeProcessedFileRegistry(),
                logger);

            var result = service.ProcessFiles([tempFile]);

            Assert.True(result.HasIssues);
            Assert.Equal("issue-log-path.log", result.IssueLogPath);
            Assert.Equal([tempFile], logger.NoMetricFiles);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ProcessFiles_LogsUnsupportedFilesToIssueLogger()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), "DataFileReader.Tests", $"{Guid.NewGuid():N}.txt");
        Directory.CreateDirectory(Path.GetDirectoryName(tempFile)!);
        File.WriteAllText(tempFile, "unsupported");

        try
        {
            var logger = new FakeIssueLogger("issue-log-path.log");
            var service = new FileProcessingService(
                [new FakeParser(canParse: _ => false, parse: (_, _) => [])],
                new FakeHealthMetricWriter(),
                new FakeProcessedFileRegistry(),
                logger);

            var result = service.ProcessFiles([tempFile]);

            Assert.True(result.HasIssues);
            Assert.Equal(0, result.FilesProcessed);
            Assert.Equal([tempFile], logger.UnsupportedFiles);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ProcessFiles_LogsParserErrorsToIssueLogger()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), "DataFileReader.Tests", $"{Guid.NewGuid():N}.json");
        Directory.CreateDirectory(Path.GetDirectoryName(tempFile)!);
        File.WriteAllText(tempFile, "{\"broken\":true}");

        try
        {
            var logger = new FakeIssueLogger("issue-log-path.log");
            var service = new FileProcessingService(
                [new FakeParser(canParse: _ => true, parse: (_, _) => throw new FormatException("bad shape"))],
                new FakeHealthMetricWriter(),
                new FakeProcessedFileRegistry(),
                logger);

            var result = service.ProcessFiles([tempFile]);

            Assert.True(result.HasIssues);
            var error = Assert.Single(logger.Errors);
            Assert.Equal(tempFile, error.FilePath);
            Assert.IsType<FormatException>(error.Exception);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ProcessFiles_LogsMalformedJsonAsUnprocessable_WithoutCallingParser()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), "DataFileReader.Tests", $"{Guid.NewGuid():N}.json");
        Directory.CreateDirectory(Path.GetDirectoryName(tempFile)!);
        File.WriteAllText(tempFile, "{\"broken\":true");

        try
        {
            var logger = new FakeIssueLogger("issue-log-path.log");
            var service = new FileProcessingService(
                [new FakeParser(canParse: _ => true, parse: (_, _) => throw new InvalidOperationException("Parser should not be called."))],
                new FakeHealthMetricWriter(),
                new FakeProcessedFileRegistry(),
                logger);

            var result = service.ProcessFiles([tempFile]);

            Assert.True(result.HasIssues);
            Assert.Equal(0, result.FilesProcessed);
            var issue = Assert.Single(logger.UnprocessableFiles);
            Assert.Equal(tempFile, issue.FilePath);
            Assert.Contains("Malformed JSON", issue.Reason);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ProcessFiles_LogsMalformedCsvAsUnprocessable_WithoutCallingParser()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), "DataFileReader.Tests", $"{Guid.NewGuid():N}.csv");
        Directory.CreateDirectory(Path.GetDirectoryName(tempFile)!);
        File.WriteAllText(tempFile, "timestamp,value\n\"2026-06-05,84");

        try
        {
            var logger = new FakeIssueLogger("issue-log-path.log");
            var service = new FileProcessingService(
                [new FakeParser(canParse: _ => true, parse: (_, _) => throw new InvalidOperationException("Parser should not be called."))],
                new FakeHealthMetricWriter(),
                new FakeProcessedFileRegistry(),
                logger);

            var result = service.ProcessFiles([tempFile]);

            Assert.True(result.HasIssues);
            Assert.Equal(0, result.FilesProcessed);
            var issue = Assert.Single(logger.UnprocessableFiles);
            Assert.Equal(tempFile, issue.FilePath);
            Assert.Contains("unbalanced quotes", issue.Reason);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void IssueFileLogger_ShouldResolveRepositoryRelativeDocumentsLogsDirectory()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "DataFileReader.Tests", Guid.NewGuid().ToString("N"));
        var nested = Path.Combine(tempRoot, "DataFileReader", "bin", "Debug");
        Directory.CreateDirectory(nested);
        File.WriteAllText(Path.Combine(tempRoot, "DataAnalyser.sln"), string.Empty);

        try
        {
            var resolved = InvokeResolveLogDirectory(nested);
            Assert.Equal(Path.Combine(tempRoot, "documents", "logs"), resolved);
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
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

    private sealed class FakeIssueLogger(string logFilePath) : IFileProcessingIssueLogger
    {
        public string LogFilePath { get; } = logFilePath;

        public bool HasEntries => UnsupportedFiles.Count > 0 || NoMetricFiles.Count > 0 || UnprocessableFiles.Count > 0 || Errors.Count > 0;

        public List<string> UnsupportedFiles { get; } = [];

        public List<string> NoMetricFiles { get; } = [];

        public List<(string FilePath, string Reason)> UnprocessableFiles { get; } = [];

        public List<(string FilePath, Exception Exception)> Errors { get; } = [];

        public void LogUnsupportedFile(string filePath)
        {
            UnsupportedFiles.Add(filePath);
        }

        public void LogNoMetrics(string filePath)
        {
            NoMetricFiles.Add(filePath);
        }

        public void LogUnprocessableFile(string filePath, string reason)
        {
            UnprocessableFiles.Add((filePath, reason));
        }

        public void LogProcessingError(string filePath, Exception exception)
        {
            Errors.Add((filePath, exception));
        }
    }

    private static string InvokeResolveLogDirectory(string startingDirectory)
    {
        var method = typeof(DataFileReaderIssueFileLogger)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .SingleOrDefault(candidate =>
                string.Equals(candidate.Name, "ResolveLogDirectory", StringComparison.Ordinal) &&
                candidate.GetParameters() is [{ ParameterType: var parameterType }] &&
                parameterType == typeof(string));

        Assert.NotNull(method);
        return Assert.IsType<string>(method!.Invoke(null, [startingDirectory]));
    }
}
