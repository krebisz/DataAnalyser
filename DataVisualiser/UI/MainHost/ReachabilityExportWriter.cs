using System.Text.Json;
using System.IO;

namespace DataVisualiser.UI.MainHost;

public sealed class ReachabilityExportWriter
{
    public ReachabilityExportResult Write(object payload, string targetDirectory, DateTime utcNow)
    {
        if (payload == null)
            throw new ArgumentNullException(nameof(payload));
        if (string.IsNullOrWhiteSpace(targetDirectory))
            throw new ArgumentException("Target directory is required.", nameof(targetDirectory));

        Directory.CreateDirectory(targetDirectory);
        var fileName = $"reachability-{utcNow:yyyyMMdd-HHmmss}.json";
        var filePath = Path.Combine(targetDirectory, fileName);
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        File.WriteAllText(filePath, JsonSerializer.Serialize(payload, options));
        if (!File.Exists(filePath))
            throw new IOException("Export completed without creating the output file.");

        return new ReachabilityExportResult(filePath);
    }
}
