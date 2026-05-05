using DataVisualiser.VNext.Rendering;

namespace DataVisualiser.VNext.Contracts;

public enum ConsumerSurfaceModelKind
{
    None,
    ChartRenderPlan,
    TabularData,
    Evidence,
    DerivedDataset,
    Other
}

public sealed record ConsumerSurfaceModel
{
    public ConsumerSurfaceModel(
        ConsumerSurfaceModelKind kind,
        string surfaceId,
        bool requiresRenderPlan,
        IReadOnlyDictionary<string, string>? metadata = null,
        ChartRenderPlanKind? renderPlanKind = null)
    {
        if (string.IsNullOrWhiteSpace(surfaceId))
            throw new ArgumentException("Surface id cannot be null or empty.", nameof(surfaceId));
        if (kind == ConsumerSurfaceModelKind.ChartRenderPlan && renderPlanKind == null)
            throw new ArgumentException("Chart render-plan surfaces must declare their render-plan kind.", nameof(renderPlanKind));
        if (kind != ConsumerSurfaceModelKind.ChartRenderPlan && renderPlanKind != null)
            throw new ArgumentException("Only chart render-plan surfaces can declare a render-plan kind.", nameof(renderPlanKind));

        Kind = kind;
        SurfaceId = surfaceId;
        RequiresRenderPlan = requiresRenderPlan;
        RenderPlanKind = renderPlanKind;
        Metadata = metadata == null
            ? new Dictionary<string, string>()
            : new Dictionary<string, string>(metadata);
    }

    public ConsumerSurfaceModelKind Kind { get; }
    public string SurfaceId { get; }
    public bool RequiresRenderPlan { get; }
    public ChartRenderPlanKind? RenderPlanKind { get; }
    public IReadOnlyDictionary<string, string> Metadata { get; }

    public static ConsumerSurfaceModel None { get; } = new(
        ConsumerSurfaceModelKind.None,
        "None",
        requiresRenderPlan: false);

    public static ConsumerSurfaceModel FromRenderPlan(ChartRenderPlan renderPlan)
    {
        ArgumentNullException.ThrowIfNull(renderPlan);

        var metadata = new Dictionary<string, string>(renderPlan.Metadata)
        {
            ["PlanKind"] = renderPlan.PlanKind.ToString(),
            ["DisplayMode"] = renderPlan.DisplayMode.ToString(),
            ["Title"] = renderPlan.Title
        };

        return new ConsumerSurfaceModel(
            ConsumerSurfaceModelKind.ChartRenderPlan,
            renderPlan.Id,
            requiresRenderPlan: true,
            metadata,
            renderPlan.PlanKind);
    }

    public static ConsumerSurfaceModel FromDerivedDatasets(IReadOnlyList<DerivedDataset> datasets)
    {
        ArgumentNullException.ThrowIfNull(datasets);
        if (datasets.Count == 0)
            throw new ArgumentException("At least one derived dataset is required.", nameof(datasets));

        var metadata = new Dictionary<string, string>
        {
            [ConstructionMetadataKeys.DatasetCount] = datasets.Count.ToString(),
            [ConstructionMetadataKeys.DatasetIds] = string.Join("|", datasets.Select(dataset => dataset.Id)),
            [ConstructionMetadataKeys.OperationSignatures] = string.Join("|", datasets.Select(dataset => dataset.OperationSignature))
        };

        return new ConsumerSurfaceModel(
            ConsumerSurfaceModelKind.DerivedDataset,
            string.Join("+", datasets.Select(dataset => dataset.Id)),
            requiresRenderPlan: false,
            metadata);
    }
}
