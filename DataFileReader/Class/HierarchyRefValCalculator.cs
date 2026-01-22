using System.Security.Cryptography;
using System.Text;

namespace DataFileReader.Class;

public static class HierarchyRefValCalculator
{
    public static void AssignStructuralReferenceValues(IReadOnlyList<HierarchyObject> nodes)
    {
        if (nodes is null)
            throw new ArgumentNullException(nameof(nodes));

        var childrenIndex = nodes.Where(n => n.ParentID.HasValue).GroupBy(n => n.ParentID!.Value).ToDictionary(g => g.Key, g => g.ToList());

        var memo = new Dictionary<int, string>();

        foreach (var node in nodes.OrderByDescending(n => n.Level ?? 0))
            node.ReferenceValue = ComputeRefVal(node, childrenIndex, memo);
    }

    private static string ComputeRefVal(HierarchyObject node, IReadOnlyDictionary<int, List<HierarchyObject>> childrenIndex, Dictionary<int, string> memo)
    {
        if (memo.TryGetValue(node.ID, out var cached))
            return cached;

        string payload;

        switch (node.ClassID)
        {
            case "Element":
                payload = $"E|{node.Name}|{NormalizeValueType(node.ValueType)}";
                break;

            case "Array":
                payload = ComputeArrayPayload(node, childrenIndex, memo);
                break;

            default:
                payload = ComputeContainerPayload(node, childrenIndex, memo);
                break;
        }

        var refVal = Sha256Hex(payload);
        memo[node.ID] = refVal;
        return refVal;
    }

    private static string ComputeContainerPayload(HierarchyObject node, IReadOnlyDictionary<int, List<HierarchyObject>> childrenIndex, Dictionary<int, string> memo)
    {
        childrenIndex.TryGetValue(node.ID, out var children);
        children ??= new List<HierarchyObject>();

        var childRefs = children.Select(c => (c.Name, Ref: ComputeRefVal(c, childrenIndex, memo))).OrderBy(x => x.Name, StringComparer.Ordinal).ThenBy(x => x.Ref, StringComparer.Ordinal).Select(x => x.Ref).ToList();

        return $"C|{node.Name}|{childRefs.Count}|{string.Join(",", childRefs)}";
    }

    private static string ComputeArrayPayload(HierarchyObject node, IReadOnlyDictionary<int, List<HierarchyObject>> childrenIndex, Dictionary<int, string> memo)
    {
        childrenIndex.TryGetValue(node.ID, out var children);
        children ??= new List<HierarchyObject>();

        var childRefs = children.OrderBy(c => c.ID).Select(c => ComputeRefVal(c, childrenIndex, memo)).ToList();

        return $"A|{node.Name}|{childRefs.Count}|{string.Join(",", childRefs)}";
    }

    private static string NormalizeValueType(string? valueType)
    {
        return string.IsNullOrWhiteSpace(valueType) ? "unknown" : valueType.Trim().ToLowerInvariant();
    }

    private static string Sha256Hex(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}