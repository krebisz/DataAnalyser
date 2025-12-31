# ChartPanelController - Reusable Chart Panel Component

## Overview

`ChartPanelController` is a reusable composite UserControl that encapsulates the common structure of chart panels in the application. It provides a standardized way to create chart panels with:

- Header section with title and toggle button
- Optional behavioral controls section
- Chart content area
- Access to chart rendering context and data

## Architecture

### Components

1. **ChartPanelController.xaml/xaml.cs**: The base UserControl that provides the common UI structure
2. **IChartRenderingContext.cs**: Interface that provides access to chart data and state
3. **ChartRenderingContextAdapter.cs**: Adapter implementation that bridges MainWindow to the interface

### Key Features

- **Title Property**: Sets the chart panel title
- **IsChartVisible Property**: Controls panel visibility and toggle button state
- **RenderingContext Property**: Provides access to ChartDataContext and ChartState
- **Header Controls**: Can inject additional controls in the header (e.g., operation toggle buttons)
- **Behavioral Controls**: Can inject behavioral controls (e.g., radio buttons, checkboxes)
- **Chart Content**: Can inject the actual chart control (LiveCharts.Wpf.CartesianChart)

## Usage Example

### 1. Create a Chart-Specific Controller

```csharp
public partial class DiffRatioChartController : ChartPanelController
{
    private readonly Button _operationToggleButton;
    private readonly Axis _axisY;
    private readonly CartesianChart _chart;

    public DiffRatioChartController()
    {
        // Initialize the base controller
        Title = "Difference / Ratio";
        IsChartVisible = false;

        // Create operation toggle button
        _operationToggleButton = new Button
        {
            Content = "/",
            Margin = new Thickness(20, 0, 0, 0),
            Padding = new Thickness(10, 3, 10, 3),
            VerticalAlignment = VerticalAlignment.Center
        };
        _operationToggleButton.Click += OnOperationToggle;

        // Create chart
        _chart = new CartesianChart
        {
            LegendLocation = LegendLocation.Right,
            Zoom = ZoomingOptions.X,
            Pan = PanningOptions.X,
            Hoverable = true,
            Margin = new Thickness(20, 5, 10, 20),
            MinHeight = 400
        };
        _chart.AxisX.Add(new Axis { Title = "Time" });
        _axisY = new Axis { Title = "Difference", ShowLabels = true };
        _chart.AxisY.Add(_axisY);

        // Set up the controller
        SetHeaderControls(_operationToggleButton);
        SetChartContent(_chart);
    }

    private void OnOperationToggle(object sender, RoutedEventArgs e)
    {
        if (ChartState != null)
        {
            ChartState.IsDiffRatioDifferenceMode = !ChartState.IsDiffRatioDifferenceMode;
            UpdateButtonState();
            RequestRender();
        }
    }

    private void UpdateButtonState()
    {
        if (ChartState != null)
        {
            _operationToggleButton.Content = ChartState.IsDiffRatioDifferenceMode ? "/" : "-";
            _axisY.Title = ChartState.IsDiffRatioDifferenceMode ? "Difference" : "Ratio";
        }
    }

    private void RequestRender()
    {
        if (IsChartVisible && ChartDataContext != null && HasSecondaryData())
        {
            RenderRequested?.Invoke(this, ChartDataContext);
        }
    }
}
```

### 2. Integrate with MainWindow

```csharp
// In MainWindow.xaml.cs constructor or initialization
var renderingContext = new ChartRenderingContextAdapter(
    _viewModel.ChartState,
    () => _viewModel.ChartState.LastContext,
    HasSecondaryData,
    ShouldRenderCharts
);

var diffRatioController = new DiffRatioChartController
{
    RenderingContext = renderingContext
};

diffRatioController.ToggleRequested += (s, e) => _viewModel.ToggleDiffRatio();
diffRatioController.RenderRequested += async (s, ctx) => 
{
    await RenderDiffRatio(ctx, ctx.MetricType, ctx.PrimarySubtype, ctx.SecondarySubtype);
};

// Bind visibility
diffRatioController.SetBinding(ChartPanelController.IsChartVisibleProperty, 
    new Binding("ChartState.IsDiffRatioVisible") { Source = _viewModel });
```

### 3. Use in XAML

```xml
<ui:DiffRatioChartController x:Name="DiffRatioChartController"
                             Title="Difference / Ratio"
                             RenderingContext="{Binding RenderingContext}"/>
```

## Benefits

1. **Reusability**: Each chart panel follows the same structure and can be easily created
2. **Separation of Concerns**: Chart-specific logic is isolated in dedicated controllers
3. **Testability**: Controllers can be tested independently with mock rendering contexts
4. **Maintainability**: Changes to the common panel structure only need to be made in one place
5. **Access to Data**: All controllers have access to ChartDataContext and ChartState through the rendering context

## Migration Path

To migrate existing chart panels:

1. Create a chart-specific controller class inheriting from `ChartPanelController`
2. Move chart-specific UI elements and logic to the controller
3. Create behavioral controls programmatically or via XAML
4. Wire up events and bindings
5. Replace the old StackPanel structure in MainWindow.xaml with the new controller

