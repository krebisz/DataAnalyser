using System.Globalization;
using DataFileReader.Helper;
using Newtonsoft.Json.Linq;

namespace DataFileReader.Parsers;

internal static class AgnosticJsonMetricExtractor
{
    private static readonly string[] TimestampNameHints =
    [
        "timestamp",
        "date",
        "time",
        "x_value_iso"
    ];

    public static List<HealthMetric> Extract(string path, JToken token)
    {
        var metrics = new List<HealthMetric>();
        Visit(token, path, "Root", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase), metrics);
        return metrics;
    }

    private static void Visit(
        JToken token,
        string sourcePath,
        string jsonPath,
        IReadOnlyDictionary<string, string> inheritedContext,
        List<HealthMetric> metrics)
    {
        switch (token)
        {
            case JObject obj:
                var context = MergeScalarContext(inheritedContext, obj);
                metrics.AddRange(ProjectObject(sourcePath, jsonPath, obj, context));

                foreach (var property in obj.Properties())
                    if (property.Value is JObject or JArray)
                        Visit(property.Value, sourcePath, $"{jsonPath}.{property.Name}", context, metrics);
                break;

            case JArray array:
                for (var index = 0; index < array.Count; index++)
                    Visit(array[index], sourcePath, $"{jsonPath}[{index}]", inheritedContext, metrics);
                break;
        }
    }

    private static Dictionary<string, string> MergeScalarContext(
        IReadOnlyDictionary<string, string> inheritedContext,
        JObject obj)
    {
        var context = new Dictionary<string, string>(inheritedContext, StringComparer.OrdinalIgnoreCase);

        foreach (var property in obj.Properties())
            if (property.Value is JValue value && value.Type != JTokenType.Null)
                context[property.Name] = Convert.ToString(value.Value, CultureInfo.InvariantCulture) ?? string.Empty;

        return context;
    }

    private static IEnumerable<HealthMetric> ProjectObject(
        string sourcePath,
        string jsonPath,
        JObject obj,
        IReadOnlyDictionary<string, string> context)
    {
        if (!TryResolveTimestamp(obj, context, out var timestamp, out var rawTimestamp))
            return [];

        var numericFields = obj.Properties()
            .Where(property => IsMetricCandidate(property.Name, property.Value))
            .Select(property => (property.Name, Value: property.Value.Value<decimal>()))
            .ToArray();

        if (numericFields.Length == 0 && HasExplicitNullMetricField(obj))
            return [];

        if (numericFields.Length == 0)
            return [CreateMetadataOnlyMetric(sourcePath, jsonPath, obj, context, timestamp, rawTimestamp)];

        var metrics = new List<HealthMetric>();
        foreach (var field in numericFields)
        {
            var metricType = ResolveMetricType(sourcePath, context, field.Name, jsonPath);
            var metricSubtype = ResolveMetricSubtype(context, field.Name);

            var metric = new HealthMetric
            {
                Provider = "AgnosticJson",
                MetricType = metricType,
                MetricSubtype = metricSubtype,
                SourceFile = sourcePath,
                NormalizedTimestamp = timestamp,
                RawTimestamp = rawTimestamp,
                Value = field.Value,
                Unit = DetermineUnit(metricType, metricSubtype, field.Name)
            };

            foreach (var item in context)
                metric.AdditionalFields[item.Key] = item.Value;
            metric.AdditionalFields["JsonPath"] = jsonPath;
            metric.AdditionalFields["ValueField"] = field.Name;

            metrics.Add(metric);
        }

        return metrics;
    }

    private static HealthMetric CreateMetadataOnlyMetric(
        string sourcePath,
        string jsonPath,
        JObject obj,
        IReadOnlyDictionary<string, string> context,
        DateTime timestamp,
        string rawTimestamp)
    {
        var metricType = ResolveMetricType(sourcePath, context, string.Empty, jsonPath);
        var metricSubtype = ResolveMetricSubtype(context, metricType);
        var metric = new HealthMetric
        {
            Provider = "AgnosticJson",
            MetricType = metricType,
            MetricSubtype = metricSubtype,
            SourceFile = sourcePath,
            NormalizedTimestamp = timestamp,
            RawTimestamp = rawTimestamp
        };

        foreach (var property in obj.Properties())
            if (property.Value is JValue value)
                metric.AdditionalFields[property.Name] = Convert.ToString(value.Value, CultureInfo.InvariantCulture) ?? string.Empty;

        foreach (var item in context)
            metric.AdditionalFields.TryAdd(item.Key, item.Value);

        metric.AdditionalFields["JsonPath"] = jsonPath;
        metric.AdditionalFields["RecordKind"] = "TimestampedMetadata";
        return metric;
    }

    private static bool TryResolveTimestamp(
        JObject obj,
        IReadOnlyDictionary<string, string> context,
        out DateTime timestamp,
        out string rawTimestamp)
    {
        foreach (var property in obj.Properties())
            if (IsTimestampName(property.Name) && TryParseTimestamp(property.Value, out timestamp, out rawTimestamp))
                return true;

        foreach (var item in context)
            if (IsTimestampName(item.Key) && TryParseTimestamp(item.Value, out timestamp, out rawTimestamp))
                return true;

        timestamp = default;
        rawTimestamp = string.Empty;
        return false;
    }

    private static bool TryParseTimestamp(JToken token, out DateTime timestamp, out string rawTimestamp)
    {
        rawTimestamp = token.ToString();

        if (token.Type == JTokenType.Date)
        {
            timestamp = token.Value<DateTime>();
            return true;
        }

        return DateTime.TryParse(rawTimestamp, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out timestamp);
    }

    private static bool TryParseTimestamp(string value, out DateTime timestamp, out string rawTimestamp)
    {
        rawTimestamp = value;
        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out timestamp);
    }

    private static bool IsMetricCandidate(string name, JToken value)
    {
        return value.Type is JTokenType.Integer or JTokenType.Float &&
               !IsTimestampName(name) &&
               !IsIdentifierName(name);
    }

    private static bool HasExplicitNullMetricField(JObject obj)
    {
        return obj.Properties().Any(property =>
            property.Value.Type == JTokenType.Null &&
            !IsTimestampName(property.Name) &&
            !IsIdentifierName(property.Name));
    }

    private static bool IsTimestampName(string name)
    {
        return TimestampNameHints.Any(hint => name.Contains(hint, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsIdentifierName(string name)
    {
        var normalized = name.Replace('-', '_');
        return string.Equals(normalized, "id", StringComparison.OrdinalIgnoreCase) ||
               normalized.EndsWith("_id", StringComparison.OrdinalIgnoreCase) ||
               normalized.EndsWith("id", StringComparison.OrdinalIgnoreCase) && normalized.Length <= 4 ||
               normalized.Contains("uuid", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(normalized, "index", StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveMetricType(string sourcePath, IReadOnlyDictionary<string, string> context, string fieldName, string jsonPath)
    {
        if (TryGetNonEmpty(context, "slug", out var slug))
            return ToPascalCase(slug);

        if (TryGetNonEmpty(context, "metricType", out var metricType))
            return CombineMetricType(metricType, fieldName);

        if (TryGetNonEmpty(context, "metric_type", out var snakeMetricType))
            return CombineMetricType(snakeMetricType, fieldName);

        if (TryGetNonEmpty(context, "type", out var type))
            return CombineMetricType(type, fieldName);

        var pathName = jsonPath.Split(['.', '[', ']'], StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
        var fallbackBaseName = IsUsefulPathName(pathName)
            ? pathName!
            : Path.GetFileNameWithoutExtension(sourcePath);

        return CombineMetricType(fallbackBaseName, fieldName);
    }

    private static string ResolveMetricSubtype(IReadOnlyDictionary<string, string> context, string fieldName)
    {
        if (TryGetNonEmpty(context, "title", out var title))
            return title;

        if (TryGetNonEmpty(context, "name", out var name))
            return name;

        return fieldName;
    }

    private static bool TryGetNonEmpty(IReadOnlyDictionary<string, string> context, string key, out string value)
    {
        return context.TryGetValue(key, out value!) && !string.IsNullOrWhiteSpace(value);
    }

    private static string ToPascalCase(string input)
    {
        var parts = input.Split(['_', '-', '.', ' '], StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 0
            ? "Unknown"
            : string.Join("", parts.Select(part => char.ToUpperInvariant(part[0]) + part[1..].ToLowerInvariant()));
    }

    private static string CombineMetricType(string baseName, string fieldName)
    {
        var baseType = ToPascalCase(baseName);
        if (string.IsNullOrWhiteSpace(fieldName))
            return baseType;

        return $"{baseType}_{fieldName}";
    }

    private static bool IsUsefulPathName(string? pathName)
    {
        return !string.IsNullOrWhiteSpace(pathName) &&
               pathName.Any(char.IsLetter) &&
               !string.Equals(pathName, "Root", StringComparison.OrdinalIgnoreCase);
    }

    private static string DetermineUnit(string metricType, string metricSubtype, string fieldName)
    {
        var combined = $"{metricType} {metricSubtype} {fieldName}".ToLowerInvariant();
        if (combined.Contains("kcal") || combined.Contains("calorie"))
            return "kcal";
        if (combined.Contains("step"))
            return "steps";
        if (combined.Contains("weight"))
            return "kg";
        if (combined.Contains("distance"))
            return "m";

        return string.Empty;
    }
}
