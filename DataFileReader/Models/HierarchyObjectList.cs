using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace DataFileReader.Models;

public class HierarchyObjectList
{
    private const string DuplicateKeySeparator = "\u001F";
    private readonly HashSet<string> _duplicateKeys = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> _pathToId = new(StringComparer.Ordinal);

    public HierarchyObjectList()
    {
        HierarchyObjects = new List<HierarchyObject>();
    }

    public List<HierarchyObject> HierarchyObjects { get; set; }

    public void GenerateMetaIDs()
    {
        foreach (var hierarchyObject in HierarchyObjects)
        {
            var type = hierarchyObject.Value.GetType();

            if (string.IsNullOrEmpty(hierarchyObject.Name))
                hierarchyObject.Name = Guid.NewGuid().ToString();

            hierarchyObject.Fields.Add(hierarchyObject.Value, type);
            hierarchyObject.GenerateMetaDataID();
        }
    }

    public void Add(string path, JToken jToken, string classID)
    {
        var hierarchyObject = new HierarchyObject();

        hierarchyObject.ID = HierarchyObjects.Count + 1; // Simple ID generation
        hierarchyObject.Name = ExtractNameFromPath(path);
        hierarchyObject.ParentID = FindParentID(path);
        hierarchyObject.Level = FindLevel(hierarchyObject.ParentID);
        hierarchyObject.ClassID = classID;
        hierarchyObject.Path = path;
        hierarchyObject.ValueType = GetValueType(jToken, classID);


        if (classID == "Container")
            hierarchyObject.Value = string.Concat(jToken.Children<JProperty>().Select(child => $"{child.Name}; "));
        else if (classID == "Array")
        {
            var childCount = jToken is JArray array ? array.Count : jToken.Children().Count();
            hierarchyObject.Value = string.Concat(Enumerable.Range(0, childCount).Select(i => $"{hierarchyObject.Name}[{i}]; "));
        }
        else
            hierarchyObject.Value = jToken.ToString();

        hierarchyObject.MetaDataID = null; // Default MetaDataID, can be set later
        hierarchyObject.Fields = new Dictionary<string, Type>();
        hierarchyObject.GenerateMetaDataID();

        if (!isDuplicate(hierarchyObject))
        {
            HierarchyObjects.Add(hierarchyObject);
            if (!_pathToId.ContainsKey(hierarchyObject.Path))
                _pathToId[hierarchyObject.Path] = hierarchyObject.ID;
            _duplicateKeys.Add(CreateDuplicateKey(hierarchyObject));
        }
    }

    private static string ExtractNameFromPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        var lastDot = path.LastIndexOf('.');
        return lastDot >= 0 ? path[(lastDot + 1)..] : path;
    }

    public int? FindParentID(string path)
    {
        var parentPath = GetParentPath(path);
        if (string.IsNullOrEmpty(parentPath))
            return null;

        if (_pathToId.TryGetValue(parentPath, out var parentId))
            return parentId;

        return HierarchyObjects.FirstOrDefault(h => h.Path == parentPath)?.ID;
    }

    private static string? GetParentPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        if (string.Equals(path, "Root", StringComparison.Ordinal))
            return null;

        if (path.EndsWith(']'))
        {
            var match = Regex.Match(path, @"^(.*)\[\d+\]$");
            if (match.Success)
                return match.Groups[1].Value;
        }

        var lastDot = path.LastIndexOf('.');
        return lastDot >= 0 ? path[..lastDot] : null;
    }

    private static string GetValueType(JToken token, string classID)
    {
        return classID switch
        {
                "Container" => "object",
                "Array" => "array",
                _ => token.Type switch
                {
                        JTokenType.Integer => "number",
                        JTokenType.Float => "number",
                        JTokenType.String => "string",
                        JTokenType.Boolean => "bool",
                        JTokenType.Date => "date",
                        JTokenType.Null => "null",
                        JTokenType.Undefined => "null",
                        _ => token.Type.ToString().ToLowerInvariant()
                }
        };
    }

    public int? FindLevel(int? parentID)
    {
        var level = 0;

        if (parentID != null)
        {
            var parent = HierarchyObjects.FirstOrDefault(h => h.ID == parentID);
            if (parent?.Level != null)
                level = parent.Level.Value + 1;
        }

        return level;
    }

    public bool isDuplicate(HierarchyObject hierarchyObject)
    {
        //return HierarchyObjects.Any(h => h.MetaDataID == hierarchyObject.MetaDataID && h.Name == hierarchyObject.Name && h.ParentID == hierarchyObject.ParentID);
        var key = CreateDuplicateKey(hierarchyObject);
        if (_duplicateKeys.Contains(key))
            return true;

        return HierarchyObjects.Any(h => h.MetaDataID == hierarchyObject.MetaDataID && h.Name == hierarchyObject.Name && h.ParentID == hierarchyObject.ParentID && hierarchyObject.Path == h.Path);
    }

    private static string CreateDuplicateKey(HierarchyObject hierarchyObject)
    {
        return string.Join(
            DuplicateKeySeparator,
            hierarchyObject.MetaDataID?.ToString() ?? string.Empty,
            hierarchyObject.Name,
            hierarchyObject.ParentID?.ToString() ?? string.Empty,
            hierarchyObject.Path);
    }
}
