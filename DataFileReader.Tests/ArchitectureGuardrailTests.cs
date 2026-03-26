using System.Text;

namespace DataFileReader.Tests;

public sealed class ArchitectureGuardrailTests
{
    [Fact]
    public void ActiveCallers_ShouldNotUseSqlHelperDirectly()
    {
        var activeCallers = new[]
        {
            Path.Combine("DataFileReader", "App", "HealthDataApp.cs"),
            Path.Combine("DataFileReader", "Program.cs"),
            Path.Combine("DataFileReader", "Services", "FileProcessingService.cs")
        };

        var offenders = new List<string>();
        foreach (var relativePath in activeCallers)
        {
            var fullPath = Path.Combine(GetRepositoryRoot(), relativePath);
            var source = File.ReadAllText(fullPath);
            if (source.Contains("SQLHelper.", StringComparison.Ordinal))
                offenders.Add(relativePath);
        }

        AssertNoMatches(offenders, "Active DataFileReader callers must not reintroduce direct SQLHelper usage.");
    }

    private static string GetRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current != null)
        {
            if (File.Exists(Path.Combine(current.FullName, "DataAnalyser.sln")))
                return current.FullName;

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root.");
    }

    private static void AssertNoMatches(IReadOnlyList<string> offenders, string header)
    {
        if (offenders.Count == 0)
            return;

        var builder = new StringBuilder();
        builder.AppendLine(header);
        foreach (var offender in offenders)
            builder.AppendLine(offender);

        Assert.True(offenders.Count == 0, builder.ToString());
    }
}
