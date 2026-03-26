namespace DataFileReader.Services;

public interface IHealthMetricsMaintenanceService
{
    void EnsureHealthMetricsTableExists();
    void CleanDatabase();
}
