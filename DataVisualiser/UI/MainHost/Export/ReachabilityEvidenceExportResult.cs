namespace DataVisualiser.UI.MainHost.Export;

public sealed record ReachabilityEvidenceExportResult(
    string FilePath,
    bool HadReachabilityRecords,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> Notes);
