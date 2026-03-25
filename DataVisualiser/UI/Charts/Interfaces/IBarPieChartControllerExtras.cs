namespace DataVisualiser.UI.Charts.Interfaces;

public interface IBarPieChartControllerExtras
{
    void InitializeControls();
    Task RenderIfVisibleAsync();
    void SelectBucketCount(int bucketCount);
    string GetDisplayMode();
}
