using System.Collections;

namespace DataFileReader.Models.Json;

public interface IJsonComplex : IJson, IEnumerable<IJson>, IEnumerable
{
    int Count { get; }
    IJson? this[int index] { get; }

    IJson Add(IJson child);
    bool Contains(string text);
}