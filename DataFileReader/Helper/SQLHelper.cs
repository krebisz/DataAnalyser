using System.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using DataFileReader.Class;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#pragma warning disable CS0618
#pragma warning disable CS8600

namespace DataFileReader.Helper;

public static class SQLHelper
{
    public static void CleanDatabase()
    {
        var connectionString = ConfigurationManager.AppSettings["HealthDB"];
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("HealthDB connection string is not configured.");

        var sql = @"
IF OBJECT_ID(N'[dbo].[CanonicalMetricMappings]', N'U') IS NOT NULL DROP TABLE [dbo].[CanonicalMetricMappings];

IF OBJECT_ID(N'[dbo].[HealthMetricsCounts]', N'U') IS NOT NULL DROP TABLE [dbo].[HealthMetricsCounts];

IF OBJECT_ID(N'[dbo].[HealthMetricsSecond]', N'U') IS NOT NULL DROP TABLE [dbo].[HealthMetricsSecond];
IF OBJECT_ID(N'[dbo].[HealthMetricsMinute]', N'U') IS NOT NULL DROP TABLE [dbo].[HealthMetricsMinute];
IF OBJECT_ID(N'[dbo].[HealthMetricsHour]', N'U') IS NOT NULL DROP TABLE [dbo].[HealthMetricsHour];
IF OBJECT_ID(N'[dbo].[HealthMetricsDay]', N'U') IS NOT NULL DROP TABLE [dbo].[HealthMetricsDay];
IF OBJECT_ID(N'[dbo].[HealthMetricsWeek]', N'U') IS NOT NULL DROP TABLE [dbo].[HealthMetricsWeek];
IF OBJECT_ID(N'[dbo].[HealthMetricsMonth]', N'U') IS NOT NULL DROP TABLE [dbo].[HealthMetricsMonth];
IF OBJECT_ID(N'[dbo].[HealthMetricsYear]', N'U') IS NOT NULL DROP TABLE [dbo].[HealthMetricsYear];

IF OBJECT_ID(N'[dbo].[HealthMetrics]', N'U') IS NOT NULL DROP TABLE [dbo].[HealthMetrics];
IF OBJECT_ID(N'[dbo].[HealthMetricsMetaData]', N'U') IS NOT NULL DROP TABLE [dbo].[HealthMetricsMetaData];
";

        using var conn = new SqlConnection(connectionString);
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.CommandTimeout = 300;
        cmd.ExecuteNonQuery();
    }

    private static void EnsureHealthMetricsMetaDataTableExists(string connectionString)
    {
        var sql = @"
        IF OBJECT_ID(N'[dbo].[HealthMetricsMetaData]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[HealthMetricsMetaData](
                [Id] UNIQUEIDENTIFIER NOT NULL
                    CONSTRAINT PK_HealthMetricsMetaData PRIMARY KEY
                    DEFAULT NEWSEQUENTIALID(),
                [MetadataJson] NVARCHAR(MAX) NOT NULL,
                [MetadataHash] VARBINARY(32) NOT NULL,
                [ReferenceValue] NVARCHAR(64) NULL,
                [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE()
            );
        END

        -- Column migration (existing DBs)
        IF COL_LENGTH(N'dbo.HealthMetricsMetaData', N'ReferenceValue') IS NULL
        BEGIN
            ALTER TABLE [dbo].[HealthMetricsMetaData]
            ADD [ReferenceValue] NVARCHAR(64) NULL;
        END

        -- Indexes (use dynamic SQL to avoid compile-time 'invalid column' errors during migrations)
        IF NOT EXISTS (
            SELECT 1 FROM sys.indexes
            WHERE object_id = OBJECT_ID(N'[dbo].[HealthMetricsMetaData]')
              AND name = 'UX_HealthMetricsMetaData_Hash'
        )
        BEGIN
            EXEC(N'CREATE UNIQUE NONCLUSTERED INDEX UX_HealthMetricsMetaData_Hash ON [dbo].[HealthMetricsMetaData](MetadataHash);');
        END

        IF NOT EXISTS (
            SELECT 1 FROM sys.indexes
            WHERE object_id = OBJECT_ID(N'[dbo].[HealthMetricsMetaData]')
              AND name = 'IX_HealthMetricsMetaData_ReferenceValue'
        )
        BEGIN
            EXEC(N'CREATE NONCLUSTERED INDEX IX_HealthMetricsMetaData_ReferenceValue ON [dbo].[HealthMetricsMetaData]([ReferenceValue]) WHERE [ReferenceValue] IS NOT NULL;');
        END
    ";

        using var conn = new SqlConnection(connectionString);
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.ExecuteNonQuery();
    }

    private static void EnsureHealthMetricsCanonicalTableExists(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return;

        try
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();
            EnsureHealthMetricsCanonicalTableExists(connection);
            SeedHealthMetricsCanonicalTableFromCounts(connection);
            SeedHealthMetricsCanonicalTableFromHealthMetrics(connection);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Error ensuring HealthMetricsCanonical table: {ex.Message}");
        }
    }

    private static void EnsureHealthMetricsCanonicalTableExists(SqlConnection connection)
    {
        var createTableQuery = @"
            IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HealthMetricsCanonical]') AND type in (N'U'))
            BEGIN
                CREATE TABLE [dbo].[HealthMetricsCanonical](
                    [MetricType] NVARCHAR(100) NOT NULL,
                    [MetricSubtype] NVARCHAR(200) NOT NULL DEFAULT '',
                    [MetricTypeName] NVARCHAR(100) NOT NULL DEFAULT '',
                    [MetricSubtypeName] NVARCHAR(200) NOT NULL DEFAULT '',
                    [Disabled] BIT NOT NULL DEFAULT 0,
                    [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE(),
                    [LastUpdated] DATETIME2 NOT NULL DEFAULT GETDATE(),
                    CONSTRAINT PK_HealthMetricsCanonical PRIMARY KEY CLUSTERED (MetricType, MetricSubtype)
                );
                CREATE NONCLUSTERED INDEX IX_HealthMetricsCanonical_MetricType
                    ON [dbo].[HealthMetricsCanonical](MetricType);
            END";

        using var command = new SqlCommand(createTableQuery, connection);
        command.ExecuteNonQuery();
    }

