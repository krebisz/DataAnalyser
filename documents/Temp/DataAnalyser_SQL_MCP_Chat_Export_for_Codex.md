# DataAnalyser SQL MCP Integration — Chat Export for Codex

## Purpose

This file summarizes the ChatGPT discussion about wiring **DataAnalyser** to SQL Server through **MCP** so that Codex/AI can interrogate SQL data for analytical patterns.

The immediate goal is **not** to build a custom C# MCP server yet.

The current target is:

> Connect Codex in VS Code to a read-only SQL MCP server using Microsoft Data API Builder (`dab`) so Codex can inspect and sample a SQL view over `Health.dbo.HealthMetrics`.

---

## Project context

Project:

```text
C:\Development\DataAnalyser
```

Database:

```text
Health
```

SQL Server connection currently used by the project:

```xml
<add key="HealthDB"
     value="Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Health;Integrated Security=SSPI;TrustServerCertificate=True;Connection Timeout=10" />
```

Converted for Data API Builder `.env` use:

```env
MSSQL_CONNECTION_STRING=Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Health;Integrated Security=SSPI;TrustServerCertificate=True;Connection Timeout=10
```

Important: this is **LocalDB**, so DAB/Codex must run as the same Windows user that can access `(localdb)\MSSQLLocalDB`.

---

## SQL table being exposed

The real source table is:

```sql
SELECT TOP (1000) [Id],
      [Provider],
      [MetricType],
      [MetricSubtype],
      [SourceFile],
      [NormalizedTimestamp],
      [RawTimestamp],
      [Value],
      [Unit],
      [MetaDataId],
      [CreatedDate]
FROM [Health].[dbo].[HealthMetrics]
```

The intended analytical model is roughly:

```text
MetricType + MetricSubtype + Unit
        ↓
Value over NormalizedTimestamp
        ↓
grouped by Provider / SourceFile / MetaDataId
```

Likely DataAnalyser feature implications:

```text
Metric timeline
Metric grouping by type/subtype
Unit-aware charting
Provider comparison
Missing timestamp detection
Duplicate timestamp detection
Outlier detection per metric type/subtype/unit
Import provenance tracing via SourceFile
```

---

## Recommended integration order

The re-evaluated shortlist was:

1. **SQL MCP Server / Data API Builder**
   - First priority.
   - Gives Codex controlled read-only access to SQL Server entities.

2. **VS Code MSSQL extension**
   - Useful for schema browsing, query testing, and SQL Server work inside VS Code.

3. **Read-only SQL views for AI**
   - Safer and cleaner than exposing raw tables.

4. **Project `AGENTS.md` / Codex instructions**
   - Tells Codex how it may use SQL MCP safely.

5. **Custom `DataAnalyser.McpServer`**
   - Later.
   - Expose project-specific tools such as `profile_table`, `run_tests`, `find_outliers`, `suggest_charts`.

6. **DuckDB + Parquet**
   - Later analytical sidecar.

7. **Great Expectations / Soda Core**
   - Later validation/data-quality layer.

---

## Step 1 — Data API Builder installed

DAB version in use:

```text
Microsoft.DataApiBuilder 2.0.8
```

Validation output later confirmed:

```text
info: Microsoft.DataApiBuilder 2.0.8
info: User provided config file: dab-config.json
info: Validating config file: dab-config.json
info: The config satisfies the schema requirements.
info: Validating entity relationships.
info: [AiHealthMetrics] REST path: /api/AiHealthMetrics
info: Config is valid.
```

---

## Step 2 — AI-facing SQL view

The view recommended for exposing data to AI:

```sql
USE [Health];
GO

CREATE OR ALTER VIEW [dbo].[vw_AiHealthMetrics]
AS
SELECT TOP (1000)
      [Id],
      [Provider],
      [MetricType],
      [MetricSubtype],
      [SourceFile],
      [NormalizedTimestamp],
      [RawTimestamp],
      [Value],
      [Unit],
      [MetaDataId],
      [CreatedDate]
FROM [dbo].[HealthMetrics];
GO
```

Reason: expose a controlled view rather than the raw table.

---

## Step 3 — `.mcp/.env`

The user did not initially have this file. It was manually created.

Commands:

