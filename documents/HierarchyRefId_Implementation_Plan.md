# Structural Reference Value (RefVal) for HierarchyObject Trees
**Scope:** Replace the current `GetMetaDataObjectReferenceValue` implementation (path-sum of `MetaDataID`) with a deterministic, bottom-up, structural fingerprint that is stable across files and suitable as a DB join key.

This document is based on the current code in:
- `Program.CreateMetaData` and `GetMetaDataObjectReferenceValue` :contentReference[oaicite:0]{index=0} :contentReference[oaicite:1]{index=1}
- `HierarchyObject` and `GenerateMetaDataID` :contentReference[oaicite:2]{index=2}
- `HierarchyObjectList.Add` (how nodes are created) :contentReference[oaicite:3]{index=3}
- `MetaData.RefVal` usage :contentReference[oaicite:4]{index=4}

---

## 1) Problem Statement (Current Gap)

### Current behavior
`RefVal` is currently computed as the **sum of MetaDataID values along the parent chain**: :contentReference[oaicite:5]{index=5}

This produces a “path-sum” that:
- is **not** a structural fingerprint
- ignores children entirely (so it cannot encode subtree shape)
- is collision-prone (integer addition is commutative; different paths can produce identical sums)
- cannot support “same subtree = same key” reliably across datasets

### Desired behavior
`RefVal` must represent **the node’s subtree structure**, so that:
- nodes with equivalent structure (and compatible semantics) share the same `RefVal` across different JSON files
- nodes can be looked up later by `RefVal` in a database to extract matching sub-objects

---

## 2) Definitions

### HierarchyObject Node Kinds
- **Element**: primitive leaf node (`ClassID == "Element"`)
- **Container**: object-like node containing named properties (`ClassID == "Container"`)
- **Array**: array-like node containing ordered items (`ClassID == "Array"`)

(These values are assigned when traversing JSON in `TraverseJson` and `HierarchyObjectList.Add`.) :contentReference[oaicite:6]{index=6} :contentReference[oaicite:7]{index=7}

### Structural Reference Value (RefVal)
A deterministic identifier derived from:
- the node’s kind (Element/Container/Array)
- node semantics (name + primitive type category)
- **children’s RefVals** (for non-leaves)
- ordering rules (arrays are order-sensitive)

---

## 3) Requirements

### 3.1 Functional Requirements
FR1. **Deterministic across runs**  
Same input structure must generate the same `RefVal` (no GUID/random elements).

FR2. **Bottom-up computability**  
Leaf nodes define a base fingerprint; containers derive their fingerprint from children.

FR3. **Structure-sensitive**
- Two containers with different child sets must produce different `RefVal`
- Two arrays with different sequences must produce different `RefVal`

FR4. **Subtree equivalence**
Nodes with identical subtrees (shape + semantics per rules below) must share the same `RefVal` even if:
- they appear in different locations in the overall tree
- they are contained under different parents

FR5. **Name + type semantics for leaves**
For `Element` nodes, `RefVal` must depend on:
- element name
- primitive type category (e.g., decimal/number vs string vs bool vs datetime)
- and kind (`Element`)

FR6. **Persistable join key**
`RefVal` must be suitable to store in DB and use for equality joins. (Prefer `string` or `long` stable format.)

### 3.2 Non-Goals (Explicit)
NG1. The literal leaf value (e.g., “7.5”) should **NOT** be part of the fingerprint (it would destroy equivalence across datasets).

NG2. Absolute depth should **NOT** affect the fingerprint (two equivalent subtrees at different depths must match).

---

## 4) Critical Design Correction: Capture Type Semantics Properly

### Current limitation
For JSON traversal, `HierarchyObject.Value` is set to `jToken.ToString()`, meaning **everything becomes a string**: :contentReference[oaicite:8]{index=8}  
Therefore `hierarchyObject.Value.GetType()` in `CreateMetaData` is almost always `System.String`: :contentReference[oaicite:9]{index=9}  
This cannot satisfy “decimal vs string vs bool” requirements.

### Required change
Store the JSON token type (or an inferred primitive category) on each `HierarchyObject` at creation time.

