using DataFileReader.Helper;

namespace DataFileReader.Parsers;

public class SamsungCsvParser : IHealthFileParser
{
    public bool CanParse(FileInfo file)
    {
        return file.Extension.Equals(".csv", StringComparison.OrdinalIgnoreCase);
    }

    public List<HealthMetric> Parse(string path, string content)
    {
        return SamsungHealthCsvParser.Parse(path, content);
    }
}
