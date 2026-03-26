using DataFileReader.Helper;

namespace DataFileReader.Services;

public sealed class SqlHealthMetricsMaintenanceService : IHealthMetricsMaintenanceService
{
    public void EnsureHealthMetricsTableExists()
    {
        SQLHelper.EnsureHealthMetricsTableExists();
    }

    public void CleanDatabase()
    {
        SQLHelper.CleanDatabase();
    }
}
