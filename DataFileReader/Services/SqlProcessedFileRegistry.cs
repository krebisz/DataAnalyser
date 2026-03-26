using DataFileReader.Helper;

namespace DataFileReader.Services;

public sealed class SqlProcessedFileRegistry : IProcessedFileRegistry
{
    public HashSet<string> GetProcessedFiles()
    {
        return SQLHelper.GetProcessedFiles();
    }

    public void MarkFileAsProcessed(string filePath)
    {
        SQLHelper.MarkFileAsProcessed(filePath);
    }
}
