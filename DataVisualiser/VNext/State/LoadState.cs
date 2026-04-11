using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.VNext.State;

public enum LoadLifecycle
{
    Empty = 0,
    Pending = 1,
    Loaded = 2,
    Failed = 3
}

public sealed record LoadState(
    LoadLifecycle Lifecycle,
    MetricLoadSnapshot? Snapshot,
    string? ErrorMessage)
{
    public static LoadState Empty { get; } = new(LoadLifecycle.Empty, null, null);
}
