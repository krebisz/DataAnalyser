using System.Configuration;
using System.Data;
using System.Text;
using DataFileReader.Class;

namespace DataFileReader.Helper;

public static class ConsoleHelper
{
    private static readonly Lazy<bool> ConsoleOutputEnabled = new(() =>
    {
        var raw = ConfigurationManager.AppSettings["ConsoleHelper:Enabled"];
        if (string.IsNullOrWhiteSpace(raw))
            return true;

        return string.Equals(raw.Trim(), "true", StringComparison.OrdinalIgnoreCase);
    });

    private static bool IsEnabled => ConsoleOutputEnabled.Value;

    public static ConsoleColor ConsoleOutputColour(string variableType)
    {
        var consoleColor = new ConsoleColor();

        switch (variableType)
        {
            case "Container":
            {
                consoleColor = ConsoleColor.Blue;
                break;
            }
            case "Element":
            {
                consoleColor = ConsoleColor.Green;
                break;
            }
            default:
            {
                consoleColor = ConsoleColor.Red;
                break;
            }
        }

        return consoleColor;
    }

    public static void PrintPathMap(HierarchyObject hierarchyObject)
    {
        if (!IsEnabled)
            return;

        Console.ForegroundColor = ConsoleColor.White;

        if (hierarchyObject.ClassID == "Element")
            Console.WriteLine($"Path: {hierarchyObject.Path}, Value: {hierarchyObject.Value}");
    }

    public static void PrintHierarchyObject(string key, string Id, string level, string value, string parent, string metaId, string refVal, ConsoleColor colour)
    {
        if (!IsEnabled)
            return;

        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("   ID: ");

        Console.ForegroundColor = colour;
        Console.Write(Id.PadRight(2));

        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("   PARENT: ");

        Console.ForegroundColor = colour;
        Console.Write(parent.PadRight(2));

        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("   LEVEL: ");

        Console.ForegroundColor = colour;
        Console.Write(level.PadRight(2));

        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(" OBJECT: ");

        Console.ForegroundColor = colour;
        var padLevel = int.Parse(level) * 2;
        var paddedPrefix = string.Empty.PadLeft(padLevel);
        var paddedKey = (paddedPrefix + "|" + key).PadRight(32);

        Console.Write(paddedKey);

        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("   VALUE: ");

        Console.ForegroundColor = colour;
        Console.Write(value.PadRight(50));

        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("   META-ID: ");

        Console.ForegroundColor = colour;
        Console.Write(metaId.PadRight(20));


        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("  REFERENCE: ");

        Console.ForegroundColor = colour;
        Console.Write(refVal.PadRight(30));

        Console.WriteLine();
    }

    public static void PrintMetaData(MetaData metaData)
    {
        if (!IsEnabled)
            return;

        var variableColour = ConsoleOutputColour(metaData.Type);

        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("NAME: ");

        Console.ForegroundColor = variableColour;
        Console.Write($" {metaData.Name.PadRight(16)}");

        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("TYPE: ");

        Console.ForegroundColor = variableColour;
        Console.Write($" {metaData.Type.PadRight(12)}");

        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("ID: ");

        Console.ForegroundColor = variableColour;
        Console.Write($" {metaData.ID.ToString().PadRight(16)}");

        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("FIELDS: ");

        Console.ForegroundColor = variableColour;
        Console.Write($" {metaData.Fields.First().Key.PadRight(50)}");

        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("REFERENCE: ");

        Console.ForegroundColor = variableColour;
        Console.WriteLine($" {metaData.ReferenceValue.PadRight(20)}");
    }

    public static void PrintFlattenedData(DataTable flattenedData)
    {
        if (!IsEnabled)
            return;

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine();
        Console.WriteLine("FLATTENED DATA:");
        Console.ForegroundColor = ConsoleColor.White;

        for (var i = 0; i < flattenedData.Columns.Count; i++)
            Console.Write(flattenedData.Columns[i].ColumnName.PadRight(20) + ", ");

        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Green;
        //foreach (DataRow row in flattenedData.Rows)
        //{
        //    Console.WriteLine((string.Join(", ", row.ItemArray).PadRight(16)));
        //}

        foreach (DataRow row in flattenedData.Rows)
        {
            var printedRow = new StringBuilder();

            foreach (var field in row.ItemArray)
            {
                var fieldValue = (field?.ToString() ?? string.Empty).PadRight(20) + ", ";
                printedRow.Append(fieldValue);
            }

            Console.WriteLine(printedRow);
        }
    }
}