```powershell
cd C:\Development\DataAnalyser

mkdir .mcp -Force
New-Item .mcp\.env -ItemType File -Force
notepad .mcp\.env
```

Contents:

```env
MSSQL_CONNECTION_STRING=Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Health;Integrated Security=SSPI;TrustServerCertificate=True;Connection Timeout=10
```

`.gitignore` should include:

```gitignore
# Local MCP / Data API Builder secrets
.mcp/.env
```

---

## Step 4 — Initialize DAB config

From:

```powershell
C:\Development\DataAnalyser\.mcp
```

Command used:

```powershell
dab init --database-type mssql --connection-string "@env('MSSQL_CONNECTION_STRING')" --host-mode Development --config dab-config.json
```

Result:

```text
info: Microsoft.DataApiBuilder 2.0.8
info: Generating user provided config file with name: dab-config.json
info: Config file generated.
info: SUGGESTION: Use 'dab add [entity-name] [options]' to add new entities in your config.
```

---

## Step 5 — Add the view entity

The first attempted command failed:

```powershell
dab add AiHealthMetrics `
  --source dbo.vw_AiHealthMetrics `
  --source.type view `
  --permissions "anonymous:read" `
  --fields.name Id `
  --fields.primary-key true `
  --description "Read-only health metric records exposed for AI-assisted DataAnalyser exploration."
```

Failure:

```text
fail: Key-fields are mandatory for views, but not provided.
fail: Unable to create the source object.
fail: Failed to add a new entity.
```

Corrected command:

```powershell
dab add AiHealthMetrics `
  --config dab-config.json `
  --source dbo.vw_AiHealthMetrics `
  --source.type view `
  --source.key-fields Id `
  --permissions "anonymous:read" `
  --description "Read-only health metric records exposed for AI-assisted DataAnalyser exploration."
```

Key correction:

```powershell
--source.key-fields Id
```

DAB views need explicit key fields.

---

## Step 6 — Field descriptions

These were recommended after adding the entity.

```powershell
dab update AiHealthMetrics `
  --config dab-config.json `
  --fields.name Id `
  --fields.description "Unique health metric record identifier."
```

```powershell
dab update AiHealthMetrics `
  --config dab-config.json `
  --fields.name Provider `
  --fields.description "Data provider or source system that produced the metric."
```

```powershell
dab update AiHealthMetrics `
  --config dab-config.json `
  --fields.name MetricType `
  --fields.description "Primary metric category, such as weight, heart rate, sleep, activity, blood pressure, or another health measurement type."
```

```powershell
dab update AiHealthMetrics `
  --config dab-config.json `
  --fields.name MetricSubtype `
  --fields.description "More specific subtype or classification of the metric within MetricType."
```

```powershell
dab update AiHealthMetrics `
  --config dab-config.json `
  --fields.name SourceFile `
  --fields.description "Original imported source file associated with the metric record."
```

```powershell
dab update AiHealthMetrics `
  --config dab-config.json `
  --fields.name NormalizedTimestamp `
  --fields.description "Normalized timestamp used by DataAnalyser for chronological analysis and charting."
```

```powershell
dab update AiHealthMetrics `
  --config dab-config.json `
  --fields.name RawTimestamp `
  --fields.description "Original timestamp value as supplied by the source data before normalization."
```

```powershell
dab update AiHealthMetrics `
  --config dab-config.json `
  --fields.name Value `
  --fields.description "Metric value captured for the health measurement. Interpret together with Unit, MetricType, and MetricSubtype."
```

```powershell
dab update AiHealthMetrics `
  --config dab-config.json `
  --fields.name Unit `
  --fields.description "Unit of measurement for Value, such as kg, bpm, steps, minutes, hours, mmHg, or percent."
```

```powershell
dab update AiHealthMetrics `
  --config dab-config.json `
  --fields.name MetaDataId `
  --fields.description "Identifier linking this metric record to additional metadata, import metadata, or source-specific details."
```

```powershell
dab update AiHealthMetrics `
  --config dab-config.json `
  --fields.name CreatedDate `
  --fields.description "Date and time when this record was created or imported into the Health database."
