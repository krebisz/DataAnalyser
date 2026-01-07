# CMS Test Results - One-Line Summary

## Test Configuration

- **Global CMS:** Disabled (`UseCmsData=False`) for all tests
- **Result:** All strategies used legacy implementations

---

## One-Line Summary Table

| Strategy               | Subtypes | CMS Enabled     | Status               | Notes                                                        |
| ---------------------- | -------- | --------------- | -------------------- | ------------------------------------------------------------ |
| **SingleMetric**       | 1        | No (Global OFF) | ✅ Expected (Legacy) | ✅ Migrated - Would work if global enabled                   |
| **CombinedMetric**     | 2        | No (Global OFF) | ✅ Expected (Legacy) | ✅ Migrated - Would work if global enabled                   |
| **MultiMetric**        | 4        | No (Global OFF) | ✅ Expected (Legacy) | ❌ NOT MIGRATED - Factory TODO, falls back to legacy         |
| **Normalized**         | 2        | No (Global OFF) | ✅ Expected (Legacy) | ❌ NOT MIGRATED - Factory TODO, falls back to legacy         |
| **Normalized**         | 4        | No (Global OFF) | ✅ Expected (Legacy) | ❌ NOT MIGRATED - Factory TODO, falls back to legacy         |
| **Difference**         | 2        | No (Global OFF) | ✅ Expected (Legacy) | ❌ NOT MIGRATED - Factory TODO, falls back to legacy         |
| **Ratio**              | 2        | No (Global OFF) | ✅ Expected (Legacy) | ❌ NOT MIGRATED - Factory TODO, falls back to legacy         |
| **WeeklyDistribution** | 1        | No (Global OFF) | ⚠️ Data Issue        | ✅ Migrated but PrimaryCms=NULL (data not passed to service) |
| **WeeklyDistribution** | 2        | No (Global OFF) | ⚠️ Data Issue        | ✅ Migrated but PrimaryCms=NULL (data not passed to service) |
| **WeeklyDistribution** | 4        | No (Global OFF) | ⚠️ Data Issue        | ✅ Migrated but PrimaryCms=NULL (data not passed to service) |
| **WeekdayTrend**       | 1        | No (Global OFF) | ✅ Expected (Legacy) | ✅ Migrated - Would work if global enabled                   |
| **WeekdayTrend**       | 2        | No (Global OFF) | ✅ Expected (Legacy) | ✅ Migrated - Would work if global enabled                   |
| **WeekdayTrend**       | 4        | No (Global OFF) | ✅ Expected (Legacy) | ✅ Migrated - Would work if global enabled                   |
| **HourlyDistribution** | 1        | No (Global OFF) | ⚠️ Data Issue        | ✅ Migrated but PrimaryCms=NULL (data not passed to service) |
| **HourlyDistribution** | 2        | No (Global OFF) | ⚠️ Data Issue        | ✅ Migrated but PrimaryCms=NULL (data not passed to service) |
| **HourlyDistribution** | 4        | No (Global OFF) | ⚠️ Data Issue        | ✅ Migrated but PrimaryCms=NULL (data not passed to service) |

---

## Legend

- ✅ **Expected (Legacy):** Working correctly, using legacy as expected when CMS disabled
- ✅ **Migrated:** CMS implementation exists and would work if global CMS enabled
- ❌ **NOT MIGRATED:** Factory has TODO, no CMS implementation, falls back to legacy
- ⚠️ **Data Issue:** CMS implementation exists but CMS data not available in context

---

## Quick Summary

**Migrated & Working (3):** SingleMetric, CombinedMetric, WeekdayTrend  
**Migrated but Data Issue (2):** WeeklyDistribution, HourlyDistribution  
**Not Migrated (4):** MultiMetric, Normalized, Difference, Ratio

**Critical Issue:** Global CMS was disabled for all tests, so no CMS strategies were actually tested.
