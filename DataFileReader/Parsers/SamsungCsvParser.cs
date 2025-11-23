using DataFileReader.Helper;
using DataFileReader.Parsers;

public class SamsungCsvParser : IHealthFileParser
{
    public bool CanParse(FileInfo file)
        => file.Extension.Equals(".csv", StringComparison.OrdinalIgnoreCase)
           && SamsungHealthParser.IsSamsungHealthFile(file.FullName);

    public List<HealthMetric> Parse(string path, string content)
        => SamsungHealthCsvParser.Parse(path, content);
}
