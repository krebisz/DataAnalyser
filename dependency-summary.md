# Dependency Summary

Generated: 2025-12-25 09:14:44
Root: .

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

-  ((unspecified))
- Azure.Data.Tables (12.11.0)
- Microsoft.DependencyValidation.Analyzers (0.11.0)
- Newtonsoft.Json (13.0.3)
- OpenAI (2.2.0)
- RestClient (3.1024.23771)
- RestSharp (112.1.1-alpha.0.4)
- System.Configuration.ConfigurationManager (10.0.0-preview.1.25080.5)
- System.Data.SqlClient (4.9.0)
- System.Net.Http (4.3.4)

### DataFileReader.Tests

-  ((unspecified))
-  ((unspecified))
- coverlet.collector (6.0.2)
- Microsoft.NET.Test.Sdk (17.12.0)
- xunit (2.9.2)
- xunit.runner.visualstudio (2.8.2)

### DataVisualiser

-  ((unspecified))
- CSharpMarkup.WinUI.LiveChartsCore.SkiaSharpView (3.1.2)
- Dapper (2.1.66)
- LiveCharts.Core (0.9.8)
- LiveCharts.Wpf.Core (0.9.8)
- Microsoft.Data.SqlClient (6.1.3)
- SkiaSharp (3.119.1)
- System.Configuration.ConfigurationManager (10.0.0)

### DataVisualiser.Tests

-  ((unspecified))
-  ((unspecified))
- coverlet.collector (6.0.2)
- Microsoft.NET.Test.Sdk (17.12.0)
- Moq (4.20.72)
- xunit (2.9.2)
- xunit.runner.visualstudio (2.8.2)

------------------------------------------------------

## Notes

- Dependencies are derived from <ProjectReference> and <PackageReference> only.
- Absence of a dependency here is authoritative.
- This document is **structural**, not architectural.
- Boundary concerns must be evaluated against:
  - SYSTEM_MAP.md
  - Project Bible.md

------------------------------------------------------

End of dependency-summary.md
