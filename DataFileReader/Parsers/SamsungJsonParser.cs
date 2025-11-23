using DataFileReader.Helper;
using DataFileReader.Parsers;

public class SamsungJsonParser : IHealthFileParser
{
    public bool CanParse(FileInfo file)
        => file.Extension.Equals(".json", StringComparison.OrdinalIgnoreCase)
           && SamsungHealthParser.IsSamsungHealthFile(file.FullName);

    public List<HealthMetric> Parse(string path, string content)
        => SamsungHealthParser.Parse(path, content);
}