```

---

## Step 7 — Validate DAB config

Command:

```powershell
cd C:\Development\DataAnalyser\.mcp
dab validate --config dab-config.json
```

Result:

```text
info: Microsoft.DataApiBuilder 2.0.8
info: User provided config file: dab-config.json
info: Validating config file: dab-config.json
info: The config satisfies the schema requirements.
info: Validating entity relationships.
info: [AiHealthMetrics] REST path: /api/AiHealthMetrics
info: Config is valid.
```

Conclusion:

```text
DAB config is valid.
```

---

## Step 8 — DAB MCP stdio start command

Initial command used:

```powershell
dab start --mcp-stdio role:anonymous --loglevel error --config dab-config.json
```

Failure:

```text
ERROR(S):
  Option 'loglevel' is unknown.
```

Correction:

```powershell
dab start --mcp-stdio role:anonymous --LogLevel error --config dab-config.json
```

This works from PowerShell.

Important detail:

```text
DAB 2.0.8 requires --LogLevel, not --loglevel.
```

Current confirmed state:

```text
dab validate ✅
dab start --mcp-stdio role:anonymous --LogLevel error --config dab-config.json ✅
```

---

## Step 9 — Codex config attempt

Project-local Codex file did not originally exist:

```text
C:\Development\DataAnalyser\.codex\config.toml
```

It was suggested to manually create it, but later the recommendation changed to use user-level Codex config first.

Current recommended user-level config path:

```text
C:\Users\<YourWindowsUser>\.codex\config.toml
```

Codex is now confirmed to read this config because adding `required = true` caused Codex to fail entirely with a "required MCP servers are not available" style error.

That means:

```text
Codex is reading the config ✅
DAB starts manually ✅
Codex still fails to initialize the MCP server ❌
```

---

## User-level Codex MCP config — direct `dab.exe` version

This was one attempted version:

```toml
[mcp_servers.dataanalyser_sql]
enabled = true
command = "C:\\Users\\<YourWindowsUser>\\.dotnet\\tools\\dab.exe"
args = [
  "start",
  "--mcp-stdio",
  "role:anonymous",
  "--LogLevel",
  "error",
  "--config",
  "dab-config.json"
]
cwd = "C:\\Development\\DataAnalyser\\.mcp"
startup_timeout_sec = 30
tool_timeout_sec = 60
default_tools_approval_mode = "prompt"
```

Important diagnostic line temporarily used:

```toml
required = true
```

That should now be removed because it prevents Codex from starting if the server fails.

---

## User-level Codex MCP config — PowerShell wrapper version

This was suggested as the next safer version because the manual PowerShell command works.

```toml
[mcp_servers.dataanalyser_sql]
enabled = true
command = "powershell.exe"
args = [
  "-NoProfile",
  "-ExecutionPolicy",
  "Bypass",
  "-Command",
  "cd 'C:\\Development\\DataAnalyser\\.mcp'; dab start --mcp-stdio role:anonymous --LogLevel error --config dab-config.json"
]
cwd = "C:\\Development\\DataAnalyser\\.mcp"
startup_timeout_sec = 30
tool_timeout_sec = 60
default_tools_approval_mode = "prompt"
```

If using this, remove:

```toml
required = true
```

---

## Optional VS Code MCP config

Create/update:

```text
C:\Development\DataAnalyser\.vscode\mcp.json
```

Suggested content:

```json
{
  "servers": {
    "dataanalyser_sql": {
      "type": "stdio",
      "command": "powershell.exe",
      "args": [
        "-NoProfile",
        "-ExecutionPolicy",
        "Bypass",
        "-Command",
        "cd 'C:\\Development\\DataAnalyser\\.mcp'; dab start --mcp-stdio role:anonymous --LogLevel error --config dab-config.json"
      ],
      "cwd": "C:\\Development\\DataAnalyser\\.mcp"
    }
  }
}
```

Then in VS Code:

```text
Ctrl + Shift + P
MCP: List Servers
```

Look for:

```text
dataanalyser_sql
```

---

## Current failure point

VS Code/Codex reports:

```text
Available MCP servers: codex_apps, datascienceWidgets, node_repl, openai_api_key_local_confirmation.
dataanalyser_sql is not connected.
```

But PowerShell reports:

```powershell
PS C:\Development\DataAnalyser\.mcp> dab start --mcp-stdio role:anonymous --LogLevel error --config dab-config.json
```

and the command runs.

Current diagnosis:

```text
DAB side works.
The issue is Codex/VS Code MCP server initialization/discovery.
```

Most likely causes remaining:

1. Codex launch process cannot run the command exactly as configured.
2. VS Code/Codex environment differs from normal PowerShell.
3. Codex is not loading the same MCP config layer expected.
4. Codex is using WSL or another runtime boundary.
5. The MCP server process starts but emits output that breaks stdio protocol.
6. Codex does not tolerate the PowerShell command wrapper.
7. The server starts but initialization times out.
8. There may be a subtle TOML syntax/config issue.

---

## Next diagnostic prompt for Codex

Ask Codex/IDE agent to inspect and resolve this specific issue:

```text
We are trying to connect a Data API Builder MCP stdio server to Codex in VS Code.

