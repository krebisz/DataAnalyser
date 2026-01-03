using DataFileReader.Helper;
using DataFileReader.Parsers;

public class SamsungJsonParser : IHealthFileParser
{
    public bool CanParse(FileInfo file)
    {
        return file.Extension.Equals(".json", StringComparison.OrdinalIgnoreCase) && SamsungHealthParser.IsSamsungHealthFile(file.FullName);
    }

    public List<HealthMetric> Parse(string path, string content)
    {
        return SamsungHealthParser.Parse(path, content);
    }
}