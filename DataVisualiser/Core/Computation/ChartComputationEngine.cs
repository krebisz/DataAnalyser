using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Strategies.Abstractions;
using System.Diagnostics;

namespace DataVisualiser.Core.Computation;

/// <summary>
///     Pure computation engine — executes strategy Compute() on a background thread and returns the result.
/// </summary>
public sealed class ChartComputationEngine
{
    public Task<ChartComputationResult?> ComputeAsync(IChartComputationStrategy strategy)
    {
        if (strategy == null)
        {
            Debug.WriteLine("ChartComputationEngine.ComputeAsync: Strategy is null");
            return Task.FromResult<ChartComputationResult?>(null);
        }

        // Run the pure computation on a threadpool thread
        return Task.Run(() =>
        {
            try
            {
                var result = strategy.Compute();
                if (result == null)
                    Debug.WriteLine($"ChartComputationEngine.ComputeAsync: Strategy '{strategy.GetType().Name}' returned null (likely no data)");
                return result;
            }
            catch (Exception ex)
            {
                // Log the exception with context before returning null
                Debug.WriteLine($"ChartComputationEngine.ComputeAsync: Exception in strategy '{strategy?.GetType().Name ?? "Unknown"}': {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                    Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                // Return null so callers can clear charts safely
                return null;
            }
        });
    }
}
