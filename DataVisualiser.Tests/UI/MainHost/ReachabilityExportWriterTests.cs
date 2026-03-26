using System.IO;
using DataVisualiser.UI.MainHost;

namespace DataVisualiser.Tests.UI.MainHost;

public sealed class ReachabilityExportWriterTests
{
    [Fact]
    public void Write_ShouldCreateJsonFileInTargetDirectory()
    {
        var writer = new ReachabilityExportWriter();
        var tempDir = Path.Combine(Path.GetTempPath(), "DataVisualiser.Tests", Guid.NewGuid().ToString("N"));

        try
        {
            var result = writer.Write(new { Name = "Test" }, tempDir, new DateTime(2026, 3, 25, 12, 0, 0, DateTimeKind.Utc));

            Assert.True(File.Exists(result.FilePath));
            Assert.EndsWith("reachability-20260325-120000.json", result.FilePath, StringComparison.OrdinalIgnoreCase);
            var contents = File.ReadAllText(result.FilePath);
            Assert.Contains("\"Name\": \"Test\"", contents);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Write_ShouldThrow_WhenTargetDirectoryIsBlank()
    {
        var writer = new ReachabilityExportWriter();

        var ex = Assert.Throws<ArgumentException>(() =>
            writer.Write(new { Name = "Test" }, "   ", new DateTime(2026, 3, 25, 12, 0, 0, DateTimeKind.Utc)));

        Assert.Equal("targetDirectory", ex.ParamName);
    }
}
