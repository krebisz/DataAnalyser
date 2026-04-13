using System.IO;
using System.Text.Json;

namespace DataVisualiser.UI.MainHost.Export;

public sealed class ReachabilityExportWriter
{
    public sealed record Result(string FilePath);

    public Result Write(object payload, string targetDirectory, DateTime utcNow)
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

        return new Result(filePath);
    }
}
