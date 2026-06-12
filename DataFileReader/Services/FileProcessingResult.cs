namespace DataFileReader.Services;

public record FileProcessingResult(
    int FilesProcessed,
    int MetricsInserted,
    string? IssueLogPath = null,
    bool HasIssues = false);
