namespace DataVisualiser.UI.Charts.Presentation;

public interface IBarPieChartControllerExtras
{
    void InitializeControls();
    Task RenderIfVisibleAsync();
    void SelectBucketCount(int bucketCount);
    string GetDisplayMode();
}
