using System.Collections;

namespace DataFileReader.Class.JSON;

public interface IJsonComplex : IJson, IEnumerable<IJson>, IEnumerable
{
    int Count { get; }
    IJson? this[int index] { get; }

    IJson Add(IJson child);
    bool Contains(string text);
}