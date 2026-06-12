using System.IO;
using System.Reflection;
using DataVisualiser;

namespace DataVisualiser.Tests;

public sealed class AppCrashLogPathTests
{
    [Fact]
    public void ResolveLogDirectory_ShouldUseRepositoryRelativeDocumentsLogsDirectory()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "DataVisualiser.Tests", Guid.NewGuid().ToString("N"));
        var nested = Path.Combine(tempRoot, "DataVisualiser", "bin", "Debug");
        Directory.CreateDirectory(nested);
        File.WriteAllText(Path.Combine(tempRoot, "DataAnalyser.sln"), string.Empty);

        try
        {
            var resolved = InvokeResolveLogDirectory(nested);
            Assert.Equal(Path.Combine(tempRoot, "documents", "logs"), resolved);
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    private static string InvokeResolveLogDirectory(string startingDirectory)
    {
        var method = typeof(App)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .SingleOrDefault(candidate =>
                string.Equals(candidate.Name, "ResolveLogDirectory", StringComparison.Ordinal) &&
                candidate.GetParameters() is [{ ParameterType: var parameterType }] &&
                parameterType == typeof(string));

        Assert.NotNull(method);
        return Assert.IsType<string>(method!.Invoke(null, [startingDirectory]));
    }
}
