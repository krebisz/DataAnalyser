using System.Configuration;
using System.Data;
using DataFileReader.Class;
using DataFileReader.Helper;
using DataFileReader.Parsers;
using Newtonsoft.Json.Linq;

public class LegacyJsonParser : IHealthFileParser
{
    public static List<string>        fileList            = new();
    public static MetaDataList        metaDataList        = new();
    public static HierarchyObjectList hierarchyObjectList = new();
    public static DataTable           flattenedData       = new();

    public bool CanParse(FileInfo file)
    {
        return file.Extension.Equals(".json", StringComparison.OrdinalIgnoreCase) && !SamsungHealthParser.IsSamsungHealthFile(file.FullName);
    }

    public List<HealthMetric> Parse(string path, string content)
    {
        // Call your existing legacy code
        Process_JSON_Legacy(Path.GetFileName(path), content);

        // Legacy pipeline already flattens its own data
        return new List<HealthMetric>();
    }

    /// <summary>
    ///     Legacy JSON processing (original implementation)
    /// </summary>
    public static void Process_JSON_Legacy(string fileName, string fileData)
    {
        try
        {
            var jsonData = JToken.Parse(fileData);

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
            Console.WriteLine($"  ✗ Error in legacy processing: {ex.Message}");
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

    private static void PrintHierarchyObjectList(HierarchyObjectList HierarchyObjectList)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine();
        Console.WriteLine("HIERARCHY:");

        //HierarchyObjectList.HierarchyObjects = HierarchyObjectList.HierarchyObjects.OrderBy(h => h.Level).OrderBy(h => h.ParentID).OrderBy(h => h.MetaDataID).ToList();
        //HierarchyObjectList.HierarchyObjects = HierarchyObjectList.HierarchyObjects.OrderBy(h => (Convert.ToDecimal(h.ReferenceValue))).OrderBy(h => h.MetaDataID).ToList();

        foreach (var hierarchyObject in HierarchyObjectList.HierarchyObjects)
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
