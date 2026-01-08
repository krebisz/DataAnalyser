using DataFileReader.Helper;

namespace DataFileReader.Parsers;

public interface IHealthFileParser
{
    bool CanParse(FileInfo file);
    List<HealthMetric> Parse(string path, string content);
}