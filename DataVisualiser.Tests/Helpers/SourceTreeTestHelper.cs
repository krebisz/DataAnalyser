using System.IO;

namespace DataVisualiser.Tests.Helpers;

internal static class SourceTreeTestHelper
{
    private static readonly string[] IgnoredDirectorySegments = ["\\bin\\", "\\obj\\"];

    public static string ReadRepositoryFile(params string[] relativeSegments)
    {
        var path = GetRepositoryPath(relativeSegments);
        return File.ReadAllText(path);
    }

    public static IReadOnlyList<string> FindForbiddenTokenMatches(IEnumerable<string> relativeDirectories, IEnumerable<string> forbiddenTokens)
    {
        var results = new List<string>();

        foreach (var relativeDirectory in relativeDirectories)
        {
            var absoluteDirectory = GetRepositoryPath(relativeDirectory);
            if (!Directory.Exists(absoluteDirectory))
                continue;

            foreach (var file in Directory.EnumerateFiles(absoluteDirectory, "*.cs", SearchOption.AllDirectories))
            {
                if (IgnoredDirectorySegments.Any(segment => file.Contains(segment, StringComparison.OrdinalIgnoreCase)))
                    continue;

                var content = File.ReadAllText(file);
                foreach (var token in forbiddenTokens)
                {
                    if (content.Contains(token, StringComparison.Ordinal))
                    {
                        results.Add($"{GetRelativeRepositoryPath(file)} :: {token}");
                    }
                }
            }
        }

        return results;
    }

    public static string GetRepositoryPath(params string[] relativeSegments)
    {
        var path = GetRepositoryRoot();
        foreach (var segment in relativeSegments)
            path = Path.Combine(path, segment);

        return path;
    }

    private static string GetRelativeRepositoryPath(string absolutePath)
    {
        return Path.GetRelativePath(GetRepositoryRoot(), absolutePath).Replace('\\', '/');
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

        throw new DirectoryNotFoundException("Could not locate the repository root from the test output directory.");
    }
}