    private static void SeedHealthMetricsCanonicalTableFromHealthMetrics(SqlConnection connection)
    {
        var seedSql = @"
            SELECT DISTINCT
                LTRIM(RTRIM(MetricType)) AS MetricType,
                ISNULL(LTRIM(RTRIM(MetricSubtype)), '') AS MetricSubtype
            FROM [dbo].[HealthMetrics]
            WHERE MetricType IS NOT NULL AND LTRIM(RTRIM(MetricType)) <> '';";

        var entries = new List<(string MetricType, string MetricSubtype)>();
        using (var command = new SqlCommand(seedSql, connection))
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                var metricType = reader["MetricType"] as string ?? string.Empty;
                var metricSubtype = reader["MetricSubtype"] as string ?? string.Empty;
                entries.Add((metricType, metricSubtype));
            }
        }

        if (entries.Count > 0)
            EnsureHealthMetricsCanonicalEntries(connection, entries);
    }

    private static void SeedHealthMetricsCanonicalTableFromCounts(SqlConnection connection)
    {
        var hasCountsTable = @"
            IF OBJECT_ID(N'[dbo].[HealthMetricsCounts]', N'U') IS NULL
                SELECT 0
            ELSE
                SELECT 1;";

        using (var checkCommand = new SqlCommand(hasCountsTable, connection))
        {
            var exists = (int)(checkCommand.ExecuteScalar() ?? 0);
            if (exists == 0)
                return;
        }

        var hasColumnsSql = @"
            SELECT CASE
                WHEN COL_LENGTH('dbo.HealthMetricsCounts', 'MetricTypeName') IS NOT NULL
                  AND COL_LENGTH('dbo.HealthMetricsCounts', 'MetricSubtypeName') IS NOT NULL
                  AND COL_LENGTH('dbo.HealthMetricsCounts', 'Disabled') IS NOT NULL
                THEN 1 ELSE 0 END;";

        using (var columnCommand = new SqlCommand(hasColumnsSql, connection))
        {
            var hasColumns = (int)(columnCommand.ExecuteScalar() ?? 0);
            if (hasColumns == 0)
                return;
        }

        var seedSql = @"
            SELECT DISTINCT
                LTRIM(RTRIM(MetricType)) AS MetricType,
                ISNULL(LTRIM(RTRIM(MetricSubtype)), '') AS MetricSubtype,
                ISNULL(MetricTypeName, '') AS MetricTypeName,
                ISNULL(MetricSubtypeName, '') AS MetricSubtypeName,
                ISNULL(Disabled, 0) AS Disabled
            FROM [dbo].[HealthMetricsCounts]
            WHERE MetricType IS NOT NULL AND LTRIM(RTRIM(MetricType)) <> '';";

        var entries = new List<(string MetricType, string MetricSubtype, string MetricTypeName, string MetricSubtypeName, bool Disabled)>();
        using (var command = new SqlCommand(seedSql, connection))
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                var metricType = reader["MetricType"] as string ?? string.Empty;
                var metricSubtype = reader["MetricSubtype"] as string ?? string.Empty;
                var metricTypeName = reader["MetricTypeName"] as string ?? string.Empty;
                var metricSubtypeName = reader["MetricSubtypeName"] as string ?? string.Empty;
                var disabledValue = reader["Disabled"] is bool disabled && disabled;

                if (string.IsNullOrWhiteSpace(metricType))
                    continue;

                var resolvedTypeName = string.IsNullOrWhiteSpace(metricTypeName) ? FormatMetricDisplayName(metricType) : metricTypeName;
                var resolvedSubtypeName = string.IsNullOrWhiteSpace(metricSubtypeName) ? FormatMetricDisplayName(metricSubtype) : metricSubtypeName;

                entries.Add((metricType, metricSubtype, resolvedTypeName, resolvedSubtypeName, disabledValue));
            }
        }

        if (entries.Count > 0)
            EnsureHealthMetricsCanonicalEntries(connection, entries);
    }

    private static string FormatMetricDisplayName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var words = value.Replace('_', ' ').Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (words.Length == 0)
            return string.Empty;

        var formatted = new StringBuilder();
        for (var i = 0; i < words.Length; i++)
        {
            var word = words[i].ToLowerInvariant();
            if (word.Length == 0)
                continue;

            formatted.Append(char.ToUpperInvariant(word[0]));
            if (word.Length > 1)
                formatted.Append(word.Substring(1));

            if (i < words.Length - 1)
                formatted.Append(' ');
        }

        return formatted.ToString();
    }

    /// <summary>
    ///     Creates the standardized HealthMetrics table if it doesn't exist
    /// </summary>
    public static void EnsureHealthMetricsTableExists()
    {
        var connectionString = ConfigurationManager.AppSettings["HealthDB"];
        EnsureHealthMetricsMetaDataTableExists(connectionString);

        var createTableQuery = @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HealthMetrics]') AND type in (N'U'))
            BEGIN
                CREATE TABLE [dbo].[HealthMetrics](
                    [Id] BIGINT IDENTITY(1,1) PRIMARY KEY,
                    [Provider] NVARCHAR(50) NULL,
                    [MetricType] NVARCHAR(100) NULL,
                    [MetricSubtype] NVARCHAR(200) NULL,
                    [SourceFile] NVARCHAR(500) NULL,
                    [NormalizedTimestamp] DATETIME2 NULL,
                    [RawTimestamp] NVARCHAR(100) NULL,
                    [Value] DECIMAL(18,4) NULL,
                    [Unit] NVARCHAR(50) NULL,
                    [MetaDataId] UNIQUEIDENTIFIER NULL,
                    [CreatedDate] DATETIME2 DEFAULT GETDATE()
                )
                
                -- Optimized composite indexes for common query patterns
                -- Index for queries filtering by MetricType and date range (most common pattern)
                CREATE NONCLUSTERED INDEX IX_HealthMetrics_MetricType_Timestamp 
                    ON HealthMetrics(MetricType, NormalizedTimestamp) 
                    INCLUDE (Value, Unit, Provider)
                    WHERE NormalizedTimestamp IS NOT NULL AND Value IS NOT NULL
                
                -- Index for queries filtering by MetricType, MetricSubtype, and date range
                CREATE NONCLUSTERED INDEX IX_HealthMetrics_MetricType_Subtype_Timestamp 
                    ON HealthMetrics(MetricType, MetricSubtype, NormalizedTimestamp) 
                    INCLUDE (Value, Unit, Provider)
                    WHERE NormalizedTimestamp IS NOT NULL AND Value IS NOT NULL

                CREATE NONCLUSTERED INDEX IX_HealthMetrics_MetaDataId
                    ON HealthMetrics(MetaDataId)
                    WHERE MetaDataId IS NOT NULL;

                -- Index for timestamp-only queries (if needed)
                CREATE NONCLUSTERED INDEX IX_HealthMetrics_Timestamp 
                    ON HealthMetrics(NormalizedTimestamp)
                    WHERE NormalizedTimestamp IS NOT NULL
                
                -- Index for SourceFile lookups (for duplicate file detection)
                CREATE NONCLUSTERED INDEX IX_HealthMetrics_SourceFile 
                    ON HealthMetrics(SourceFile)
                    WHERE SourceFile IS NOT NULL
            END";

        try
        {
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                using (var sqlCommand = new SqlCommand(createTableQuery, sqlConnection))
                {
                    sqlCommand.ExecuteNonQuery();
                }
            }

            // Ensure MetricSubtype column exists in existing tables
            if (!string.IsNullOrEmpty(connectionString))
            {
                EnsureMetricSubtypeColumnExists(connectionString);
                // Create smaller-resolution HealthMetrics tables if they don't exist
                EnsureHealthMetricsResolutionTablesExist(connectionString);
                // Optimize indexes for existing tables (will skip if table was just created with optimized indexes)
                OptimizeHealthMetricsIndexes();
                // Ensure the summary counts table exists
                EnsureHealthMetricsCountsTableExists(connectionString);
                EnsureHealthMetricsCanonicalTableExists(connectionString);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating HealthMetrics table: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    ///     Ensures the per-resolution HealthMetrics tables exist (Second, Minute, Hour, Day, Week, Month, Year)
    ///     These tables are lightweight versions of HealthMetrics containing only:
    ///     Id, MetricType, MetricSubtype, NormalizedTimestamp, Value, Unit
    ///     Column names are consistent with the main HealthMetrics table.
    /// </summary>
    private static void EnsureHealthMetricsResolutionTablesExist(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return;

        var tableNames = new[]
        {
                "HealthMetricsSecond",
                "HealthMetricsMinute",
                "HealthMetricsHour",
                "HealthMetricsDay",
                "HealthMetricsWeek",
                "HealthMetricsMonth",
                "HealthMetricsYear"
        };

        var sb = new StringBuilder();

        foreach (var table in tableNames)
            sb.Append($@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{table}]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[{table}](
                        [Id] BIGINT IDENTITY(1,1) PRIMARY KEY,
                        [MetricType] NVARCHAR(100) NULL,
                        [MetricSubtype] NVARCHAR(200) NULL,
                        [NormalizedTimestamp] DATETIME2 NULL,
                        [Value] DECIMAL(18,4) NULL,
                        [Unit] NVARCHAR(50) NULL,
                        [CreatedDate] DATETIME2 DEFAULT GETDATE()
                    );

                    -- Useful index for MetricType + timestamp queries
                    CREATE NONCLUSTERED INDEX IX_{table}_MetricType_Timestamp
                        ON [dbo].[{table}](MetricType, NormalizedTimestamp)
                        INCLUDE (Value, Unit)
                        WHERE NormalizedTimestamp IS NOT NULL AND Value IS NOT NULL;

                    -- Index on timestamp only for time-range queries
                    CREATE NONCLUSTERED INDEX IX_{table}_Timestamp
                        ON [dbo].[{table}](NormalizedTimestamp)
                        WHERE NormalizedTimestamp IS NOT NULL;

                    -- Add computed column for normalized MetricSubtype (NULL -> empty string)
                    -- This allows us to create a unique constraint that treats NULL and empty string as the same
                    ALTER TABLE [dbo].[{table}] ADD MetricSubtypeNormalized AS ISNULL(MetricSubtype, '') PERSISTED;

                    -- Unique constraint to prevent duplicate aggregations
                    -- This ensures one record per (MetricType, MetricSubtype, NormalizedTimestamp) combination
                    CREATE UNIQUE NONCLUSTERED INDEX UQ_{table}_MetricType_Subtype_Timestamp
                        ON [dbo].[{table}](MetricType, MetricSubtypeNormalized, NormalizedTimestamp)
                        WHERE NormalizedTimestamp IS NOT NULL;
                END
                ELSE
                BEGIN
                    -- For existing tables, add CreatedDate column if it doesn't exist
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'CreatedDate' AND object_id = OBJECT_ID(N'[dbo].[{table}]'))
                    BEGIN
                        ALTER TABLE [dbo].[{table}] ADD [CreatedDate] DATETIME2 DEFAULT GETDATE();
                    END

                    -- For existing tables, add computed column and unique constraint if they don't exist
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'MetricSubtypeNormalized' AND object_id = OBJECT_ID(N'[dbo].[{table}]'))
                    BEGIN
                        ALTER TABLE [dbo].[{table}] ADD MetricSubtypeNormalized AS ISNULL(MetricSubtype, '') PERSISTED;
                    END

                    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_{table}_MetricType_Subtype_Timestamp' AND object_id = OBJECT_ID(N'[dbo].[{table}]'))
                    BEGIN
                        CREATE UNIQUE NONCLUSTERED INDEX UQ_{table}_MetricType_Subtype_Timestamp
                            ON [dbo].[{table}](MetricType, MetricSubtypeNormalized, NormalizedTimestamp)
                            WHERE NormalizedTimestamp IS NOT NULL;
                    END
                END
            ");

        try
        {
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                using (var sqlCommand = new SqlCommand(sb.ToString(), sqlConnection))
                {
                    sqlCommand.CommandTimeout = 120;
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            // Log but do not throw - these helper tables should not block main operations
            Console.WriteLine($"Warning: Error creating resolution HealthMetrics tables: {ex.Message}");
        }
    }

    /// <summary>
    ///     Optimizes existing HealthMetrics table by replacing old indexes with optimized composite indexes
    ///     This should be called after table creation or when optimizing an existing large table
    /// </summary>
    public static void OptimizeHealthMetricsIndexes()
    {
        var connectionString = ConfigurationManager.AppSettings["HealthDB"];
        if (string.IsNullOrEmpty(connectionString))
            return;

        var optimizeIndexesQuery = @"
            -- Check if table exists
            IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HealthMetrics]') AND type in (N'U'))
            BEGIN
                -- Drop old inefficient indexes if they exist
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_HealthMetrics_Provider_Metric' AND object_id = OBJECT_ID(N'[dbo].[HealthMetrics]'))
                    DROP INDEX IX_HealthMetrics_Provider_Metric ON [dbo].[HealthMetrics];
                
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_HealthMetrics_MetricSubtype' AND object_id = OBJECT_ID(N'[dbo].[HealthMetrics]'))
                    DROP INDEX IX_HealthMetrics_MetricSubtype ON [dbo].[HealthMetrics];
                
                -- Drop old timestamp index if it doesn't have the filtered WHERE clause
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_HealthMetrics_Timestamp' AND object_id = OBJECT_ID(N'[dbo].[HealthMetrics]'))
                BEGIN
                    -- Check if it's the old unfiltered version (has_filter = 0 means no filter)
                    IF EXISTS (
                        SELECT * FROM sys.indexes 
                        WHERE name = 'IX_HealthMetrics_Timestamp' 
                        AND object_id = OBJECT_ID(N'[dbo].[HealthMetrics]')
                        AND has_filter = 0
                    )
                    BEGIN
                        DROP INDEX IX_HealthMetrics_Timestamp ON [dbo].[HealthMetrics];
                    END
                END
                
                -- Create optimized composite index for MetricType + NormalizedTimestamp queries (if it doesn't exist)
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_HealthMetrics_MetricType_Timestamp' AND object_id = OBJECT_ID(N'[dbo].[HealthMetrics]'))
                BEGIN
                    CREATE NONCLUSTERED INDEX IX_HealthMetrics_MetricType_Timestamp 
                        ON [dbo].[HealthMetrics](MetricType, NormalizedTimestamp) 
                        INCLUDE (Value, Unit, Provider)
                        WHERE NormalizedTimestamp IS NOT NULL AND Value IS NOT NULL;
                END
                
                -- Create optimized composite index for MetricType + MetricSubtype + NormalizedTimestamp queries (if it doesn't exist)
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_HealthMetrics_MetricType_Subtype_Timestamp' AND object_id = OBJECT_ID(N'[dbo].[HealthMetrics]'))
                BEGIN
                    CREATE NONCLUSTERED INDEX IX_HealthMetrics_MetricType_Subtype_Timestamp 
                        ON [dbo].[HealthMetrics](MetricType, MetricSubtype, NormalizedTimestamp) 
                        INCLUDE (Value, Unit, Provider)
                        WHERE NormalizedTimestamp IS NOT NULL AND Value IS NOT NULL;
                END
                
                -- Create filtered timestamp index (if it doesn't exist)
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_HealthMetrics_Timestamp' AND object_id = OBJECT_ID(N'[dbo].[HealthMetrics]'))
                BEGIN
                    CREATE NONCLUSTERED INDEX IX_HealthMetrics_Timestamp 
                        ON [dbo].[HealthMetrics](NormalizedTimestamp)
                        WHERE NormalizedTimestamp IS NOT NULL;
                END
                
                -- Ensure SourceFile index exists (if it doesn't exist)
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_HealthMetrics_SourceFile' AND object_id = OBJECT_ID(N'[dbo].[HealthMetrics]'))
                BEGIN
                    CREATE NONCLUSTERED INDEX IX_HealthMetrics_SourceFile 
                        ON [dbo].[HealthMetrics](SourceFile)
                        WHERE SourceFile IS NOT NULL;
                END
            END";

        try
        {
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                using (var sqlCommand = new SqlCommand(optimizeIndexesQuery, sqlConnection))
                {
                    sqlCommand.CommandTimeout = 300; // 5 minutes timeout for large tables
                    sqlCommand.ExecuteNonQuery();
                    Console.WriteLine("HealthMetrics table indexes optimized successfully.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error optimizing HealthMetrics indexes: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    ///     Creates the HealthMetricsCounts summary table if it doesn't exist
    ///     This table stores the count of records for each MetricType/MetricSubtype combination
    /// </summary>
    private static void EnsureHealthMetricsCountsTableExists(string connectionString)
    {
        try
        {
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                var createTableQuery = @"
                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HealthMetricsCounts]') AND type in (N'U'))
                    BEGIN
                        CREATE TABLE [dbo].[HealthMetricsCounts](
                            [MetricType] NVARCHAR(100) NOT NULL,
                            [MetricSubtype] NVARCHAR(200) NOT NULL DEFAULT '',
                            [RecordCount] BIGINT NOT NULL DEFAULT 0,
                            [EarliestDateTime] DATETIME2 NULL,
                            [MostRecentDateTime] DATETIME2 NULL,
                            [DaysBetween] AS CAST(DATEDIFF(day, [EarliestDateTime], [MostRecentDateTime]) AS DECIMAL(18,2)) PERSISTED,
                            [DaysPerRecord] AS CAST(CAST(DATEDIFF(day, [EarliestDateTime], [MostRecentDateTime]) AS DECIMAL(18,5)) / NULLIF([RecordCount], 0) AS DECIMAL(18,5)) PERSISTED,
                            [CreatedDate] DATETIME2 DEFAULT GETDATE(),
                            [LastUpdated] DATETIME2 NOT NULL DEFAULT GETDATE(),
                            CONSTRAINT PK_HealthMetricsCounts PRIMARY KEY CLUSTERED (MetricType, MetricSubtype)
                        )
                        
                        -- Index for fast lookups
                        CREATE NONCLUSTERED INDEX IX_HealthMetricsCounts_RecordCount 
                            ON [dbo].[HealthMetricsCounts](RecordCount DESC)
                    END
                    ELSE
                    BEGIN
                        -- Add new columns if they don't exist (migration for existing tables)
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HealthMetricsCounts]') AND name = 'CreatedDate')
                        BEGIN
                            ALTER TABLE [dbo].[HealthMetricsCounts]
                            ADD [CreatedDate] DATETIME2 DEFAULT GETDATE();
                        END
                        
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HealthMetricsCounts]') AND name = 'EarliestDateTime')
                        BEGIN
                            ALTER TABLE [dbo].[HealthMetricsCounts]
                            ADD [EarliestDateTime] DATETIME2 NULL;
                        END
                        
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HealthMetricsCounts]') AND name = 'MostRecentDateTime')
                        BEGIN
                            ALTER TABLE [dbo].[HealthMetricsCounts]
                            ADD [MostRecentDateTime] DATETIME2 NULL;
                        END

                        -- Add computed columns if they don't exist
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HealthMetricsCounts]') AND name = 'DaysBetween')
                        BEGIN
                            ALTER TABLE [dbo].[HealthMetricsCounts]
                            ADD [DaysBetween] AS CAST(DATEDIFF(day, [EarliestDateTime], [MostRecentDateTime]) AS DECIMAL(18,2)) PERSISTED;
                        END
                        
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HealthMetricsCounts]') AND name = 'DaysPerRecord')
                        BEGIN
                            ALTER TABLE [dbo].[HealthMetricsCounts]
                            ADD [DaysPerRecord] AS CAST(CAST(DATEDIFF(day, [EarliestDateTime], [MostRecentDateTime]) AS DECIMAL(18,5)) / NULLIF([RecordCount], 0) AS DECIMAL(18,5)) PERSISTED;
                        END
                    END";

                using (var sqlCommand = new SqlCommand(createTableQuery, sqlConnection))
                {
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating HealthMetricsCounts table: {ex.Message}");
            // Don't throw - this is a helper table, shouldn't block main operations
        }
    }

    /// <summary>
    ///     Initializes the HealthMetricsCounts table by calculating counts from existing HealthMetrics data
    ///     This should be called once to populate the summary table from existing data
    /// </summary>
    public static void InitializeHealthMetricsCounts()
    {
        var connectionString = ConfigurationManager.AppSettings["HealthDB"];
        if (string.IsNullOrEmpty(connectionString))
            return;

        try
        {
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                var initializeQuery = @"
                    -- Ensure table exists first
                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HealthMetricsCounts]') AND type in (N'U'))
                    BEGIN
                        CREATE TABLE [dbo].[HealthMetricsCounts](
                            [MetricType] NVARCHAR(100) NOT NULL,
                            [MetricSubtype] NVARCHAR(200) NOT NULL DEFAULT '',
                            [RecordCount] BIGINT NOT NULL DEFAULT 0,
                            [EarliestDateTime] DATETIME2 NULL,
                            [MostRecentDateTime] DATETIME2 NULL,
                            [DaysBetween] AS CAST(DATEDIFF(day, [EarliestDateTime], [MostRecentDateTime]) AS DECIMAL(18,2)) PERSISTED,
                            [DaysPerRecord] AS CAST(CAST(DATEDIFF(day, [EarliestDateTime], [MostRecentDateTime]) AS DECIMAL(18,5)) / NULLIF([RecordCount], 0) AS DECIMAL(18,5)) PERSISTED,
                            [CreatedDate] DATETIME2 DEFAULT GETDATE(),
                            [LastUpdated] DATETIME2 NOT NULL DEFAULT GETDATE(),
                            CONSTRAINT PK_HealthMetricsCounts PRIMARY KEY CLUSTERED (MetricType, MetricSubtype)
                        )
                        
                        CREATE NONCLUSTERED INDEX IX_HealthMetricsCounts_RecordCount 
                            ON [dbo].[HealthMetricsCounts](RecordCount DESC)
                    END
                    ELSE
                    BEGIN
                        -- Add new columns if they don't exist (migration for existing tables)
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HealthMetricsCounts]') AND name = 'EarliestDateTime')
                        BEGIN
                            ALTER TABLE [dbo].[HealthMetricsCounts]
                            ADD [EarliestDateTime] DATETIME2 NULL;
                        END
                        
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HealthMetricsCounts]') AND name = 'MostRecentDateTime')
                        BEGIN
                            ALTER TABLE [dbo].[HealthMetricsCounts]
                            ADD [MostRecentDateTime] DATETIME2 NULL;
                        END

                        -- Add computed columns if they don't exist
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HealthMetricsCounts]') AND name = 'DaysBetween')
                        BEGIN
                            ALTER TABLE [dbo].[HealthMetricsCounts]
                            ADD [DaysBetween] AS CAST(DATEDIFF(day, [EarliestDateTime], [MostRecentDateTime]) AS DECIMAL(18,2)) PERSISTED;
                        END
                        
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HealthMetricsCounts]') AND name = 'DaysPerRecord')
                        BEGIN
                            ALTER TABLE [dbo].[HealthMetricsCounts]
                            ADD [DaysPerRecord] AS CAST(CAST(DATEDIFF(day, [EarliestDateTime], [MostRecentDateTime]) AS DECIMAL(18,5)) / NULLIF([RecordCount], 0) AS DECIMAL(18,5)) PERSISTED;
                        END
                    END

                    ;WITH SourceCounts AS (
                        SELECT
                            ISNULL(MetricType, '') AS MetricType,
                            ISNULL(MetricSubtype, '') AS MetricSubtype,
                            COUNT(*) AS RecordCount,
                            MIN(NormalizedTimestamp) AS EarliestDateTime,
                            MAX(NormalizedTimestamp) AS MostRecentDateTime
                        FROM [dbo].[HealthMetrics]
                        WHERE MetricType IS NOT NULL
                          AND (NormalizedTimestamp IS NOT NULL
                               OR (RawTimestamp IS NOT NULL AND RawTimestamp != ''))
                        GROUP BY MetricType, MetricSubtype
                    )
                    MERGE [dbo].[HealthMetricsCounts] AS target
                    USING SourceCounts AS source
                    ON target.MetricType = source.MetricType
                       AND target.MetricSubtype = source.MetricSubtype
                    WHEN MATCHED THEN
                        UPDATE SET
                            RecordCount = source.RecordCount,
                            EarliestDateTime = source.EarliestDateTime,
                            MostRecentDateTime = source.MostRecentDateTime,
                            LastUpdated = GETDATE()
                    WHEN NOT MATCHED THEN
                        INSERT (MetricType, MetricSubtype, RecordCount, EarliestDateTime, MostRecentDateTime, CreatedDate, LastUpdated)
                        VALUES (source.MetricType, source.MetricSubtype, source.RecordCount, source.EarliestDateTime, source.MostRecentDateTime, GETDATE(), GETDATE())
                    WHEN NOT MATCHED BY SOURCE THEN
                        UPDATE SET
                            RecordCount = 0,
                            EarliestDateTime = NULL,
                            MostRecentDateTime = NULL,
                            LastUpdated = GETDATE();
                    ";

                using (var sqlCommand = new SqlCommand(initializeQuery, sqlConnection))
                {
                    sqlCommand.CommandTimeout = 300; // 5 minutes timeout for large tables
                    sqlCommand.ExecuteNonQuery();
                    Console.WriteLine("HealthMetricsCounts table initialized successfully.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing HealthMetricsCounts: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    ///     Adds MetricSubtype column to existing HealthMetrics table if it doesn't exist
    /// </summary>
    private static void EnsureMetricSubtypeColumnExists(string connectionString)
    {
        try
        {
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                // Check if column exists
                var checkColumnQuery = @"
                    IF NOT EXISTS (
                        SELECT * FROM sys.columns 
                        WHERE object_id = OBJECT_ID(N'[dbo].[HealthMetrics]') 
                        AND name = 'MetricSubtype'
                    )
                    BEGIN
                        ALTER TABLE [dbo].[HealthMetrics]
                        ADD [MetricSubtype] NVARCHAR(200) NULL;
                        
                        CREATE INDEX IX_HealthMetrics_MetricSubtype ON HealthMetrics(MetricSubtype);
                    END";

                using (var sqlCommand = new SqlCommand(checkColumnQuery, sqlConnection))
                {
                    sqlCommand.ExecuteNonQuery();
                }

                // More comprehensive update using MetricTypeParser logic
                // This will parse all non-alphanumeric delimiters and update existing records
                UpdateExistingMetricSubtypes(connectionString);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error ensuring MetricSubtype column exists: {ex.Message}");
            // Don't throw - this is a migration step that might fail if column already exists
        }
    }

    /// <summary>
    ///     Updates existing records to populate MetricSubtype by parsing MetricType
    /// </summary>
    private static void UpdateExistingMetricSubtypes(string connectionString)
    {
        try
        {
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                // Get all distinct MetricTypes that need updating
                // We need to update records where MetricType contains a subtype
                // This includes records where MetricSubtype is not set, or where MetricType still contains the subtype suffix
                var getMetricTypesQuery = @"
                    SELECT DISTINCT [MetricType], [MetricSubtype]
                    FROM [dbo].[HealthMetrics]
                    WHERE [MetricType] IS NOT NULL";

                var recordsToUpdate = new List<(string OriginalMetricType, string BaseType, string Subtype)>();
                using (var sqlCommand = new SqlCommand(getMetricTypesQuery, sqlConnection))
                {
                    using (var reader = sqlCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var metricType = reader["MetricType"]?.ToString();
                            var existingSubtype = reader["MetricSubtype"]?.ToString();

                            if (!string.IsNullOrEmpty(metricType))
                            {
                                var parsedSubtype = MetricTypeParser.GetSubtypeString(metricType);
                                var baseType = MetricTypeParser.GetBaseType(metricType);

                                // If MetricType contains a subtype (baseType != metricType), we need to update
                                if (!string.IsNullOrEmpty(parsedSubtype) && baseType != metricType)
                                {
                                    // Use existing subtype if it's already set, otherwise use parsed subtype
                                    var subtypeToUse = !string.IsNullOrEmpty(existingSubtype) ? existingSubtype : parsedSubtype;
                                    recordsToUpdate.Add((metricType, baseType, subtypeToUse));
                                }
                            }
                        }
                    }
                }

                // Update each record: set MetricSubtype (if not already set) and update MetricType to be just the base type
                foreach (var record in recordsToUpdate)
                {
                    var updateQuery = @"
                        UPDATE [dbo].[HealthMetrics]
                        SET [MetricType] = @BaseType,
                            [MetricSubtype] = CASE 
                                WHEN ([MetricSubtype] IS NULL OR [MetricSubtype] = '') THEN @Subtype
                                ELSE [MetricSubtype]
                            END
                        WHERE [MetricType] = @OriginalMetricType";

                    using (var updateCommand = new SqlCommand(updateQuery, sqlConnection))
                    {
                        updateCommand.Parameters.AddWithValue("@Subtype", record.Subtype);
                        updateCommand.Parameters.AddWithValue("@BaseType", record.BaseType);
                        updateCommand.Parameters.AddWithValue("@OriginalMetricType", record.OriginalMetricType);
                        updateCommand.ExecuteNonQuery();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating existing MetricSubtypes: {ex.Message}");
            // Don't throw - this is a migration step
        }
    }

    private static Guid? GetOrCreateMetaDataId(SqlConnection connection, string metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
            return null;

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(metadataJson));
        var referenceValue = ComputeStructuralReferenceValue(metadataJson);

        var sql = @"
        DECLARE @Id UNIQUEIDENTIFIER;

        SELECT @Id = Id
        FROM HealthMetricsMetaData
        WHERE MetadataHash = @Hash;

        IF @Id IS NULL
        BEGIN
            INSERT INTO HealthMetricsMetaData (MetadataJson, MetadataHash, ReferenceValue)
            OUTPUT inserted.Id
            VALUES (@Json, @Hash, @ReferenceValue);
        END
        ELSE
        BEGIN
            IF @ReferenceValue IS NOT NULL
            BEGIN
                UPDATE HealthMetricsMetaData
                SET ReferenceValue = @ReferenceValue
                WHERE Id = @Id AND ReferenceValue IS NULL;
            END

            SELECT @Id;
        END
    ";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@Json", metadataJson);
        cmd.Parameters.AddWithValue("@Hash", hash);
        cmd.Parameters.AddWithValue("@ReferenceValue", (object?)referenceValue ?? DBNull.Value);

        return (Guid?)cmd.ExecuteScalar();
    }

    private static string? ComputeStructuralReferenceValue(string metadataJson)
    {
        try
        {
            var token = JToken.Parse(metadataJson);
            var hierarchyObjectList = new HierarchyObjectList();
            JsoonHelper.CreateHierarchyObjectList(ref hierarchyObjectList, token);

            HierarchyRefValCalculator.AssignStructuralReferenceValues(hierarchyObjectList.HierarchyObjects);

            var root = hierarchyObjectList.HierarchyObjects.FirstOrDefault(x => x.ParentID is null && x.Path == "Root") ?? hierarchyObjectList.HierarchyObjects.OrderBy(x => x.Level ?? 0).FirstOrDefault();

            return root?.ReferenceValue;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     Gets distinct SourceFile values from HealthMetrics table (files that have already been processed)
    /// </summary>
    public static HashSet<string> GetProcessedFiles()
    {
        var connectionString = ConfigurationManager.AppSettings["HealthDB"];
        var processedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                var sql = "SELECT DISTINCT SourceFile FROM HealthMetrics WHERE SourceFile IS NOT NULL AND SourceFile != ''";

                using (var sqlCommand = new SqlCommand(sql, sqlConnection))
                {
                    using (var reader = sqlCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var sourceFile = reader["SourceFile"]?.ToString();
                            if (!string.IsNullOrEmpty(sourceFile))
                                processedFiles.Add(sourceFile);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // If table doesn't exist yet, return empty set (no files processed yet)
            Console.WriteLine($"Note: Could not retrieve processed files list: {ex.Message}");
        }

        return processedFiles;
    }

    /// <summary>
    ///     Marks a file as processed by inserting a minimal record (used for empty files)
    /// </summary>
    public static void MarkFileAsProcessed(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return;

        var connectionString = ConfigurationManager.AppSettings["HealthDB"];

        try
        {
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                // Check if file is already marked as processed
                var checkQuery = "SELECT COUNT(*) FROM HealthMetrics WHERE SourceFile = @SourceFile";
                using (var checkCommand = new SqlCommand(checkQuery, sqlConnection))
                {
                    checkCommand.Parameters.AddWithValue("@SourceFile", filePath);
                    var count = (int)checkCommand.ExecuteScalar();
                    if (count > 0)
                            // File already marked as processed
                        return;
                }

                // Insert minimal record to mark file as processed
                var insertQuery = @"
                    INSERT INTO HealthMetrics 
                    (Provider, MetricType, MetricSubtype, SourceFile, NormalizedTimestamp, RawTimestamp, Value, Unit, MetaDataId)
                    VALUES 
                    (@Provider, @MetricType, @MetricSubtype, @SourceFile, @NormalizedTimestamp, @RawTimestamp, @Value, @Unit, @MetaDataId)";

                using (var sqlCommand = new SqlCommand(insertQuery, sqlConnection))
                {
                    sqlCommand.Parameters.AddWithValue("@Provider", DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@MetricType", DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@MetricSubtype", DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@SourceFile", filePath);
                    sqlCommand.Parameters.AddWithValue("@NormalizedTimestamp", DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@RawTimestamp", DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@Value", DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@Unit", DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@MetaDataId", DBNull.Value);

                    sqlCommand.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error marking file as processed: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    ///     Inserts health metrics into the database using bulk insert for performance
    ///     Also updates the HealthMetricsCounts summary table incrementally
    /// </summary>
    public static void InsertHealthMetrics(List<HealthMetric> metrics)
    {
        if (metrics == null || metrics.Count == 0)
            return;

        var connectionString = ConfigurationManager.AppSettings["HealthDB"];

        try
        {
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                // Use parameterized query for safety and performance
                var insertQuery = @"
                    INSERT INTO HealthMetrics
                        (Provider, MetricType, MetricSubtype, SourceFile,NormalizedTimestamp, RawTimestamp, Value, Unit, MetaDataId)
                    VALUES
                        (@Provider, @MetricType, @MetricSubtype, @SourceFile, @NormalizedTimestamp, @RawTimestamp, @Value, @Unit, @MetaDataId)";

                // Track MetricType/Subtype combinations for summary table update
                // Store count, min timestamp, and max timestamp for each combination
                var typeSubtypeData = new Dictionary<(string MetricType, string MetricSubtype), (int Count, DateTime? MinTimestamp, DateTime? MaxTimestamp)>();
                var canonicalEntries = new List<(string MetricType, string MetricSubtype)>();
                var canonicalKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                const string canonicalKeySeparator = "\u001F";

                void TrackCanonicalEntry(string metricTypeKey, string metricSubtypeKey)
                {
                    if (string.IsNullOrEmpty(metricTypeKey))
                        return;

                    var normalizedSubtype = metricSubtypeKey ?? string.Empty;
                    var canonicalKey = $"{metricTypeKey}{canonicalKeySeparator}{normalizedSubtype}";
                    if (!canonicalKeys.Add(canonicalKey))
                        return;

                    canonicalEntries.Add((metricTypeKey, normalizedSubtype));
                }

                foreach (var metric in metrics)
                {
                    // Parse MetricType to extract base type and subtype
                    var trimmedMetricType = (metric.MetricType ?? string.Empty).Trim();
                    var baseType = trimmedMetricType;
                    var subtype = (metric.MetricSubtype ?? string.Empty).Trim();

                    if (!string.IsNullOrEmpty(trimmedMetricType))
                    {
                        // If subtype not already set, parse it from MetricType
                        if (string.IsNullOrEmpty(subtype))
                            subtype = MetricTypeParser.GetSubtypeString(trimmedMetricType)?.Trim() ?? string.Empty;

                        // If a subtype exists, update MetricType to be just the base type
                        if (!string.IsNullOrEmpty(subtype))
                            baseType = MetricTypeParser.GetBaseType(trimmedMetricType)?.Trim() ?? string.Empty;
                    }

                    if (!string.IsNullOrEmpty(baseType))
                    {
                        var subtypeKey = string.IsNullOrEmpty(subtype) ? string.Empty : subtype;
                        TrackCanonicalEntry(baseType, subtypeKey);

                        var hasValidTimestamp = metric.NormalizedTimestamp.HasValue || !string.IsNullOrEmpty(metric.RawTimestamp);

                        if (hasValidTimestamp && metric.NormalizedTimestamp.HasValue)
                        {
                            var key = (MetricType: baseType, MetricSubtype: subtypeKey);
                            var timestamp = metric.NormalizedTimestamp.Value;

                            if (!typeSubtypeData.ContainsKey(key))
                                typeSubtypeData[key] = (Count: 0, MinTimestamp: timestamp, MaxTimestamp: timestamp);

                            var current = typeSubtypeData[key];
                            typeSubtypeData[key] = (Count: current.Count + 1, MinTimestamp: timestamp < current.MinTimestamp ? timestamp : current.MinTimestamp, MaxTimestamp: timestamp > current.MaxTimestamp ? timestamp : current.MaxTimestamp);
                        }
                        else if (hasValidTimestamp)
                        {
                            var key = (MetricType: baseType, MetricSubtype: subtypeKey);
                            if (!typeSubtypeData.ContainsKey(key))
                                typeSubtypeData[key] = (Count: 0, MinTimestamp: null, MaxTimestamp: null);
                            var current = typeSubtypeData[key];
                            typeSubtypeData[key] = (Count: current.Count + 1, current.MinTimestamp, current.MaxTimestamp);
                        }
                    }

                    using (var sqlCommand = new SqlCommand(insertQuery, sqlConnection))
                    {
                        sqlCommand.Parameters.AddWithValue("@Provider", (object)metric.Provider ?? DBNull.Value);
                        sqlCommand.Parameters.AddWithValue("@MetricType", string.IsNullOrEmpty(baseType) ? DBNull.Value : baseType);
                        sqlCommand.Parameters.AddWithValue("@MetricSubtype", string.IsNullOrEmpty(subtype) ? DBNull.Value : subtype);
                        sqlCommand.Parameters.AddWithValue("@SourceFile", (object)metric.SourceFile ?? DBNull.Value);
                        sqlCommand.Parameters.AddWithValue("@NormalizedTimestamp", (object)metric.NormalizedTimestamp ?? DBNull.Value);
                        sqlCommand.Parameters.AddWithValue("@RawTimestamp", (object)metric.RawTimestamp ?? DBNull.Value);
                        sqlCommand.Parameters.AddWithValue("@Value", (object)metric.Value ?? DBNull.Value);
                        sqlCommand.Parameters.AddWithValue("@Unit", (object)metric.Unit ?? DBNull.Value);

                        // Serialize additional fields to JSON
                        var metadataJson = metric.AdditionalFields.Count > 0 ? JsonConvert.SerializeObject(metric.AdditionalFields) : null;

                        var metaDataId = metadataJson != null ? GetOrCreateMetaDataId(sqlConnection, metadataJson) : null;

                        sqlCommand.Parameters.AddWithValue("@MetaDataId", (object?)metaDataId ?? DBNull.Value);


                        sqlCommand.ExecuteNonQuery();
                    }
                }

                // Persists canonical metadata for new metric combinations
                if (canonicalEntries.Count > 0)
                    EnsureHealthMetricsCanonicalEntries(sqlConnection, canonicalEntries);

                // Update the summary counts table for all MetricType/Subtype combinations in this batch
                if (typeSubtypeData.Count > 0)
                    UpdateHealthMetricsCounts(sqlConnection, typeSubtypeData);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error inserting health metrics: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    ///     Updates the HealthMetricsCounts summary table with new record counts and timestamp ranges
    ///     Uses MERGE to efficiently handle inserts and updates in a single operation
    /// </summary>
    private static void UpdateHealthMetricsCounts(SqlConnection connection, Dictionary<(string MetricType, string MetricSubtype), (int Count, DateTime? MinTimestamp, DateTime? MaxTimestamp)> typeSubtypeData)
    {
        try
        {
            // Ensure the table exists before trying to update it
            EnsureHealthMetricsCountsTableExists(connection);

            // Use MERGE to efficiently update or insert counts and timestamps
            var mergeQuery = @"
                MERGE [dbo].[HealthMetricsCounts] AS target
                USING (VALUES {0}) AS source (MetricType, MetricSubtype, IncrementCount, MinTimestamp, MaxTimestamp)
                ON target.MetricType = source.MetricType 
                   AND target.MetricSubtype = source.MetricSubtype
                WHEN MATCHED THEN
                    UPDATE SET 
                        RecordCount = target.RecordCount + source.IncrementCount,
                        EarliestDateTime = CASE 
                            WHEN source.MinTimestamp IS NOT NULL AND (target.EarliestDateTime IS NULL OR source.MinTimestamp < target.EarliestDateTime)
                            THEN source.MinTimestamp
                            ELSE target.EarliestDateTime
                        END,
                        MostRecentDateTime = CASE
                            WHEN source.MaxTimestamp IS NOT NULL AND (target.MostRecentDateTime IS NULL OR source.MaxTimestamp > target.MostRecentDateTime)
                            THEN source.MaxTimestamp
                            ELSE target.MostRecentDateTime
                        END,
                        LastUpdated = GETDATE()
                WHEN NOT MATCHED THEN
                    INSERT (MetricType, MetricSubtype, RecordCount, EarliestDateTime, MostRecentDateTime, CreatedDate, LastUpdated)
                    VALUES (source.MetricType, source.MetricSubtype, source.IncrementCount, source.MinTimestamp, source.MaxTimestamp, GETDATE(), GETDATE());";

            // Build the VALUES clause for all combinations
            var valuesList = new List<string>();
            var parameters = new List<SqlParameter>();
            var paramIndex = 0;

            foreach (var kvp in typeSubtypeData)
            {
                var metricTypeParam = $"@MetricType{paramIndex}";
                var metricSubtypeParam = $"@MetricSubtype{paramIndex}";
                var countParam = $"@Count{paramIndex}";
                var minTimestampParam = $"@MinTimestamp{paramIndex}";
                var maxTimestampParam = $"@MaxTimestamp{paramIndex}";

                valuesList.Add($"({metricTypeParam}, {metricSubtypeParam}, {countParam}, {minTimestampParam}, {maxTimestampParam})");

                parameters.Add(new SqlParameter(metricTypeParam, kvp.Key.MetricType));
                parameters.Add(new SqlParameter(metricSubtypeParam, kvp.Key.MetricSubtype ?? string.Empty));
                parameters.Add(new SqlParameter(countParam, kvp.Value.Count));
                parameters.Add(new SqlParameter(minTimestampParam, (object)kvp.Value.MinTimestamp ?? DBNull.Value));
                parameters.Add(new SqlParameter(maxTimestampParam, (object)kvp.Value.MaxTimestamp ?? DBNull.Value));

                paramIndex++;
            }

            var finalQuery = string.Format(mergeQuery, string.Join(", ", valuesList));

            using (var sqlCommand = new SqlCommand(finalQuery, connection))
            {
                sqlCommand.Parameters.AddRange(parameters.ToArray());
                sqlCommand.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            // Log but don't throw - summary table updates shouldn't block main inserts
            Console.WriteLine($"Warning: Error updating HealthMetricsCounts: {ex.Message}");
        }
    }

    /// <summary>
    ///     Creates the HealthMetricsCounts summary table if it doesn't exist (overload that uses existing connection)
    /// </summary>
    private static void EnsureHealthMetricsCountsTableExists(SqlConnection connection)
    {
        try
        {
            var createTableQuery = @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HealthMetricsCounts]') AND type in (N'U'))
                BEGIN
                        CREATE TABLE [dbo].[HealthMetricsCounts](
                            [MetricType] NVARCHAR(100) NOT NULL,
                            [MetricSubtype] NVARCHAR(200) NOT NULL DEFAULT '',
                            [RecordCount] BIGINT NOT NULL DEFAULT 0,
                            [EarliestDateTime] DATETIME2 NULL,
                            [MostRecentDateTime] DATETIME2 NULL,
                            [DaysBetween] AS CAST(DATEDIFF(day, [EarliestDateTime], [MostRecentDateTime]) AS DECIMAL(18,2)) PERSISTED,
                            [DaysPerRecord] AS CAST(CAST(DATEDIFF(day, [EarliestDateTime], [MostRecentDateTime]) AS DECIMAL(18,5)) / NULLIF([RecordCount], 0) AS DECIMAL(18,5)) PERSISTED,
                            [CreatedDate] DATETIME2 DEFAULT GETDATE(),
                            [LastUpdated] DATETIME2 NOT NULL DEFAULT GETDATE(),
                            CONSTRAINT PK_HealthMetricsCounts PRIMARY KEY CLUSTERED (MetricType, MetricSubtype)
                        )
                    
                    -- Index for fast lookups
                    CREATE NONCLUSTERED INDEX IX_HealthMetricsCounts_RecordCount 
                        ON [dbo].[HealthMetricsCounts](RecordCount DESC)
                END
                ELSE
                BEGIN
                    -- Add new columns if they don't exist (migration for existing tables)
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HealthMetricsCounts]') AND name = 'EarliestDateTime')
                    BEGIN
                        ALTER TABLE [dbo].[HealthMetricsCounts]
                        ADD [EarliestDateTime] DATETIME2 NULL;
                    END
                    
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HealthMetricsCounts]') AND name = 'MostRecentDateTime')
                    BEGIN
                        ALTER TABLE [dbo].[HealthMetricsCounts]
                        ADD [MostRecentDateTime] DATETIME2 NULL;
                    END

                    -- Add computed columns if they don't exist
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HealthMetricsCounts]') AND name = 'DaysBetween')
                    BEGIN
                        ALTER TABLE [dbo].[HealthMetricsCounts]
                        ADD [DaysBetween] AS CAST(DATEDIFF(day, [EarliestDateTime], [MostRecentDateTime]) AS DECIMAL(18,2)) PERSISTED;
                    END
                    
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HealthMetricsCounts]') AND name = 'DaysPerRecord')
                    BEGIN
                        ALTER TABLE [dbo].[HealthMetricsCounts]
                        ADD [DaysPerRecord] AS CAST(CAST(DATEDIFF(day, [EarliestDateTime], [MostRecentDateTime]) AS DECIMAL(18,5)) / NULLIF([RecordCount], 0) AS DECIMAL(18,5)) PERSISTED;
                    END
                END";

            using (var sqlCommand = new SqlCommand(createTableQuery, connection))
            {
                sqlCommand.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating HealthMetricsCounts table: {ex.Message}");
            // Don't throw - this is a helper table, shouldn't block main operations
        }
    }

    private static void EnsureHealthMetricsCanonicalEntries(SqlConnection connection, IEnumerable<(string MetricType, string MetricSubtype)> combos)
    {
        try
        {
            const string canonicalKeySeparator = "\u001F";
            var sanitizedEntries = new List<(string MetricType, string MetricSubtype, string MetricTypeName, string MetricSubtypeName, bool Disabled)>();
            var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var combo in combos)
            {
                var metricType = combo.MetricType?.Trim() ?? string.Empty;
                if (string.IsNullOrEmpty(metricType))
                    continue;

                var metricSubtype = combo.MetricSubtype?.Trim() ?? string.Empty;
                var combinedKey = $"{metricType}{canonicalKeySeparator}{metricSubtype}";

                if (!seenKeys.Add(combinedKey))
                    continue;

                sanitizedEntries.Add((metricType, metricSubtype, FormatMetricDisplayName(metricType), FormatMetricDisplayName(metricSubtype), false));
            }

            if (sanitizedEntries.Count > 0)
                EnsureHealthMetricsCanonicalEntries(connection, sanitizedEntries);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Error updating HealthMetricsCanonical entries: {ex.Message}");
        }
    }

    private static void EnsureHealthMetricsCanonicalEntries(SqlConnection connection, IEnumerable<(string MetricType, string MetricSubtype, string MetricTypeName, string MetricSubtypeName, bool Disabled)> entries)
    {
        try
        {
            EnsureHealthMetricsCanonicalTableExists(connection);

            const string canonicalKeySeparator = "\u001F";
            var sanitizedEntries = new List<(string MetricType, string MetricSubtype, string MetricTypeName, string MetricSubtypeName, bool Disabled)>();
            var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var entry in entries)
            {
                var metricType = entry.MetricType?.Trim() ?? string.Empty;
                if (string.IsNullOrEmpty(metricType))
                    continue;

                var metricSubtype = entry.MetricSubtype?.Trim() ?? string.Empty;
                var combinedKey = $"{metricType}{canonicalKeySeparator}{metricSubtype}";

                if (!seenKeys.Add(combinedKey))
                    continue;

                sanitizedEntries.Add((metricType, metricSubtype, string.IsNullOrWhiteSpace(entry.MetricTypeName) ? FormatMetricDisplayName(metricType) : entry.MetricTypeName.Trim(), string.IsNullOrWhiteSpace(entry.MetricSubtypeName) ? FormatMetricDisplayName(metricSubtype) : entry.MetricSubtypeName.Trim(), entry.Disabled));
            }

            if (sanitizedEntries.Count == 0)
                return;

            var valuesList = new List<string>();
            var parameters = new List<SqlParameter>();

            for (var i = 0; i < sanitizedEntries.Count; i++)
            {
                var entry = sanitizedEntries[i];
                var metricTypeParam = $"@CanonicalMetricType{i}";
                var metricSubtypeParam = $"@CanonicalMetricSubtype{i}";
                var metricTypeNameParam = $"@CanonicalMetricTypeName{i}";
                var metricSubtypeNameParam = $"@CanonicalMetricSubtypeName{i}";
                var disabledParam = $"@CanonicalMetricDisabled{i}";

                valuesList.Add($"({metricTypeParam}, {metricSubtypeParam}, {metricTypeNameParam}, {metricSubtypeNameParam}, {disabledParam})");

                parameters.Add(new SqlParameter(metricTypeParam, entry.MetricType));
                parameters.Add(new SqlParameter(metricSubtypeParam, entry.MetricSubtype));
                parameters.Add(new SqlParameter(metricTypeNameParam, entry.MetricTypeName));
                parameters.Add(new SqlParameter(metricSubtypeNameParam, entry.MetricSubtypeName));
                parameters.Add(new SqlParameter(disabledParam, entry.Disabled ? 1 : 0));
            }

            var mergeSql = $@"
                MERGE [dbo].[HealthMetricsCanonical] AS target
                USING (VALUES {string.Join(", ", valuesList)}) AS source
                    (MetricType, MetricSubtype, MetricTypeName, MetricSubtypeName, Disabled)
                ON target.MetricType = source.MetricType AND target.MetricSubtype = source.MetricSubtype
                WHEN NOT MATCHED THEN
                    INSERT (MetricType, MetricSubtype, MetricTypeName, MetricSubtypeName, Disabled, CreatedDate, LastUpdated)
                    VALUES (source.MetricType, source.MetricSubtype, source.MetricTypeName, source.MetricSubtypeName, source.Disabled, GETDATE(), GETDATE());";

            using var mergeCommand = new SqlCommand(mergeSql, connection);
            mergeCommand.Parameters.AddRange(parameters.ToArray());
            mergeCommand.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Error updating HealthMetricsCanonical entries: {ex.Message}");
        }
    }

    public static void CreateSQLTable(MetaData metaData)
    {
        var connectionString = ConfigurationManager.AppSettings["HealthDB"];
        var tableName = metaData.Name ?? $"ID_{metaData.ID}";

        DeleteSQLTable(tableName);

        var createTableQuery = $"CREATE TABLE [{tableName}]({string.Join(", ", metaData.Fields.Keys.Select(field => $"[{field}] VARCHAR(MAX) NULL"))}, [CreatedDate] DATETIME2 DEFAULT GETDATE())";

        using (var sqlConnection = new SqlConnection(connectionString))
        {
            sqlConnection.Open();
            using (var sqlCommand = new SqlCommand(createTableQuery, sqlConnection))
            {
                sqlCommand.ExecuteNonQuery();
            }
        }
    }

    public static void UpdateSQLTable(MetaData metaData, string fileContent)
    {
        var connectionString = ConfigurationManager.AppSettings["HealthDB"];
        var sqlQuery = string.Empty;

        try
        {
            var contentLines = fileContent.Split('\n');
            var tableName = metaData.Name ?? $"ID_{Math.Abs(metaData.ID)}";

            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                foreach (var contentLine in contentLines)
                {
                    var line = contentLine.Trim();
                    var fieldData = line.Split(',');

                    if (metaData != null && fieldData.Length >= metaData.Fields.Count)
                    {
                        sqlQuery = $"INSERT INTO {tableName} ({string.Join(", ", metaData.Fields.Keys)}) VALUES ({string.Join(", ", fieldData.Select(fd => $"'{fd}'"))})";

                        using (var sqlCommand = new SqlCommand(sqlQuery, sqlConnection))
                        {
                            sqlCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public static void DeleteSQLTable(string tableName)
    {
        var connectionString = ConfigurationManager.AppSettings["HealthDB"];
        var sqlConnection = new SqlConnection(connectionString);
        var sqlQuery = $"DROP TABLE {tableName}";

        try
        {
            sqlConnection.Open();
            using var sqlCommand = new SqlCommand(sqlQuery, sqlConnection);
            sqlCommand.ExecuteNonQuery();
        }
        finally
        {
            sqlConnection.Close();
        }
    }

    /// <summary>
    ///     Retrieves health metrics data from the HealthMetricsWeek table
    /// </summary>
    /// <param name="metricType">Optional filter by MetricType</param>
    /// <param name="metricSubtype">Optional filter by MetricSubtype</param>
    /// <param name="fromDate">Optional start date filter</param>
    /// <param name="toDate">Optional end date filter</param>
    /// <returns>List of weekly aggregated health metrics</returns>
    public static List<HealthMetric> GetHealthMetricsWeek(string? metricType = null, string? metricSubtype = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var connectionString = ConfigurationManager.AppSettings["HealthDB"];
        var results = new List<HealthMetric>();

        try
        {
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                var sql = new StringBuilder(@"
                    SELECT 
                        MetricType,
                        MetricSubtype,
                        NormalizedTimestamp,
                        Value,
                        Unit
                    FROM [dbo].[HealthMetricsWeek]
                    WHERE 1=1");

                var parameters = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(metricType))
                {
                    sql.Append(" AND MetricType = @MetricType");
                    parameters.Add(new SqlParameter("@MetricType", metricType));
                }

                if (!string.IsNullOrEmpty(metricSubtype))
                {
                    sql.Append(" AND MetricSubtype = @MetricSubtype");
                    parameters.Add(new SqlParameter("@MetricSubtype", metricSubtype));
                }

                if (fromDate.HasValue)
                {
                    sql.Append(" AND NormalizedTimestamp >= @FromDate");
                    parameters.Add(new SqlParameter("@FromDate", fromDate.Value));
                }

                if (toDate.HasValue)
                {
                    sql.Append(" AND NormalizedTimestamp <= @ToDate");
                    parameters.Add(new SqlParameter("@ToDate", toDate.Value));
                }

                sql.Append(" ORDER BY NormalizedTimestamp, MetricType, MetricSubtype");

                using (var sqlCommand = new SqlCommand(sql.ToString(), sqlConnection))
                {
                    sqlCommand.Parameters.AddRange(parameters.ToArray());

                    using (var reader = sqlCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var metric = new HealthMetric
                            {
                                    MetricType = reader["MetricType"]?.ToString() ?? string.Empty,
                                    MetricSubtype = reader["MetricSubtype"]?.ToString() ?? string.Empty,
                                    Value = reader["Value"] != DBNull.Value ? Convert.ToDecimal(reader["Value"]) : null,
                                    Unit = reader["Unit"]?.ToString() ?? string.Empty
                            };

                            if (reader["NormalizedTimestamp"] != DBNull.Value)
                                metric.NormalizedTimestamp = Convert.ToDateTime(reader["NormalizedTimestamp"]);

                            results.Add(metric);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving HealthMetricsWeek data: {ex.Message}");
            throw;
        }

        return results;
    }

    public static void InsertHealthMetricsHour(string? metricType = null, string? metricSubtype = null, DateTime? fromDate = null, DateTime? toDate = null, bool overwriteExisting = false)
    {
        ExecuteHealthMetricsAggregation("[dbo].[HealthMetricsHour]", "DATEADD(HOUR, DATEDIFF(HOUR, 0, NormalizedTimestamp), 0)", "HourStart", metricType, metricSubtype, fromDate, toDate, overwriteExisting);
    }

    public static void InsertHealthMetricsDay(string? metricType = null, string? metricSubtype = null, DateTime? fromDate = null, DateTime? toDate = null, bool overwriteExisting = false)
    {
        ExecuteHealthMetricsAggregation("[dbo].[HealthMetricsDay]", "DATEADD(DAY, DATEDIFF(DAY, 0, NormalizedTimestamp), 0)", "DayStart", metricType, metricSubtype, fromDate, toDate, overwriteExisting);
    }


    public static void InsertHealthMetricsWeek(string? metricType = null, string? metricSubtype = null, DateTime? fromDate = null, DateTime? toDate = null, bool overwriteExisting = false)
    {
        ExecuteHealthMetricsAggregation("[dbo].[HealthMetricsWeek]",
                @"
            DATEADD(day, -(DATEPART(weekday, CAST(CAST(NormalizedTimestamp AS DATE) AS DATETIME2)) 
            + @@DATEFIRST - 2) % 7, CAST(CAST(NormalizedTimestamp AS DATE) AS DATETIME2))",
                "WeekStart",
                metricType,
                metricSubtype,
                fromDate,
                toDate,
                overwriteExisting);
    }

    public static void InsertHealthMetricsMonth(string? metricType = null, string? metricSubtype = null, DateTime? fromDate = null, DateTime? toDate = null, bool overwriteExisting = false)
    {
        ExecuteHealthMetricsAggregation("[dbo].[HealthMetricsMonth]", "DATEADD(MONTH, DATEDIFF(MONTH, 0, NormalizedTimestamp), 0)", "MonthStart", metricType, metricSubtype, fromDate, toDate, overwriteExisting);
    }

    private static void ExecuteHealthMetricsAggregation(string targetTable, string timeGroupingExpression, string timestampColumnAlias, string? metricType, string? metricSubtype, DateTime? fromDate, DateTime? toDate, bool overwriteExisting)
    {
        var connectionString = ConfigurationManager.AppSettings["HealthDB"];

        try
        {
            using var sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();

            // -------------------------------------------------------
            // 1. Delete existing records if overwriteExisting is true
            //    (for explicit recalculation of specific ranges)
            // -------------------------------------------------------
            if (overwriteExisting)
            {
                var deleteSql = new StringBuilder($@"
                DELETE FROM {targetTable}
                WHERE 1=1");

                var deleteParams = new List<SqlParameter>();

                AppendOptionalFilter(deleteSql, deleteParams, "MetricType", metricType, "DeleteMetricType");
                AppendOptionalFilter(deleteSql, deleteParams, "ISNULL(MetricSubtype, '')", metricSubtype, "DeleteMetricSubtype");
                AppendOptionalDateRange(deleteSql, deleteParams, fromDate, toDate, "Delete");

                using var deleteCmd = new SqlCommand(deleteSql.ToString(), sqlConnection);
                deleteCmd.Parameters.AddRange(deleteParams.ToArray());
                var deletedRows = deleteCmd.ExecuteNonQuery();
                if (deletedRows > 0)
                    Console.WriteLine($"Deleted {deletedRows} existing records from {targetTable} (overwrite mode).");
            }

            // -------------------------------------------------------
            // 2. Build MERGE Query (handles duplicates gracefully)
            //    MERGE will INSERT new records or UPDATE existing ones
            //    based on the unique constraint (MetricType, MetricSubtype, NormalizedTimestamp)
            // -------------------------------------------------------

            var sql = new StringBuilder($@"
            MERGE {targetTable} AS target
            USING (
                SELECT 
                    MetricType,
                    ISNULL(MetricSubtype, '') AS MetricSubtype,
                    {timeGroupingExpression} AS {timestampColumnAlias},
                    AVG(Value) AS AvgValue,
                    MAX(Unit) AS Unit
                FROM [dbo].[HealthMetrics]
                WHERE NormalizedTimestamp IS NOT NULL
                  AND Value IS NOT NULL");

            var parameters = new List<SqlParameter>();

            AppendOptionalFilter(sql, parameters, "MetricType", metricType, "MetricType");
            AppendOptionalFilter(sql, parameters, "ISNULL(MetricSubtype, '')", metricSubtype, "MetricSubtype");
            AppendOptionalDateRange(sql, parameters, fromDate, toDate);

            sql.Append($@"
                GROUP BY 
                    MetricType,
                    ISNULL(MetricSubtype, ''),
                    {timeGroupingExpression}
            ) AS source
            ON target.MetricType = source.MetricType
               AND ISNULL(target.MetricSubtype, '') = source.MetricSubtype
               AND target.NormalizedTimestamp = source.{timestampColumnAlias}
            WHEN MATCHED THEN
                UPDATE SET 
                    Value = source.AvgValue,
                    Unit = source.Unit
            WHEN NOT MATCHED THEN
                INSERT (MetricType, MetricSubtype, NormalizedTimestamp, Value, Unit, CreatedDate)
                VALUES (source.MetricType, NULLIF(source.MetricSubtype, ''), source.{timestampColumnAlias}, source.AvgValue, source.Unit, GETDATE());");

            // -------------------------------------------------------
            // 3. Execute MERGE
            // -------------------------------------------------------
            using var mergeCmd = new SqlCommand(sql.ToString(), sqlConnection);
            mergeCmd.Parameters.AddRange(parameters.ToArray());
            mergeCmd.CommandTimeout = 300;

            var rows = mergeCmd.ExecuteNonQuery();
            Console.WriteLine($"Merged {rows} aggregated records into {targetTable} (inserted new or updated existing).");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error inserting aggregated health metrics into {targetTable}: {ex.Message}");
            throw;
        }
    }

    private static void AppendOptionalFilter(StringBuilder sql, List<SqlParameter> parameters, string columnExpression, string? value, string parameterName)
    {
        if (!string.IsNullOrEmpty(value))
        {
            sql.Append($" AND {columnExpression} = @{parameterName}");
            parameters.Add(new SqlParameter($"@{parameterName}", value));
        }
    }

    private static void AppendOptionalDateRange(StringBuilder sql, List<SqlParameter> parameters, DateTime? fromDate, DateTime? toDate, string prefix = "")
    {
        if (fromDate.HasValue && toDate.HasValue)
        {
            sql.Append($" AND NormalizedTimestamp >= @{prefix}FromDate AND NormalizedTimestamp <= @{prefix}ToDate");
            parameters.Add(new SqlParameter($"@{prefix}FromDate", fromDate.Value));
            parameters.Add(new SqlParameter($"@{prefix}ToDate", toDate.Value));
        }
        else if (fromDate.HasValue)
        {
            sql.Append($" AND NormalizedTimestamp >= @{prefix}FromDate");
            parameters.Add(new SqlParameter($"@{prefix}FromDate", fromDate.Value));
        }
        else if (toDate.HasValue)
        {
            sql.Append($" AND NormalizedTimestamp <= @{prefix}ToDate");
            parameters.Add(new SqlParameter($"@{prefix}ToDate", toDate.Value));
        }
    }

    /// <summary>
    ///     Gets the date range (min and max timestamps) for a specific MetricType and MetricSubtype combination
    /// </summary>
    /// <param name="metricType">The MetricType to query</param>
    /// <param name="metricSubtype">The MetricSubtype to query (null or empty string for records without subtype)</param>
    /// <returns>Tuple with MinDate and MaxDate, or null if no records found</returns>
    public static(DateTime MinDate, DateTime MaxDate)? GetDateRangeForMetric(string metricType, string? metricSubtype = null)
    {
        var connectionString = ConfigurationManager.AppSettings["HealthDB"];

        try
        {
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                var sql = new StringBuilder(@"
                    SELECT 
                        MIN(NormalizedTimestamp) AS MinDate,
                        MAX(NormalizedTimestamp) AS MaxDate
                    FROM [dbo].[HealthMetrics]
                    WHERE MetricType = @MetricType
                        AND NormalizedTimestamp IS NOT NULL");

                var parameters = new List<SqlParameter>
                {
                        new("@MetricType", metricType)
                };

                if (!string.IsNullOrEmpty(metricSubtype))
                {
                    sql.Append(" AND ISNULL(MetricSubtype, '') = @MetricSubtype");
                    parameters.Add(new SqlParameter("@MetricSubtype", metricSubtype));
                }
                else
                {
                    sql.Append(" AND (MetricSubtype IS NULL OR MetricSubtype = '')");
                }

                using (var sqlCommand = new SqlCommand(sql.ToString(), sqlConnection))
                {
                    sqlCommand.Parameters.AddRange(parameters.ToArray());

                    using (var reader = sqlCommand.ExecuteReader())
                    {
                        if (reader.Read())
                            if (reader["MinDate"] != DBNull.Value && reader["MaxDate"] != DBNull.Value)
                            {
                                var minDate = Convert.ToDateTime(reader["MinDate"]);
                                var maxDate = Convert.ToDateTime(reader["MaxDate"]);
                                return (minDate, maxDate);
                            }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting date range for {metricType}/{metricSubtype ?? "null"}: {ex.Message}");
            throw;
        }

        return null;
    }

    /// <summary>
    ///     Gets all distinct metric types from the HealthMetrics table.
    /// </summary>
    public static List<string> GetAllMetricTypes()
    {
        var connectionString = ConfigurationManager.AppSettings["HealthDB"];
        var metricTypes = new List<string>();

        try
        {
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                var sql = @"
                    SELECT DISTINCT MetricType 
                    FROM [dbo].[HealthMetrics]
                    WHERE MetricType IS NOT NULL
                    ORDER BY MetricType";

                using (var sqlCommand = new SqlCommand(sql, sqlConnection))
                {
                    using (var reader = sqlCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var metricType = reader["MetricType"]?.ToString();
                            if (!string.IsNullOrEmpty(metricType))
                                metricTypes.Add(metricType);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting all metric types: {ex.Message}");
            throw;
        }

        return metricTypes;
    }

    /// <summary>
    ///     Gets all distinct subtypes for a given metric type from the HealthMetrics table.
    ///     Returns a list that includes null/empty for metrics without subtypes.
    /// </summary>
    public static List<string?> GetSubtypesForMetricType(string metricType)
    {
        var connectionString = ConfigurationManager.AppSettings["HealthDB"];
        var subtypes = new List<string?>();

        try
        {
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                var sql = @"
                    SELECT DISTINCT MetricSubtype,
                        CASE WHEN MetricSubtype IS NULL OR MetricSubtype = '' THEN 0 ELSE 1 END AS SortOrder
                    FROM [dbo].[HealthMetrics]
                    WHERE MetricType = @MetricType
                    ORDER BY 
                        SortOrder,
                        MetricSubtype";

                using (var sqlCommand = new SqlCommand(sql, sqlConnection))
                {
                    sqlCommand.Parameters.Add(new SqlParameter("@MetricType", metricType));

                    using (var reader = sqlCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var subtype = reader["MetricSubtype"]?.ToString();
                            // Normalize empty strings to null for consistency
                            subtypes.Add(string.IsNullOrEmpty(subtype) ? null : subtype);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting subtypes for metric type {metricType}: {ex.Message}");
            throw;
        }

        return subtypes;
    }
}

#pragma warning restore CS8600
#pragma warning restore CS0618