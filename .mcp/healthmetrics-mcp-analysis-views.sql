USE [Health];
GO

SET ANSI_NULLS ON;
GO

SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER VIEW [dbo].[vw_AiHealthMetrics]
AS
SELECT
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

CREATE OR ALTER VIEW [dbo].[vw_AiHealthMetricProviders]
AS
SELECT
    CASE
        WHEN [Provider] IS NULL THEN N'<NULL>'
        WHEN [Provider] = N'' THEN N'<EMPTY>'
        ELSE [Provider]
    END AS [ProviderKey],
    [Provider],
    CONVERT(BIGINT, COUNT_BIG(*)) AS [RecordCount]
FROM [dbo].[HealthMetrics]
GROUP BY [Provider];
GO

CREATE OR ALTER VIEW [dbo].[vw_AiHealthMetricTypes]
AS
SELECT
    CASE
        WHEN [MetricType] IS NULL THEN N'<NULL>'
        WHEN [MetricType] = N'' THEN N'<EMPTY>'
        ELSE [MetricType]
    END AS [MetricTypeKey],
    [MetricType],
    CONVERT(BIGINT, COUNT_BIG(*)) AS [RecordCount]
FROM [dbo].[HealthMetrics]
GROUP BY [MetricType];
GO

CREATE OR ALTER VIEW [dbo].[vw_AiHealthMetricSubtypes]
AS
SELECT
    CASE
        WHEN [MetricSubtype] IS NULL THEN N'<NULL>'
        WHEN [MetricSubtype] = N'' THEN N'<EMPTY>'
        ELSE [MetricSubtype]
    END AS [MetricSubtypeKey],
    [MetricSubtype],
    CONVERT(BIGINT, COUNT_BIG(*)) AS [RecordCount]
FROM [dbo].[HealthMetrics]
GROUP BY [MetricSubtype];
GO

CREATE OR ALTER VIEW [dbo].[vw_AiHealthMetricUnits]
AS
SELECT
    CASE
        WHEN [Unit] IS NULL THEN N'<NULL>'
        WHEN [Unit] = N'' THEN N'<EMPTY>'
        ELSE [Unit]
    END AS [UnitKey],
    [Unit],
    CONVERT(BIGINT, COUNT_BIG(*)) AS [RecordCount]
FROM [dbo].[HealthMetrics]
GROUP BY [Unit];
GO

CREATE OR ALTER VIEW [dbo].[vw_AiHealthMetricShapeCounts]
AS
SELECT
    CASE WHEN [Provider] IS NULL THEN N'<NULL>' WHEN [Provider] = N'' THEN N'<EMPTY>' ELSE [Provider] END AS [ProviderKey],
    CASE WHEN [MetricType] IS NULL THEN N'<NULL>' WHEN [MetricType] = N'' THEN N'<EMPTY>' ELSE [MetricType] END AS [MetricTypeKey],
    CASE WHEN [MetricSubtype] IS NULL THEN N'<NULL>' WHEN [MetricSubtype] = N'' THEN N'<EMPTY>' ELSE [MetricSubtype] END AS [MetricSubtypeKey],
    CASE WHEN [Unit] IS NULL THEN N'<NULL>' WHEN [Unit] = N'' THEN N'<EMPTY>' ELSE [Unit] END AS [UnitKey],
    [Provider],
    [MetricType],
    [MetricSubtype],
    [Unit],
    CONVERT(BIGINT, COUNT_BIG(*)) AS [RecordCount]
FROM [dbo].[HealthMetrics]
GROUP BY
    [Provider],
    [MetricType],
    [MetricSubtype],
    [Unit];
GO

CREATE OR ALTER VIEW [dbo].[vw_AiHealthMetricDuplicatePatterns]
AS
SELECT
    CASE WHEN [Provider] IS NULL THEN N'<NULL>' WHEN [Provider] = N'' THEN N'<EMPTY>' ELSE [Provider] END AS [ProviderKey],
    CASE WHEN [MetricType] IS NULL THEN N'<NULL>' WHEN [MetricType] = N'' THEN N'<EMPTY>' ELSE [MetricType] END AS [MetricTypeKey],
    CASE WHEN [MetricSubtype] IS NULL THEN N'<NULL>' WHEN [MetricSubtype] = N'' THEN N'<EMPTY>' ELSE [MetricSubtype] END AS [MetricSubtypeKey],
    CASE WHEN [Unit] IS NULL THEN N'<NULL>' WHEN [Unit] = N'' THEN N'<EMPTY>' ELSE [Unit] END AS [UnitKey],
    CASE
        WHEN [NormalizedTimestamp] IS NULL THEN N'<NULL>'
        ELSE CONVERT(NVARCHAR(33), [NormalizedTimestamp], 126)
    END AS [NormalizedTimestampKey],
    [Provider],
    [MetricType],
    [MetricSubtype],
    [Unit],
    [NormalizedTimestamp],
    CONVERT(BIGINT, COUNT_BIG(*)) AS [DuplicateCount]
FROM [dbo].[HealthMetrics]
GROUP BY
    [Provider],
    [MetricType],
    [MetricSubtype],
    [Unit],
    [NormalizedTimestamp]
HAVING COUNT_BIG(*) > 1;
GO
