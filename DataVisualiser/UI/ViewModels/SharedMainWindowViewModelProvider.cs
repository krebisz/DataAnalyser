using DataVisualiser.Core.Services;
using DataVisualiser.UI.State;

namespace DataVisualiser.UI.ViewModels;

public sealed record SharedMainWindowViewModelContext(
    MainWindowViewModel ViewModel,
    ChartState ChartState,
    MetricState MetricState,
    UiState UiState,
    MetricSelectionService MetricSelectionService);

public static class SharedMainWindowViewModelProvider
{
    private static readonly object SyncLock = new();
    private static SharedMainWindowViewModelContext? _context;

    public static SharedMainWindowViewModelContext GetOrCreate(string connectionString)
    {
        lock (SyncLock)
        {
            if (_context != null)
                return _context;

            var chartState = new ChartState();
            var metricState = new MetricState();
            var uiState = new UiState();
            var metricSelectionService = new MetricSelectionService(connectionString);
            var viewModel = new MainWindowViewModel(chartState, metricState, uiState, metricSelectionService);

            _context = new SharedMainWindowViewModelContext(
                viewModel,
                chartState,
                metricState,
                uiState,
                metricSelectionService);

            return _context;
        }
    }
}
