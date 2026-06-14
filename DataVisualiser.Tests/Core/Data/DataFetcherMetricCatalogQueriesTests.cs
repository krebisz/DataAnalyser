using DataVisualiser.Tests.Helpers.Infrastructure;

namespace DataVisualiser.Tests.Data;

public sealed class DataFetcherMetricCatalogQueriesTests
{
    [Fact]
    public void BaseMetricTypeOptions_ShouldGroupByMetricTypeDisplayName()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser",
            "Core",
            "Data",
            "Repositories",
            "DataFetcherMetricCatalogQueries.cs");

        Assert.Contains("WITH TypeNames AS", source);
        Assert.Contains("MetricTypeName AS MetricType", source);
        Assert.Contains("GROUP BY MetricTypeName", source);
    }

    [Fact]
    public void BaseMetricTypeOptions_ShouldExcludeEmptyTypesAndTypesWithoutEnabledSubtypes()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser",
            "Core",
            "Data",
            "Repositories",
            "DataFetcherMetricCatalogQueries.cs");

        Assert.Contains("c.MetricType != ''", source);
        Assert.Contains("c.MetricSubtype IS NOT NULL", source);
        Assert.Contains("c.MetricSubtype != ''", source);
        Assert.Contains("t.MetricType != ''", source);
        Assert.Contains("t.MetricSubtype IS NOT NULL", source);
        Assert.Contains("t.MetricSubtype != ''", source);
        Assert.Contains("AND (m.Disabled IS NULL OR m.Disabled = 0)", source);
    }

    [Fact]
    public void SubtypeOptions_ShouldResolveRawMetricTypeOrDisplayName()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser",
            "Core",
            "Data",
            "Repositories",
            "DataFetcherMetricCatalogQueries.cs");

        Assert.Contains("metricTypeName.MetricType = c.MetricType", source);
        Assert.Contains("metricTypeName.MetricType = t.MetricType", source);
        Assert.Contains("COALESCE(NULLIF(metricTypeName.MetricTypeName, ''), metricTypeName.MetricType)", source);
    }

    [Fact]
    public void SubtypeOptions_ShouldUseDisplayedSubtypeNameAsSelectionKey()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser",
            "Core",
            "Data",
            "Repositories",
            "DataFetcherMetricCatalogQueries.cs");

        Assert.Contains("COALESCE(NULLIF(m.MetricSubtypeName, ''), c.MetricSubtype) AS MetricSubtype", source);
        Assert.Contains("COALESCE(NULLIF(m.MetricSubtypeName, ''), t.MetricSubtype) AS MetricSubtype", source);
        Assert.Contains("GROUP BY COALESCE(NULLIF(m.MetricSubtypeName, ''), c.MetricSubtype)", source);
        Assert.Contains("GROUP BY COALESCE(NULLIF(m.MetricSubtypeName, ''), t.MetricSubtype)", source);
        Assert.DoesNotContain("MAX(m.MetricSubtypeName)", source);
    }

    [Fact]
    public void AdminMetricTypeFilter_ShouldUseDisplayedMetricTypeName()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser",
            "Core",
            "Data",
            "Repositories",
            "DataFetcherAdminQueries.cs");

        Assert.Contains("COALESCE(NULLIF(m.MetricTypeName, ''), c.MetricType) AS MetricType", source);
        Assert.Contains("metricTypeName.MetricType = c.MetricType", source);
        Assert.Contains("COALESCE(NULLIF(metricTypeName.MetricTypeName, ''), metricTypeName.MetricType) = @MetricType", source);
    }
}
