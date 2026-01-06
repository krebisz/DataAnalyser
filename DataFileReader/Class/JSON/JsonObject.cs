using System.Collections;
using System.Text;
using Newtonsoft.Json;

public class JsonObject : IJsonComplex, IEnumerable<IJson>, IEnumerable
{
    protected static readonly JsonSerializerSettings JSON_SETTINGS = new()
    {
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor | ConstructorHandling.Default,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate
    };

    public List<IJson> Properties { get; set; } = new();

    public IJson? this[string name]
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(name))
                for (var index = 0; index < Count; index++)
                    if (name.Equals(Properties[index].Name, StringComparison.InvariantCultureIgnoreCase))
                        return Properties[index];
            return null;
        }
        set
        {
            if (!string.IsNullOrWhiteSpace(name))
                for (var index = 0; index < Count; index++)
                    if (name.Equals(Properties[index].Name, StringComparison.InvariantCultureIgnoreCase) && value != null)
                        Properties[index] = value;
        }
    }

    public bool IsArray => false;

    public bool IsObject => true;

    public bool IsValue => false;

    public bool IsEmpty => Properties.Count < 1;

    public int Count => Properties.Count;

    public string? Name { get; set; }

    public IJson? Parent { get; set; }

    public IJson? this[int index]
    {
        get => index < 0 || index >= Count ? null : Properties[index];
        set
        {
            if (index >= 0 && index < Count && value != null)
                Properties[index] = value;
        }
    }

    public IEnumerator<IJson> GetEnumerator()
    {
        return Properties.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Properties.GetEnumerator();
    }

    public IJson Add(IJson child)
    {
        if (child != null)
        {
            child.Parent = this;
            Properties.Add(child);
        }

        return this;
    }

    public IJson As(string rename)
    {
        return new JsonObject
        {
                Name = rename,
                Parent = Parent,
                Properties = new List<IJson>(Properties)
        };
    }

    public bool Contains(string text)
    {
        if (!string.IsNullOrWhiteSpace(text))
            for (var index = 0; index < Properties.Count; index++)
                if (text.Equals(Properties[index].Name, StringComparison.InvariantCultureIgnoreCase))
                    return true;
        return false;
    }

    public static IJson Create(string text)
    {
        string? name = null;
        IJson? json = null;

        try
        {
            var buffer = Convert.FromBase64String(text);
            if (buffer != null && buffer.Length > 0)
                text = Encoding.UTF8.GetString(buffer);
        }
        catch
        {
        }

        var reader = new JsonTextReader(new StringReader(text));
        while (reader.Read())
            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    json = new JsonObject
                    {
                            Name = name ?? "root",
                            Parent = json,
                            Properties = new List<IJson>()
                    };
                    if (json.Parent is IJsonComplex)
                        ((IJsonComplex)json.Parent).Add(json);
                    break;
                case JsonToken.EndObject:
                    if (json != null)
                        json = json.Parent ?? json;
                    break;
                case JsonToken.StartArray:
                    json = new JsonArray
                    {
                            Name = name ?? "root",
                            Parent = json,
                            Elements = new List<IJson>()
                    };
                    if (json.Parent is IJsonComplex)
                        ((IJsonComplex)json.Parent).Add(json);
                    break;
                case JsonToken.EndArray:
                    if (json != null)
                        json = json.Parent ?? json;
                    break;
                case JsonToken.PropertyName:
                    name = reader.Value as string;
                    break;
                case JsonToken.Boolean:
                    if (json is IJsonComplex)
                    {
                        var created = JsonValue.Create(name, Convert.ToBoolean(reader.Value));
                        if (created != null)
                            ((IJsonComplex)json).Add(created);
                    }
                    break;
                case JsonToken.Integer:
                    if (json is IJsonComplex)
                    {
                        var created = JsonValue.Create(name, Convert.ToInt64(reader.Value));
                        if (created != null)
                            ((IJsonComplex)json).Add(created);
                    }
                    break;
                case JsonToken.Float:
                    if (json is IJsonComplex)
                    {
                        var created = JsonValue.Create(name, Convert.ToDecimal(reader.Value));
                        if (created != null)
                            ((IJsonComplex)json).Add(created);
                    }
                    break;
                case JsonToken.String:
                    if (json is IJsonComplex)
                    {
                        var created = JsonValue.Create(name, Convert.ToString(reader.Value));
                        if (created != null)
                            ((IJsonComplex)json).Add(created);
                    }
                    break;
                case JsonToken.Null:
                    if (json is IJsonComplex)
                    {
                        var created = JsonValue.Create(name, reader.Value);
                        if (created != null)
                            ((IJsonComplex)json).Add(created);
                    }
                    break;
                case JsonToken.Date:
                    if (json is IJsonComplex)
                    {
                        var created = JsonValue.Create(name, Convert.ToDateTime(reader.Value));
                        if (created != null)
                            ((IJsonComplex)json).Add(created);
                    }
                    break;
                case JsonToken.Bytes:
                    if (json is IJsonComplex)
                    {
                        var value = reader.Value is byte[] bytes ? bytes : reader.Value is string base64 ? Convert.FromBase64String(base64) : reader.Value;
                        var created = JsonValue.Create(name, value);
                        if (created != null)
                            ((IJsonComplex)json).Add(created);
                    }
                    break;
            }

        while (json?.Parent != null)
            json = json.Parent;

        if (json is JsonArray array && array.Count == 1)
            json = array[0] ?? json;
        if (json is JsonObject obj && obj.Count == 1 && obj[0]?.IsObject == true)
            json = obj[0];

        return json!;
    }

    public static string Serialize(dynamic @object)
    {
        return JsonConvert.SerializeObject(@object, Formatting.Indented, JSON_SETTINGS);
    }

    public static string Serialize<T>(T @object)
    {
        return JsonConvert.SerializeObject(@object, typeof(T), Formatting.Indented, JSON_SETTINGS);
    }
}
