using System.Windows;
using System.Windows.Media;
using DataVisualiser.Core.Configuration.Defaults;
using LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Defaults;

/// <summary>
///     Centralized UI defaults for chart controllers and layout.
/// </summary>
public static class ChartUiDefaults
{
    public const string MainChartTitle = "ChartMain";
    public const string NormalizedChartTitle = "ChartNorm";
    public const string DistributionChartTitle = "Distribution";
    public const string DiffRatioChartTitle = "Difference / Ratio";
    public const string WeekdayTrendChartTitle = "Weekday Trends";
    public const string TransformChartTitle = "Data Transform";

    public const string AxisTitleTime = ChartRenderDefaults.AxisTitleTime;
    public const string AxisTitleValue = ChartRenderDefaults.AxisTitleValue;
    public const string AxisTitleDayOfWeek = ChartRenderDefaults.AxisTitleDayOfWeek;
    public const string AxisTitleRelative = "Relative";
    public const string AxisTitleBucket = "Bucket";
    public const string AxisTitleDifference = "Difference";

    public const double ChartMinHeight = 400.0;
    public const double MainChartMinHeight = 500.0;

    public static readonly Thickness ChartContentMargin = new(20, 5, 10, 20);
    public static readonly Thickness BehavioralControlsMargin = new(40, 5, 10, 0);
    public static readonly Thickness LabelMargin = new(0, 0, 10, 0);
    public static readonly Thickness SectionLabelMargin = new(20, 0, 10, 0);
    public static readonly Thickness ToggleButtonMargin = new(20, 0, 0, 0);
    public static readonly Thickness ToggleButtonPadding = new(10, 3, 10, 3);
    public static readonly Thickness ToggleButtonPaddingCompact = new(10, 3, 10, 3);
    public static readonly Thickness DayCheckboxMargin = new(5, 0, 0, 0);
    public static readonly Thickness RadioButtonMargin = new(5, 0, 0, 0);

    public const double SubtypeComboWidth = 160.0;
    public const string ChartTypeToggleLabel = "Polar";
    public const string ChartTypeToggleToolTip = "Toggle between Cartesian and Polar chart";

    public const string OperationToggleContent = "/";
    public const string OperationToggleToolTip = "Toggle between Difference (-) and Ratio (/)";

    public const double PolarAxisMinValue = ChartRenderDefaults.PolarAxisMinValue;
    public const double PolarAxisMaxValue = ChartRenderDefaults.PolarAxisMaxValue;

    public const double DistributionModeComboWidth = 100.0;
    public const double DistributionSubtypeComboWidth = 160.0;
    public const double DistributionIntervalComboWidth = 80.0;
    public const double DistributionPolarInitialRotation = -45.0;

    public static readonly Thickness ChartPanelBorderMargin = new(0, 0, 0, 10);
    public static readonly Thickness ChartPanelStackMargin = new(0, 0, 0, 10);
    public static readonly Thickness ChartHeaderMargin = new(20, 5, 10, 0);
    public static readonly Thickness ChartHeaderPadding = new(10, 5, 10, 5);
    public static readonly Thickness ChartHeaderToggleMargin = new(10, 0, 0, 0);
    public static readonly Thickness ChartHeaderTogglePadding = new(10, 3, 10, 3);
    public static readonly Thickness ChartHeaderControlsMargin = new(20, 0, 0, 0);

    public const double ChartHeaderFontSize = 14.0;

    public static readonly Brush ChartPanelBorderBrush = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44));
    public static readonly Brush ChartHeaderBackground = new SolidColorBrush(Color.FromArgb(0xBB, 0x13, 0x13, 0x13));
    public static readonly Brush ChartHeaderBorderBrush = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44));

    public static readonly Thickness TransformOperationRowMargin = new(20, 10, 20, 10);
    public static readonly Thickness TransformPanelRightMargin = new(0, 0, 20, 0);
    public static readonly Thickness TransformPanelRightMarginSmall = new(0, 0, 10, 0);
    public static readonly Thickness TransformComputeButtonMargin = new(10, 20, 0, 0);
    public static readonly Thickness TransformComputeButtonPadding = new(15, 5, 15, 5);
    public static readonly Thickness TransformChartContainerMargin = new(10, 0, 0, 0);

    public const double TransformPrimaryPanelMinWidth = 160.0;
    public const double TransformPrimaryComboMinWidth = 140.0;
    public const double TransformOperationPanelMinWidth = 150.0;
    public const double TransformOperationComboMinWidth = 120.0;
    public const double TransformSecondaryPanelMinWidth = 160.0;
    public const double TransformGridMinWidth = 250.0;
    public const double TransformGridMaxHeight = 400.0;
    public const double TransformChartContainerMinWidth = 400.0;
    public const double TransformGridTimestampMinWidth = 150.0;
    public const double TransformGridValueMinWidth = 100.0;
    public const double TransformComputeButtonFontSize = 20.0;

    public static readonly LegendLocation DefaultLegendLocation = LegendLocation.Right;
    public static readonly ZoomingOptions DefaultZoom = ZoomingOptions.X;
    public static readonly PanningOptions DefaultPan = PanningOptions.X;
    public const bool DefaultHoverable = true;
}

