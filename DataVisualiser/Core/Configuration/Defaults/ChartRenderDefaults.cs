namespace DataVisualiser.Core.Configuration.Defaults;

/// <summary>
///     Centralized defaults for rendering logic used by chart engines/services.
/// </summary>
public static class ChartRenderDefaults
{
    public const string AxisTitleTime = "Time";
    public const string AxisTitleValue = "Value";
    public const string AxisTitleDayOfWeek = "Day of Week";

    public const int DesiredXAxisTickCount = 10;

    public const int SmoothedPointSize = 5;
    public const int SmoothedLineThickness = 2;
    public const int RawPointSize = 3;
    public const int RawLineThickness = 1;

    public const double WeekdayLineStrokeThickness = 2.0;
    public const double WeekdayPointSize = 6.0;
    public const double WeekdayLineSmoothness = 0.3;
    public const string WeekdayDateLabelFormat = "yyyy-MM-dd";

    public const double PolarAxisMinValue = 0.0;
    public const double PolarAxisMaxValue = 360.0;
}
