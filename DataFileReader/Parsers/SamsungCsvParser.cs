using DataFileReader.Helper;
using DataFileReader.Parsers;

public class SamsungCsvParser : IHealthFileParser
{
    public bool CanParse(FileInfo file)
    {
        return file.Extension.Equals(".csv", StringComparison.OrdinalIgnoreCase) && SamsungHealthParser.IsSamsungHealthFile(file.FullName);
    }

    public List<HealthMetric> Parse(string path, string content)
    {
        return SamsungHealthCsvParser.Parse(path, content);
    }
}