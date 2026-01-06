using System.Text.Json;
using DataFileReader.Class;
using Newtonsoft.Json.Linq;

namespace DataFileReader.Helper;

public static class JsoonHelper
{
    public static List<string> GetFieldList(JArray jsonArray)
    {
        var fieldList = new List<string>();

        foreach (var jsonToken in jsonArray)
        {
            var jsonString = DataHelper.RemoveEscapeCharacters(jsonToken.ToString());

            var dynamicObject = JsonSerializer.Deserialize<dynamic>(jsonString);
            var dynamicText = dynamicObject?.ToString() ?? string.Empty;
            dynamicText = "[" + dynamicText + "]";

            var childJsonArray = JArray.Parse(dynamicText) ?? new JArray();

            if (childJsonArray.Count > 0)
                foreach (JObject childJsonObject in childJsonArray)
                {
                    var childJsonValues = childJsonObject.Values();

                    foreach (var childJsonValue in childJsonValues)
                    {
                        fieldList.Add(childJsonValue.ToString());

                        if (childJsonValue != null && childJsonValue.HasValues)
                        {
                            var subArray = new JArray(childJsonValue);

                            GetFieldList(subArray);
                        }
                    }
                }
        }

        return fieldList;
    }

    public static void CreateHierarchyObjectList(ref HierarchyObjectList hierarchyObjectList, JToken token, string path = "Root")
    {
        if (token is JObject obj)
            foreach (var prop in obj.Properties())
            {
                var currentPath = string.IsNullOrEmpty(path) ? prop.Name : $"{path}.{prop.Name}";
                hierarchyObjectList.Add(path, token, "Container");
                CreateHierarchyObjectList(ref hierarchyObjectList, prop.Value, currentPath);
            }
        else if (token is JArray array)
            for (var i = 0; i < array.Count; i++)
            {
                var currentPath = $"{path}[{i}]";
                hierarchyObjectList.Add(path, token, "Array");
                CreateHierarchyObjectList(ref hierarchyObjectList, array[i], currentPath);
            }
        else
                // Primitive value (string, number, bool, etc.)
            hierarchyObjectList.Add(path, token, "Element");
    }
}
