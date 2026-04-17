# Dependency Summary

Generated: 2026-04-17 12:54:00
Root: C:\Development\POCs\DataAnalyser

This file is auto-generated.
It reflects **declared dependencies only**.
No inference. No semantic interpretation.

------------------------------------------------------

## Project-to-Project Dependencies

| Source Project | Depends On |
|---------------|------------|
| DataFileReader.Tests | DataFileReader |
| DataVisualiser | DataFileReader |
| DataVisualiser.Tests | DataVisualiser |

------------------------------------------------------

## External Package Dependencies

### DataFileReader

- Azure.Data.Tables (12.11.0)
- Microsoft.Data.SqlClient (6.1.2)
- Microsoft.DependencyValidation.Analyzers (0.11.0)
- Newtonsoft.Json (13.0.3)
- OpenAI (2.2.0)
- RestClient (3.1024.23771)
- System.Configuration.ConfigurationManager (10.0.0-preview.1.25080.5)
- System.Data.SqlClient (4.9.0)

### DataFileReader.Tests

- coverlet.collector (6.0.2)
- Microsoft.NET.Test.Sdk (17.12.0)
- xunit (2.9.2)
- xunit.runner.visualstudio (2.8.2)

### DataVisualiser

- Dapper (2.1.66)
- LiveCharts.Core (0.9.8)
- LiveCharts.Wpf.Core (0.9.8)
- LiveChartsCore (2.0.0-rc6.1)
- LiveChartsCore.SkiaSharpView (2.0.0-rc6.1)
- LiveChartsCore.SkiaSharpView.WPF (2.0.0-rc6.1)
- Microsoft.Data.SqlClient (6.1.4)
- Syncfusion.Licensing (32.1.25)
- Syncfusion.SfSunburstChart.WPF (32.1.25)

### DataVisualiser.Tests

- coverlet.collector (6.0.2)
- Microsoft.NET.Test.Sdk (17.12.0)
- Moq (4.20.72)
- xunit (2.9.2)
- xunit.runner.visualstudio (2.8.2)

------------------------------------------------------

## Notes

- Dependencies are derived from `<ProjectReference>` and `<PackageReference>` only.
- Versions may be `(unspecified)` when provided through central package management, imported props/targets, or other MSBuild indirection rather than directly on the project node.
- Absence of a dependency here is authoritative only within the declared project-file scope above.
- This document is **structural**, not architectural.
- Boundary concerns must be evaluated against:
  - SYSTEM_MAP.md
  - Project Bible.md

------------------------------------------------------

End of dependency-summary.md
