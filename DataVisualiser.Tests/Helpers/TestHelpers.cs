using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Validation.Parity;

namespace DataVisualiser.Tests.Helpers;

/// <summary>
///     Helper methods for parity tests.
/// </summary>
public static class TestHelpers
{
    public static LegacyExecutionResult ToLegacyExecutionResult(this ChartComputationResult? result)
    {
        return ParityResultAdapter.ToLegacyExecutionResult(result);
    }

    public static CmsExecutionResult ToCmsExecutionResult(this ChartComputationResult? result)
    {
        return ParityResultAdapter.ToCmsExecutionResult(result);
    }
}