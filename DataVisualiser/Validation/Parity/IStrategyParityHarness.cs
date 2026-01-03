// File: DataVisualiser/Charts/Parity/IStrategyParityHarness.cs

namespace DataVisualiser.Validation.Parity;
// ---------- Contracts (kept in this file to avoid "missing type" drift) ----------

// ---------- Harness interface ----------

public interface IStrategyParityHarness
{
    ParityResult Validate(StrategyParityContext context, Func<LegacyExecutionResult> legacyExecution, Func<CmsExecutionResult> cmsExecution);
}