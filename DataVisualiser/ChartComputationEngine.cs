using System;
using System.Threading.Tasks;

namespace DataVisualiser.Charts
{
    /// <summary>
    /// Pure computation engine — executes strategy Compute() on a background thread and returns the result.
    /// </summary>
    public sealed class ChartComputationEngine
    {
        public Task<ChartComputationResult?> ComputeAsync(IChartComputationStrategy strategy)
        {
            // Run the pure computation on a threadpool thread
            return Task.Run(() =>
            {
                try
                {
                    return strategy.Compute();
                }
                catch
                {
                    // swallow here and return null so callers can clear charts safely
                    return null;
                }
            });
        }
    }
}
