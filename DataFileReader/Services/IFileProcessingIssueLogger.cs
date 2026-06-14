namespace DataFileReader.Services;

public interface IFileProcessingIssueLogger
{
    string LogFilePath { get; }

    bool HasEntries { get; }

    void LogUnsupportedFile(string filePath);

    void LogNoMetrics(string filePath);

    void LogUnprocessableFile(string filePath, string reason);

    void LogProcessingError(string filePath, Exception exception);
}
