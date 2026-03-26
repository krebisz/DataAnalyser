namespace DataFileReader.Services;

public interface IProcessedFileRegistry
{
    HashSet<string> GetProcessedFiles();
    void MarkFileAsProcessed(string filePath);
}
