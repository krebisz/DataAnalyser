namespace DataFileReader.Models.Json;

public interface IJsonPrimitive : IJson
{
    TypeCode TypeCode { get; }
    Type Type { get; }
    object? Value { get; set; }
}