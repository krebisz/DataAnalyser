using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataVisualiser.Charts
{
    public interface IChartComputationStrategy
    {
        /// <summary>
        /// Pure data computation: given inputs produce a ChartComputationResult.
        /// Must be synchronous (invoked inside a Task.Run by the engine).
        /// </summary>
        ChartComputationResult Compute();

        /// <summary>
        /// Optional friendly name for series/default labels.
        /// </summary>
        string PrimaryLabel { get; }
        string SecondaryLabel { get; }

        /// <summary>
        /// Unit (optional) for Y-axis labeling.
        /// </summary>
        string? Unit { get; }
    }
}
