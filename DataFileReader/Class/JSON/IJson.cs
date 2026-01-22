namespace DataFileReader.Class.JSON;

public interface IJson
{
    bool IsArray { get; }
    bool IsObject { get; }
    bool IsValue { get; }
    bool IsEmpty { get; }
    string? Name { get; set; }
    IJson? Parent { get; set; }

    IJson As(string rename);
}