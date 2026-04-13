using System.IO;
using System.Reflection;
using DataVisualiser.UI.MainHost.Export;

namespace DataVisualiser.Tests.UI.MainHost;

public sealed class ReachabilityExportPathResolverTests
{
    [Fact]
    public void ResolveDocumentsDirectory_ShouldPreferRepositoryRootDocuments_WhenSolutionIsFound()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "DataVisualiser.Tests", Guid.NewGuid().ToString("N"));
        var nested = Path.Combine(tempRoot, "src", "bin");

        Directory.CreateDirectory(nested);
        File.WriteAllText(Path.Combine(tempRoot, "DataAnalyser.sln"), string.Empty);

        try
        {
            var resolved = InvokeResolveDocumentsDirectory(nested);
            Assert.Equal(Path.Combine(tempRoot, "documents"), resolved);
        }
        finally
        {
            Directory.Delete(tempRoot, true);
        }
    }

    [Fact]
    public void ResolveDocumentsDirectory_ShouldFallbackToStartingDirectoryDocuments_WhenSolutionIsMissing()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "DataVisualiser.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);

        try
        {
            var resolved = InvokeResolveDocumentsDirectory(tempRoot);
            Assert.Equal(Path.Combine(tempRoot, "documents"), resolved);
        }
        finally
        {
            Directory.Delete(tempRoot, true);
        }
    }

    private static string InvokeResolveDocumentsDirectory(string startingDirectory)
    {
        var method = typeof(ReachabilityExportPathResolver)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .SingleOrDefault(candidate =>
                string.Equals(candidate.Name, "ResolveDocumentsDirectory", StringComparison.Ordinal) &&
                candidate.GetParameters() is [{ ParameterType: var parameterType }] &&
                parameterType == typeof(string));

        Assert.NotNull(method);
        return Assert.IsType<string>(method!.Invoke(null, [startingDirectory]));
    }
}
