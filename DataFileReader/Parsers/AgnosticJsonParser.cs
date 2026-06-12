using System.Data;
using System.Configuration;
using DataFileReader.Helper;
using DataFileReader.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataFileReader.Parsers;

public sealed class AgnosticJsonParser : IHealthFileParser
{
    public static MetaDataList metaDataList = new();
    public static HierarchyObjectList hierarchyObjectList = new();
    public static DataTable flattenedData = new();
    private readonly bool _processHierarchy;

    public AgnosticJsonParser()
        : this(processHierarchy: true)
    {
    }

    public AgnosticJsonParser(bool processHierarchy)
    {
        _processHierarchy = processHierarchy;
    }

    public bool CanParse(FileInfo file)
    {
        return file.Extension.Equals(".json", StringComparison.OrdinalIgnoreCase);
    }

    public List<HealthMetric> Parse(string path, string content)
    {
        var token = ParseWithoutDateCoercion(content);

        if (_processHierarchy)
            ProcessJson(Path.GetFileName(path), token);

        return AgnosticJsonMetricExtractor.Extract(path, token);
    }

    public void ProcessHierarchy(string path, string content)
    {
        var token = ParseWithoutDateCoercion(content);
        ProcessJson(Path.GetFileName(path), token);
    }

    private static JToken ParseWithoutDateCoercion(string content)
    {
        using var reader = new JsonTextReader(new StringReader(content))
        {
            DateParseHandling = DateParseHandling.None
        };

        return JToken.Load(reader);
    }

    private static void ProcessJson(string fileName, JToken jsonData)
    {
        try
        {
            hierarchyObjectList = new HierarchyObjectList();
            JsoonHelper.CreateHierarchyObjectList(ref hierarchyObjectList, jsonData);

            metaDataList = new MetaDataList(hierarchyObjectList);
            flattenedData = metaDataList.FlattenData(hierarchyObjectList);

            var verboseLogging = ConfigurationManager.AppSettings["VerboseLogging"] == "true";
            if (verboseLogging)
            {
                PrintPathMapList(hierarchyObjectList);
                PrintHierarchyObjectList(hierarchyObjectList);
                PrintMetaDataList(metaDataList);
                PrintFlattenedDataList(flattenedData);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Error processing JSON file {fileName}: {ex.Message}");
        }
    }

    public static void PrintPathMapList(HierarchyObjectList hierarchyObjectList)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine();
        Console.WriteLine("PATH MAP:");

        foreach (var hierarchyObject in hierarchyObjectList.HierarchyObjects)
            if (!string.IsNullOrEmpty(hierarchyObject.Path))
                ConsoleHelper.PrintPathMap(hierarchyObject);
    }

    private static void PrintHierarchyObjectList(HierarchyObjectList hierarchyObjectList)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine();
        Console.WriteLine("HIERARCHY:");

        foreach (var hierarchyObject in hierarchyObjectList.HierarchyObjects)
            ConsoleHelper.PrintHierarchyObject(
                hierarchyObject.Name,
                hierarchyObject.ID.ToString(),
                hierarchyObject.Level?.ToString() ?? string.Empty,
                hierarchyObject.Value,
                hierarchyObject.ParentID?.ToString() ?? string.Empty,
                hierarchyObject.MetaDataID?.ToString() ?? string.Empty,
                hierarchyObject.ReferenceValue,
                ConsoleHelper.ConsoleOutputColour(hierarchyObject.ClassID));
    }

    public static void PrintMetaDataList(MetaDataList metaDataList)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine();
        Console.WriteLine("METADATA:");

        foreach (var metaData in metaDataList.MetaDataObjects)
            ConsoleHelper.PrintMetaData(metaData);
    }

    public static void PrintFlattenedDataList(DataTable flattenedData)
    {
        ConsoleHelper.PrintFlattenedData(flattenedData);
    }
}