**Add a new property to `HierarchyObject`:**
```csharp
public string ValueType { get; set; } = ""; 
// Examples: "number", "string", "bool", "date", "null", "object", "array"
```
Populate it in HierarchyObjectList.Add(string path, JToken jToken, string classID).


5) Proposed Algorithm (Canonical Subtree Fingerprint)

5.1 Overview
Compute RefVal as a bottom-up structural hash, similar to a Merkle-tree:
-RefVal(Element) = Hash(kind, name, primitiveTypeCategory)
-RefVal(Container) = Hash(kind, name, sorted(children.RefVal))
-RefVal(Array) = Hash(kind, name, ordered(children.RefVal))

5.2 Ordering rules

-Container children are order-insensitive:
sort by child name, then child RefVal (or stable tuple)

-Array children are order-sensitive:
preserve order by index (or by encounter order / stable ID if indices are not stored)


6) Implementation Plan (Step-by-Step, Concrete)

Step 1 — Extend HierarchyObject to capture value type
File: HierarchyObject.cs
Add property: `public string ValueType { get; set; } = "";` Rationale: required for Element base-case semantics.

(Existing properties shown here.) HierarchyObject


Step 2 — Populate ValueType inside HierarchyObjectList.Add(...)
File: HierarchyObjectList.cs 
HierarchyObjectList

Add helper:
```private static string InferValueType(JToken token)
{
    return token.Type switch
    {
        JTokenType.Integer => "number",
        JTokenType.Float => "number",
        JTokenType.String => "string",
        JTokenType.Boolean => "bool",
        JTokenType.Date => "date",
        JTokenType.Null => "null",
        JTokenType.Object => "object",
        JTokenType.Array => "array",
        _ => token.Type.ToString().ToLowerInvariant()
    };
}
```
Then inside Add(...), after setting hierarchyObject.ClassID and before duplication checks:
`hierarchyObject.ValueType = InferValueType(jToken);`
Notes: For Container and Array, ValueType will be "object"/"array"; for Element, it will be primitive categories.


Step 3 — Stop using GetMetaDataObjectReferenceValue for RefVal
File: Program.cs
Currently both HierarchyObject.RefVal and MetaData.RefVal are set using GetMetaDataObjectReferenceValue: Program
Replace that with a new calculator that computes structural fingerprints.


Step 4 — Add a dedicated calculator (new methods)
File: Program.cs (or better: new file ReferenceValueCalculator.cs)
Add the following methods (complete, drop-in):
```
private static Dictionary<int, List<HierarchyObject>> BuildChildrenIndex(List<HierarchyObject> nodes)
{
    var map = new Dictionary<int, List<HierarchyObject>>();

    foreach (var n in nodes)
    {
        if (n.ParentID is null) continue;

        var pid = n.ParentID.Value;
        if (!map.TryGetValue(pid, out var list))
        {
            list = new List<HierarchyObject>();
            map[pid] = list;
        }
        list.Add(n);
    }

    return map;
}

private static string ComputeElementRefVal(HierarchyObject n)
{
    // Semantic base case: kind + name + primitive category (NOT literal value)
    // Using a string form avoids int overflow and is clearer for DB keys.
    // You can later replace this with SHA-256 if desired.
    return $"E|{n.Name}|{n.ValueType}";
}

private static string ComputeContainerRefVal(
    HierarchyObject n,
    IReadOnlyDictionary<int, List<HierarchyObject>> childrenIndex,
    Dictionary<int, string> memo)
{
    if (memo.TryGetValue(n.ID, out var cached)) return cached;

    childrenIndex.TryGetValue(n.ID, out var children);
    children ??= new List<HierarchyObject>();

    // Order-insensitive: sort for canonicalization
    var childRefs = children
        .Select(c => ComputeRefVal(c, childrenIndex, memo))
        .OrderBy(x => x, StringComparer.Ordinal)
        .ToList();

    var payload = $"C|{n.Name}|{childRefs.Count}|{string.Join(",", childRefs)}";
    memo[n.ID] = payload;
    return payload;
}

private static string ComputeArrayRefVal(
    HierarchyObject n,
    IReadOnlyDictionary<int, List<HierarchyObject>> childrenIndex,
    Dictionary<int, string> memo)
{
    if (memo.TryGetValue(n.ID, out var cached)) return cached;

    childrenIndex.TryGetValue(n.ID, out var children);
    children ??= new List<HierarchyObject>();

    // Order-sensitive: preserve stable order.
    // With current model, best available is ID order (creation order).
    // If you later store an explicit array index, use it here instead.
    var childRefs = children
        .OrderBy(c => c.ID)
        .Select(c => ComputeRefVal(c, childrenIndex, memo))
        .ToList();

    var payload = $"A|{n.Name}|{childRefs.Count}|{string.Join(",", childRefs)}";
    memo[n.ID] = payload;
    return payload;
}

private static string ComputeRefVal(
    HierarchyObject n,
    IReadOnlyDictionary<int, List<HierarchyObject>> childrenIndex,
    Dictionary<int, string> memo)
{
    return n.ClassID switch
    {
        "Element" => ComputeElementRefVal(n),
        "Array" => ComputeArrayRefVal(n, childrenIndex, memo),
        _ => ComputeContainerRefVal(n, childrenIndex, memo) // default Container
    };
}
```
Important: this produces a canonical structural signature string.
If you want a compact key, add a final hashing step (e.g., SHA-256 hex) at the end of each payload.


