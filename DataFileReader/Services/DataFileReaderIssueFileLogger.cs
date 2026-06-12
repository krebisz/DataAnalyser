using System.Text;

namespace DataFileReader.Services;

public sealed class DataFileReaderIssueFileLogger : IFileProcessingIssueLogger
{
    private readonly object _sync = new();

    public DataFileReaderIssueFileLogger()
    {
        var directory = ResolveLogDirectory(AppContext.BaseDirectory);
        Directory.CreateDirectory(directory);
        LogFilePath = Path.Combine(directory, $"DataFileReader-{DateTime.UtcNow:yyyyMMdd-HHmmssfff}-utc.log");
    }

    public string LogFilePath { get; }

    public bool HasEntries { get; private set; }

    public void LogUnsupportedFile(string filePath)
    {
        Write("UnsupportedFile", filePath, null);
    }

    public void LogNoMetrics(string filePath)
    {
        Write("NoMetrics", filePath, null);
    }

    public void LogProcessingError(string filePath, Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        Write("ProcessingError", filePath, exception);
    }

    private void Write(string category, string filePath, Exception? exception)
    {
        try
        {
            lock (_sync)
            {
                File.AppendAllText(LogFilePath, BuildEntry(category, filePath, exception));
                HasEntries = true;
            }
        }
        catch
        {
            // Logging must never break ingestion.
        }
    }

    private static string BuildEntry(string category, string filePath, Exception? exception)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Category: {category}");
        sb.AppendLine($"UTC: {DateTime.UtcNow:O}");
        sb.AppendLine($"Machine: {Environment.MachineName}");
        sb.AppendLine($"User: {Environment.UserName}");
        sb.AppendLine($"Process: {Environment.ProcessId}");
        sb.AppendLine($"File: {filePath}");

        try
        {
            if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
            {
                var info = new FileInfo(filePath);
                sb.AppendLine($"FileSizeBytes: {info.Length}");
                sb.AppendLine($"LastWriteTimeUtc: {info.LastWriteTimeUtc:O}");
            }
        }
        catch
        {
            // Keep the entry minimal if file metadata cannot be read.
        }

        if (exception != null)
        {
            sb.AppendLine($"ExceptionType: {exception.GetType().FullName}");
            sb.AppendLine($"Message: {exception.Message}");
            sb.AppendLine(exception.ToString());
        }

        sb.AppendLine();
        return sb.ToString();
    }

    internal static string ResolveLogDirectory(string startingDirectory)
    {
        if (string.IsNullOrWhiteSpace(startingDirectory))
            throw new ArgumentException("Starting directory is required.", nameof(startingDirectory));

        var current = new DirectoryInfo(startingDirectory);
        while (current != null)
        {
            if (File.Exists(Path.Combine(current.FullName, "DataAnalyser.sln")))
                return Path.Combine(current.FullName, "documents", "logs");

            current = current.Parent;
        }

        return Path.Combine(startingDirectory, "documents", "logs");
    }
}
