namespace DataFileReader.Class;

public class MetaData
{
    public MetaData()
    {
        ID = 0;
        Name = string.Empty;
        ReferenceValue = string.Empty;
        Type = string.Empty;
        Fields = new Dictionary<string, Type>();
    }

    public int ID { get; set; }

    public string Name { get; set; }

    public string ReferenceValue { get; set; }

    public string Type { get; set; }

    public Dictionary<string, Type> Fields { get; set; }

    public void GenerateID()
    {
        //ID = Fields.OrderBy(field => field.Key).Aggregate(0, (hash, field) => HashCode.Combine(hash, field.Key.GetHashCode(), field.Value.GetHashCode()));
        ID = Math.Abs(Fields.OrderBy(field => field.Key).
                             Aggregate(0, (hash, field) => HashCode.Combine(hash, field.Key.GetHashCode(), field.Value.GetHashCode())));
    }
}
