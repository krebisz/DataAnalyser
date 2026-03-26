using System.IO;

namespace DataVisualiser.UI.MainHost;

public sealed class ReachabilityExportPathResolver : IReachabilityExportPathResolver
{
    public string ResolveDocumentsDirectory()
    {
        return ResolveDocumentsDirectory(AppContext.BaseDirectory);
    }

    internal static string ResolveDocumentsDirectory(string startingDirectory)
    {
        if (string.IsNullOrWhiteSpace(startingDirectory))
            throw new ArgumentException("Starting directory is required.", nameof(startingDirectory));

        var current = new DirectoryInfo(startingDirectory);
        while (current != null)
        {
            if (File.Exists(Path.Combine(current.FullName, "DataAnalyser.sln")))
                return Path.Combine(current.FullName, "documents");

            current = current.Parent;
        }

        return Path.Combine(startingDirectory, "documents");
    }
}
