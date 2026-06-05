using System.IO;
using System.Text.Json;

namespace DataVisualiser.UI.Export;

public sealed class EvidenceExportWriter
{
    public sealed record Result(string FilePath);

    public Result Write(object payload, string targetDirectory, DateTime utcNow)
    {
        if (payload == null)
            throw new ArgumentNullException(nameof(payload));
        if (string.IsNullOrWhiteSpace(targetDirectory))
            throw new ArgumentException("Target directory is required.", nameof(targetDirectory));

        Directory.CreateDirectory(targetDirectory);
        var filePath = Path.Combine(targetDirectory, $"reachability-{utcNow:yyyyMMdd-HHmmss}.json");
        File.WriteAllText(filePath, JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }));
        if (!File.Exists(filePath))
            throw new IOException("Export completed without creating the output file.");

        return new Result(filePath);
    }
}
