namespace DataVisualiser.UI.Charts.Infrastructure;

internal readonly record struct RenderingHostTarget<TRoute, THost>(TRoute Route, THost Host);
