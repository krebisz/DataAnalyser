# AiHealthMetrics data shape interrogation

**Constraint:** Read-only. This note uses only evidence obtained from attempts to access the `dataanalyser_sql` MCP server in this session. No SQL, application code, or server configuration was changed.

## 1. MCP connection status

Unavailable. This session has no bound `dataanalyser_sql` tool. A direct MCP initialization request to the existing local MCP endpoint returned HTTP 500. A separate stdio launch of the configured server could not start because port 5000 is already occupied.

No entity records were available through MCP, so the remaining sections deliberately distinguish unavailable observations from facts.

## 2. Exposed entity name

Not observed through an active MCP connection.

## 3. Available fields

Not observed through an active MCP connection.

## 4. Observed metric types/subtypes

Not observed through an active MCP connection.

## 5. Observed units

Not observed through an active MCP connection.

## 6. Candidate time axis

Not established from MCP-accessible records.

## 7. Candidate metric value field

Not established from MCP-accessible records.

## 8. Duplicate/grain issues

Not assessed. No MCP-accessible records were returned for duplicate or grain checks.

## 9. Sentinel/null/missing-value issues

Not assessed. No MCP-accessible records were returned for null, blank, or sentinel-value checks.

## 10. First safe canonicalization assumptions

None should be made yet. The only safe posture is to preserve each returned record and its source fields unchanged until the entity fields, identifier, timestamp semantics, metric identity, unit behavior, and duplicate grain have been observed through MCP.

## 11. First unsafe assumptions

- That the requested entity is exposed and readable in the current MCP session.
- That any timestamp-like field represents a measurement time.
- That a numeric-looking field is comparable across metric types or units.
- That repeated metric rows are duplicates rather than distinct observations.

## 12. Recommended next DataAnalyser implementation step

Restore a working `dataanalyser_sql` binding for a fresh session, then run a bounded read-only entity inspection before implementation: retrieve the entity metadata and a deterministic sample, followed by grouped checks for metric type/subtype, unit, timestamp presence, value nullability, and duplicate candidate keys. Do not introduce canonicalization or charting logic until that evidence exists.
