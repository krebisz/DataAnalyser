using DataVisualiser.Core.Computation.Results;

namespace DataVisualiser.Core.Strategies.Abstractions;

public interface IChartComputationStrategy
{
    /// <summary>
    ///     Optional friendly name for series/default labels.
    /// </summary>
    string PrimaryLabel { get; }

    string SecondaryLabel { get; }

    /// <summary>
    ///     Unit (optional) for Y-axis labeling.
    /// </summary>
    string? Unit { get; }

    /// <summary>
    ///     Pure data computation: given inputs produce a ChartComputationResult.
    ///     Must be synchronous (invoked inside a Task.Run by the engine).
    ///     Returns null if no data is available or computation fails.
    /// </summary>
    ChartComputationResult? Compute();
}
