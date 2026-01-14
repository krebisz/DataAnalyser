using DataFileReader.Class;
using DataFileReader.Helper;
using Newtonsoft.Json.Linq;

namespace DataFileReader.Tests;

public class HierarchyRefValTests
{
    private static HierarchyObjectList BuildHierarchy(string json)
    {
        var token = JToken.Parse(json);
        var list = new HierarchyObjectList();
        JsoonHelper.CreateHierarchyObjectList(ref list, token);

        _ = new MetaDataList(list); // computes structural ReferenceValue
        return list;
    }

    [Fact]
    public void Equivalent_subtrees_match_across_files()
    {
        var h1 = BuildHierarchy("""{"a":{"b":1,"c":"x"}}""");
        var h2 = BuildHierarchy("""{"a":{"b":99,"c":"y"}}""");

        var n1 = h1.HierarchyObjects.Single(x => x.Path == "Root.a");
        var n2 = h2.HierarchyObjects.Single(x => x.Path == "Root.a");

        Assert.Equal(n1.ReferenceValue, n2.ReferenceValue);
    }

    [Fact]
    public void Different_leaf_type_breaks_equivalence()
    {
        var h1 = BuildHierarchy("""{"a":{"b":1}}""");
        var h2 = BuildHierarchy("""{"a":{"b":"1"}}""");

        var n1 = h1.HierarchyObjects.Single(x => x.Path == "Root.a.b");
        var n2 = h2.HierarchyObjects.Single(x => x.Path == "Root.a.b");

        Assert.NotEqual(n1.ReferenceValue, n2.ReferenceValue);
    }

    [Fact]
    public void Container_child_set_change_breaks_equivalence()
    {
        var h1 = BuildHierarchy("""{"a":{"b":1}}""");
        var h2 = BuildHierarchy("""{"a":{"b":1,"c":2}}""");

        var n1 = h1.HierarchyObjects.Single(x => x.Path == "Root.a");
        var n2 = h2.HierarchyObjects.Single(x => x.Path == "Root.a");

        Assert.NotEqual(n1.ReferenceValue, n2.ReferenceValue);
    }

    [Fact]
    public void Array_order_change_breaks_equivalence()
    {
        var h1 = BuildHierarchy("""{"a":[{"b":1},{"c":2}]}""");
        var h2 = BuildHierarchy("""{"a":[{"c":2},{"b":1}]}""");

        var n1 = h1.HierarchyObjects.Single(x => x.Path == "Root.a");
        var n2 = h2.HierarchyObjects.Single(x => x.Path == "Root.a");

        Assert.NotEqual(n1.ReferenceValue, n2.ReferenceValue);
    }
}