Known facts:
- Project path: C:\Development\DataAnalyser
- DAB config path: C:\Development\DataAnalyser\.mcp\dab-config.json
- DAB .env path: C:\Development\DataAnalyser\.mcp\.env
- DAB version: Microsoft.DataApiBuilder 2.0.8
- `dab validate --config dab-config.json` succeeds from C:\Development\DataAnalyser\.mcp
- `dab start --mcp-stdio role:anonymous --LogLevel error --config dab-config.json` runs from PowerShell in C:\Development\DataAnalyser\.mcp
- Codex reports that `dataanalyser_sql` is not connected.
- Adding `required = true` to the Codex MCP config causes Codex startup to fail, proving the Codex config is being read.

Please inspect:
1. The active Codex config.toml file.
2. Whether the `[mcp_servers.dataanalyser_sql]` section is valid TOML.
3. Whether the MCP server should use `dab.exe` directly or `powershell.exe`.
4. Whether VS Code/Codex can resolve the `dab` command.
5. Whether WSL is enabled for Codex.
6. Whether the DAB stdio process emits startup text that breaks MCP stdio initialization.
7. Whether a different command form is required for Codex on Windows.
8. Whether `.vscode/mcp.json` is a better route than Codex config for this IDE session.

Do not modify application code. Stay focused on getting the MCP server connected.
```

---

## First useful prompt once MCP connects

After `dataanalyser_sql` appears connected, ask Codex:

```text
Using the dataanalyser_sql MCP server, inspect the AiHealthMetrics entity.

Stay read-only.

Based only on the exposed fields and sampled records, identify:
1. candidate time axis
2. candidate metric grouping fields
3. candidate metric value field
4. likely dimensions
5. likely data-quality risks
6. useful first chart types for DataAnalyser
7. additional SQL views that would make analysis easier
```

---

## Safety rules for Codex/AI SQL interrogation

Create or update:

```text
C:\Development\DataAnalyser\AGENTS.md
```

Suggested content:

```md
# DataAnalyser AI Instructions

## SQL MCP usage

Use the `dataanalyser_sql` MCP server only for read-only database inspection.

Allowed:
- describe entities
- inspect fields
- sample records
- count records
- identify missingness
- identify candidate metric columns
- identify candidate dimension columns
- identify candidate time columns
- suggest analytical questions supported by the data

Forbidden:
- INSERT
- UPDATE
- DELETE
- DROP
- ALTER
- TRUNCATE
- MERGE
- schema changes
- credential exposure
- destructive operations

## DataAnalyser project direction

DataAnalyser is a domain-agnostic analytical system focused on:
- data ingestion
- canonical metric series construction
- transformation strategies
- visualization
- future AI-assisted analytical reasoning

## Design rule

Do not tightly couple:
- SQL access
- UI code
- transformation logic
- chart rendering
- AI-generated analysis

Findings from SQL interrogation should become explicit artifacts such as:
- MetricCandidate
- DimensionCandidate
- TimeAxisCandidate
- ChartSuggestion
- DataQualityFinding
- InsightFinding
```

---

## Blunt conclusion

The SQL/DAB portion is working.

The current unresolved problem is:

> Codex in VS Code is not successfully launching or connecting to the DAB MCP stdio server, even though the same DAB command runs manually in PowerShell.

The next step is not more SQL work. It is debugging Codex/VS Code MCP process launch and config discovery.