Step 5 — Apply RefVal computation once, for all nodes (bottom-up)
File: Program.cs
-In CreateMetaData, currently you do: HierarchyObjectList.GenerateMetaIDs(); Program
-compute per node via GetMetaDataObjectReferenceValue Program

Replace the per-node calculation with:
```
var nodes = HierarchyObjectList.HierarchyObjects;

// Build an index for fast child lookup
var childrenIndex = BuildChildrenIndex(nodes);

// Memoization prevents repeated subtree recomputation
var memo = new Dictionary<int, string>();

// Compute RefVal for every node
foreach (var n in nodes.OrderByDescending(x => x.Level ?? 0))
{
    n.RefVal = ComputeRefVal(n, childrenIndex, memo);
}
```

Then when you create MetaData, assign: `metaData.RefVal = hierarchyObject.RefVal;`
This removes the double computation you currently do: (Program)


Step 6 — Deprecate / remove GetMetaDataObjectReferenceValue
File: Program.cs 

Options:
-delete it (recommended to avoid accidental reuse)
-or keep but rename to ComputeLegacyPathSumRefVal and comment “DO NOT USE FOR STRUCTURAL IDENTITY”.


Step 7 — Tighten duplicate behavior (optional but recommended)
Current duplicate detection uses: `MetaDataID + Name + ParentID`
Once RefVal is structural, you can redefine duplicates as: same RefVal (optionally same ClassID)
This will better align the hierarchy list with your goal of canonical equivalence.


7) Validation Checklist (Must Pass)

V1. Equivalent subtrees match
Given two nodes from different files with the same structure and element semantics, RefVal must be identical.

V2. Different leaf type breaks equivalence
Same leaf name, different primitive type category => different RefVal.

V3. Container child changes breaks equivalence
Changing/adding/removing a single child changes the container RefVal.

V4. Array order changes breaks equivalence
Swapping two array items changes the array RefVal.


8) Notes on DB Storage
If you keep RefVal as the payload string (e.g., C|Root|2|...), it is human-inspectable but can be long. Recommended production form:
-Compute payload as above
-Store SHA-256(payload) as RefValHash (fixed length, fast join)
-Optionally store the payload in a separate column for debugging


9) Summary of Required Code Changes

a) HierarchyObject.cs
-add ValueType property

b) HierarchyObjectList.cs
-populate ValueType using JToken.Type

c) Program.cs
-remove usage of GetMetaDataObjectReferenceValue for RefVal
-compute RefVal bottom-up via new structural algorithm
-assign metaData.RefVal = hierarchyObject.RefVal instead of recomputing


Appendix: Why the Old Method Must Be Replaced

The current parent-walk summation method: Program
-cannot encode child structure
-cannot represent subtree equivalence
-is collision-prone by construction

The replacement method explicitly encodes:
-node kind
-node semantics
-canonicalized children

This matches the requirements described for persistence + later extraction by RefVal.