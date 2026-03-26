namespace DataVisualiser.UI.MainHost;

public sealed record ReachabilityEvidenceExportResult(
    string FilePath,
    bool HadReachabilityRecords,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> Notes);
