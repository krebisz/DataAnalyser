# External Review Rules

This document defines the rules for the external review / response cycle.

It is not a replacement for the consolidation plan.
It tells the reviewing agent how to assess the current position and how any proposed next cycle must stay aligned with the plan's actual execution model.

## Authority And Context

The reviewing agent must treat the following as the governing context:

1. the current codebase state
2. [DataVisualiser_Consolidation_Plan.md](c:\Development\POCs\DataAnalyser\documents\DataVisualiser_Consolidation_Plan.md)
3. [log.md](c:\Development\POCs\DataAnalyser\documents\log.md)
4. the current uncommitted workspace changes, where relevant

The review must be based on the code as it exists now, not on an imagined clean-room rewrite.

The reviewing agent must also respect the current historical position in the plan:

- the completed cycle already moved through bounded work across the plan's `Phase A` through `Phase F` path
- the review must judge that work from the end-of-cycle baseline, not from earlier partial checkpoints
- the plan may not be rewritten retroactively except to acknowledge what has already been completed

## Plan-Aligned Execution Model

Any critique or next-cycle proposal must stay aligned with the execution rules already established in the plan.

That means the reviewing agent must work from these assumptions:

- one iteration must have one primary objective
- work must stay bounded to a real subsystem slice
- safely coupled slices are allowed only when they already share a real contract / host / route pattern and batching them reduces duplicated churn
- regression protection must be established or refreshed before mutation
- simplification comes before consolidation
- consolidation comes before abstraction
- abstraction is only justified after a repeated pattern is actually proved
- retire / remove / clean is expected during each pass, but broad cleanup must still stay bounded
- validation must occur after every significant pass
- if live behavior changes, automated validation must be followed by a targeted manual smoke halt before continuing

The reviewing agent must not ignore this iterative flow and jump straight to a broad mass rewrite unless it can justify, concretely, why that would still be safer and more aligned than the established bounded approach.

## Required Iteration Flow

If the reviewing agent proposes a next cycle, it must express that proposal in the same operational shape used by the plan.

Each proposed iteration should be framed through this flow:

1. establish / refresh regression protection
2. select a bounded subsystem slice
3. inventory the current shape
4. constrain / simplify / reorganize
5. consolidate / coalesce
6. generalize / abstract only if the pass proves it
7. retire / remove / clean
8. validate / re-measure
9. record the new baseline

If the reviewing agent believes a step should be skipped, it must explain why that deviation is justified.

## Questions To Answer

The reviewing agent must answer these three questions directly:

1. Given the current state of the code, to what extent have the outlined objectives of the plan been met?
2. What gaps, inefficiencies, risks, or missed opportunities remain?
3. How would you reach the objectives of the plan in the next major cycle?

If something cannot reasonably be completed in one major cycle, that limitation must be stated explicitly.

## Required Review Standard

The reviewing agent must:

- evaluate the current result against the actual objectives, phases, and guardrails in the plan
- distinguish between:
  - completed work
  - remaining intentional debt
  - true regressions or weak decisions
- judge the work from the current repository state, not from partial historical checkpoints
- identify where the implementation is defensible and where it is not
- avoid cosmetic criticism that is not tied to maintainability, correctness, risk, or alignment with the plan
- respect the difference between:
  - a bad decision at the time it was made
  - a reasonable bounded decision that later left intentional debt for a safer downstream phase

## Constraints On Next-Cycle Proposals

Any next-cycle proposal must:

- identify the primary objective of each proposed iteration
- identify the bounded slice for each proposed iteration
- state what regression protection should exist first
- state what validation should be run
- state whether manual smoke would likely be required
- state what should remain intentionally deferred if it cannot safely fit in the same cycle

Any next-cycle proposal must not:

- pretend that all remaining debt can be removed safely in one sweep if that is not credible
- reopen already stabilized seams casually without a concrete structural reason
- optimize for file-count alone
- force abstractions across honest outliers without proving convergence
- replace the bounded iteration model with vague architectural preference

## Safety And Forfeit Rules

The following standards apply to any proposed or executed follow-up work:

- incorrect, broken, or regressive implementations are treated as a forfeit
- substantial regressions count more heavily than minor missed cleanups
- speculative abstraction or cosmetic churn without structural payoff is not acceptable
- if a suggested cycle cannot be completed safely within one major cycle, that must be stated plainly
- no credit is given for pretending that remaining debt is solved when it is only renamed or moved

## What Counts As A Strong Response

A strong response should:

- acknowledge the completed structural gains where they are real
- identify the remaining outliers honestly
- explain why some files or seams were left alone
- propose a next cycle that is bounded, defensible, and proportionate
- separate high-value next-cycle work from lower-value cleanup
- tie its proposal back to the actual plan flow instead of offering a disconnected rewrite strategy

## Rebuttal And Next-Cycle Flow

The exchange will proceed as follows:

1. The reviewing agent provides its assessment and next-cycle proposal.
2. That assessment is sent back to the current implementation agent for rebuttal.
3. The reviewing agent receives that rebuttal.
4. After that, two isolated actions occur:
   - the current implementation agent produces its second major-cycle plan outline.
   - the reviewing agent is allowed to perform its first cycle.
5. The current implementation agent then critiques the reviewing agent's implementation against the plan and the stated objectives.

## Evaluation Criteria

The human evaluator will judge each step by:

- honesty
- consistency
- integrity
- alignment with the plan
- correctness of technical reasoning
- willingness to state limits instead of over-claiming

## Additional Guidance

- Do not change the plan retroactively except to acknowledge what has already been completed.
- Do not ignore the current code in favor of a preferred architecture that was never earned by the actual refactor sequence.
- Do not optimize for file-count alone; structural payoff and correctness matter more.
- If you challenge a decision, explain the concrete alternative and why it would have been safer, faster, or more aligned.
- If you believe the current cycle has already materially met a plan objective, say so directly.
- If you believe a remaining area is still weak, name the file or subsystem and explain why.
- If you believe a next-cycle proposal needs more than one iteration, justify why the bounded-slice model still requires that.
