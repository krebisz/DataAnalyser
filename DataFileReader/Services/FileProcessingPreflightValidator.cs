using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataFileReader.Services;

internal static class FileProcessingPreflightValidator
{
    public static FileProcessingPreflightResult Validate(FileInfo file, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return FileProcessingPreflightResult.Invalid("File is empty.");

        if (file.Extension.Equals(".json", StringComparison.OrdinalIgnoreCase))
            return ValidateJson(content);

        if (file.Extension.Equals(".csv", StringComparison.OrdinalIgnoreCase))
            return ValidateCsv(content);

        return FileProcessingPreflightResult.Valid;
    }

    private static FileProcessingPreflightResult ValidateJson(string content)
    {
        try
        {
            using var reader = new JsonTextReader(new StringReader(content))
            {
                DateParseHandling = DateParseHandling.None
            };
            JToken.Load(reader);
            return FileProcessingPreflightResult.Valid;
        }
        catch (JsonException ex)
        {
            return FileProcessingPreflightResult.Invalid($"Malformed JSON: {ex.Message}");
        }
    }

    private static FileProcessingPreflightResult ValidateCsv(string content)
    {
        var lines = content.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();

        if (lines.Length < 2)
            return FileProcessingPreflightResult.Invalid("CSV requires at least a header row and one data row.");

        for (var index = 0; index < Math.Min(lines.Length, 10); index++)
            if (!HasBalancedQuotes(lines[index]))
                return FileProcessingPreflightResult.Invalid($"CSV has unbalanced quotes on line {index + 1}.");

        var firstLineColumns = SplitCsvLine(lines[0]);
        var hasVendorIdentifier = firstLineColumns.Count == 1 &&
                                  firstLineColumns[0].Contains("com.samsung", StringComparison.OrdinalIgnoreCase);
        var headerLineIndex = hasVendorIdentifier ? 1 : 0;
        var dataLineIndex = headerLineIndex + 1;

        if (lines.Length <= dataLineIndex)
            return FileProcessingPreflightResult.Invalid("CSV has no data rows.");

        var headers = SplitCsvLine(lines[headerLineIndex]);
        if (headers.Count < 2 || headers.Count(header => !string.IsNullOrWhiteSpace(header)) < 2)
            return FileProcessingPreflightResult.Invalid("CSV header row has too few columns.");

        var firstDataRow = SplitCsvLine(lines[dataLineIndex]);
        if (firstDataRow.All(string.IsNullOrWhiteSpace))
            return FileProcessingPreflightResult.Invalid("CSV first data row is empty.");

        return FileProcessingPreflightResult.Valid;
    }

    private static bool HasBalancedQuotes(string line)
    {
        var quoteCount = line.Count(character => character == '"');
        return quoteCount % 2 == 0;
    }

    private static List<string> SplitCsvLine(string line)
    {
        var values = new List<string>();
        var currentValue = new System.Text.StringBuilder();
        var inQuotes = false;

        foreach (var character in line)
            if (character == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (character == ',' && !inQuotes)
            {
                values.Add(currentValue.ToString().Trim());
                currentValue.Clear();
            }
            else
            {
                currentValue.Append(character);
            }

        values.Add(currentValue.ToString().Trim());
        return values;
    }
}

internal sealed record FileProcessingPreflightResult(bool IsValid, string Reason)
{
    public static FileProcessingPreflightResult Valid { get; } = new(true, string.Empty);

    public static FileProcessingPreflightResult Invalid(string reason) => new(false, reason);
}
