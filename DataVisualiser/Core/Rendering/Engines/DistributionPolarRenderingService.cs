using System.Windows.Media;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Separator = LiveCharts.Wpf.Separator;

namespace DataVisualiser.Core.Rendering.Engines;

public sealed class DistributionPolarRenderingService
{
    private const double OuterRadius = 1.0;
    private const double LabelRadius = 1.12;
    private const int CircleSegments = 72;
    private const int GridRingCount = 4;

    public void RenderPolarChart(DistributionRangeResult rangeResult, DistributionModeDefinition definition, CartesianChart chart)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        var bucketCount = definition.XAxisLabels.Count;
        if (bucketCount <= 0)
            return;

        var radiusModel = RadiusModel.Create(rangeResult);
        var axisModel = AxisModel.Create(chart, LabelRadius);

        chart.Series.Clear();
        chart.AxisX.Clear();
        chart.AxisY.Clear();
        chart.DataTooltip = null;

        ApplyProjectionBounds(chart, axisModel);
        AddGrid(chart, definition, radiusModel);
        AddPerimeterLabels(chart, definition);
        AddRingLabels(chart, rangeResult, radiusModel);
        AddDataSeries(chart, "Min", rangeResult.Mins, bucketCount, radiusModel, Color.FromRgb(70, 130, 180));
        AddDataSeries(chart, "Max", rangeResult.Maxs, bucketCount, radiusModel, Color.FromRgb(220, 80, 80));
        AddDataSeries(chart, "Avg", rangeResult.Averages, bucketCount, radiusModel, Color.FromRgb(235, 200, 40));
        chart.Update(true, true);
    }

    public void RefitPolarProjection(CartesianChart chart)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        var axisModel = AxisModel.Create(chart, LabelRadius);
        ApplyProjectionBounds(chart, axisModel);
        chart.Update(true, true);
    }

    private static void ApplyProjectionBounds(CartesianChart chart, AxisModel axisModel)
    {
        var xAxis = EnsureAxis(chart.AxisX);
        var yAxis = EnsureAxis(chart.AxisY);

        ConfigureAxis(xAxis, axisModel.XMin, axisModel.XMax);
        ConfigureAxis(yAxis, axisModel.YMin, axisModel.YMax);
    }

    private static Axis EnsureAxis(AxesCollection axes)
    {
        if (axes.Count > 0)
            return axes[0];

        var axis = new Axis();
        axes.Add(axis);
        return axis;
    }

    private static void ConfigureAxis(Axis axis, double minValue, double maxValue)
    {
        axis.MinValue = minValue;
        axis.MaxValue = maxValue;
        axis.ShowLabels = false;
        axis.Separator ??= new Separator();
        axis.Separator.IsEnabled = false;
    }

    private static void AddGrid(CartesianChart chart, DistributionModeDefinition definition, RadiusModel radiusModel)
    {
        for (var ringIndex = 1; ringIndex <= GridRingCount; ringIndex++)
        {
            var normalizedRadius = OuterRadius * ringIndex / GridRingCount;
            chart.Series.Add(CreateGridSeries(BuildCirclePoints(normalizedRadius), Color.FromRgb(210, 210, 210)));
        }

        for (var bucketIndex = 0; bucketIndex < definition.XAxisLabels.Count; bucketIndex++)
        {
            var angle = ResolveAngleRadians(bucketIndex, definition.XAxisLabels.Count);
            chart.Series.Add(CreateGridSeries(
                    new ChartValues<ObservablePoint>
                    {
                            new(0, 0),
                            ProjectPoint(angle, OuterRadius)
                    },
                    Color.FromRgb(220, 220, 220)));
        }
    }

    private static void AddPerimeterLabels(CartesianChart chart, DistributionModeDefinition definition)
    {
        for (var bucketIndex = 0; bucketIndex < definition.XAxisLabels.Count; bucketIndex++)
        {
            var index = bucketIndex;
            var angle = ResolveAngleRadians(bucketIndex, definition.XAxisLabels.Count);
            var point = ProjectPoint(angle, LabelRadius);

            chart.Series.Add(new LineSeries
            {
                    Title = null,
                    Values = new ChartValues<ObservablePoint>
                    {
                            point
                    },
                    DataLabels = true,
                    LabelPoint = _ => definition.XAxisLabels[index],
                    Stroke = Brushes.Transparent,
                    Fill = Brushes.Transparent,
                    PointGeometry = null,
                    PointGeometrySize = 0,
                    StrokeThickness = 0
            });
        }
    }

    private static void AddRingLabels(CartesianChart chart, DistributionRangeResult rangeResult, RadiusModel radiusModel)
    {
        for (var ringIndex = 1; ringIndex <= GridRingCount; ringIndex++)
        {
            var actualValue = radiusModel.RadialMin + ringIndex * radiusModel.RadialSpan / GridRingCount;
            var point = ProjectPoint(0, OuterRadius * ringIndex / GridRingCount);
            var label = FormatValue(actualValue, rangeResult.Unit);

            chart.Series.Add(new LineSeries
            {
                    Title = null,
                    Values = new ChartValues<ObservablePoint>
                    {
                            point
                    },
                    DataLabels = true,
                    LabelPoint = _ => label,
                    Stroke = Brushes.Transparent,
                    Fill = Brushes.Transparent,
                    PointGeometry = null,
                    PointGeometrySize = 0,
                    StrokeThickness = 0
            });
        }
    }

    private static void AddDataSeries(CartesianChart chart, string name, IReadOnlyList<double> values, int bucketCount, RadiusModel radiusModel, Color strokeColor)
    {
        var seriesValues = BuildProjectedValues(values, bucketCount, radiusModel);
        if (seriesValues.Count == 0)
            return;

        chart.Series.Add(new LineSeries
        {
                Title = name,
                Values = seriesValues,
                LineSmoothness = 0,
                Stroke = new SolidColorBrush(strokeColor),
                Fill = Brushes.Transparent,
                PointGeometry = DefaultGeometries.Circle,
                PointGeometrySize = 6,
                StrokeThickness = 2
        });
    }

    private static LineSeries CreateGridSeries(ChartValues<ObservablePoint> values, Color strokeColor)
    {
        return new LineSeries
        {
                Title = null,
                Values = values,
                Stroke = new SolidColorBrush(strokeColor),
                Fill = Brushes.Transparent,
                PointGeometry = null,
                PointGeometrySize = 0,
                StrokeThickness = 1
        };
    }

    private static ChartValues<ObservablePoint> BuildCirclePoints(double normalizedRadius)
    {
        var values = new ChartValues<ObservablePoint>();
        for (var step = 0; step <= CircleSegments; step++)
        {
            var angle = 2 * Math.PI * step / CircleSegments;
            values.Add(ProjectPoint(angle, normalizedRadius));
        }

        return values;
    }

    private static ChartValues<ObservablePoint> BuildProjectedValues(IReadOnlyList<double> values, int bucketCount, RadiusModel radiusModel)
    {
        var projected = new ChartValues<ObservablePoint>();
        if (values.Count < bucketCount)
            return projected;

        ObservablePoint? firstPoint = null;
        for (var bucketIndex = 0; bucketIndex < bucketCount; bucketIndex++)
        {
            var value = values[bucketIndex];
            if (double.IsNaN(value) || double.IsInfinity(value))
                continue;

            var angle = ResolveAngleRadians(bucketIndex, bucketCount);
            var normalizedRadius = radiusModel.Normalize(value);
            var point = ProjectPoint(angle, normalizedRadius);
            firstPoint ??= point;
            projected.Add(point);
        }

        if (firstPoint != null)
            projected.Add(new ObservablePoint(firstPoint.X, firstPoint.Y));

        return projected;
    }

    private static ObservablePoint ProjectPoint(double angleRadians, double normalizedRadius)
    {
        return new ObservablePoint(
                normalizedRadius * Math.Cos(angleRadians),
                normalizedRadius * Math.Sin(angleRadians));
    }

    private static double ResolveAngleRadians(int bucketIndex, int bucketCount)
    {
        var fraction = bucketCount == 0 ? 0 : (double)bucketIndex / bucketCount;
        return -Math.PI / 2 + 2 * Math.PI * fraction;
    }

    private static string FormatValue(double value, string? unit)
    {
        var formatted = MathHelper.FormatDisplayedValue(value);
        return string.IsNullOrWhiteSpace(unit) ? formatted : $"{formatted} {unit}";
    }

    private readonly record struct RadiusModel(double RadialMin, double RadialSpan)
    {
        public static RadiusModel Create(DistributionRangeResult rangeResult)
        {
            var min = rangeResult.GlobalMin;
            var max = rangeResult.GlobalMax;
            if (double.IsNaN(min) || double.IsInfinity(min))
                min = 0;
            if (double.IsNaN(max) || double.IsInfinity(max))
                max = min + 1;

            var range = max - min;
            if (range <= 0 || double.IsNaN(range) || double.IsInfinity(range))
                range = Math.Abs(max) > 0 ? Math.Abs(max) : 1;

            return new RadiusModel(min - range, max - (min - range));
        }

        public double Normalize(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                return 0;

            if (RadialSpan <= 0)
                return 0;

            var normalized = (value - RadialMin) / RadialSpan;
            return Math.Max(0.05, Math.Min(OuterRadius, normalized * OuterRadius));
        }
    }

    private readonly record struct AxisModel(double XMin, double XMax, double YMin, double YMax)
    {
        public static AxisModel Create(CartesianChart chart, double outerExtent)
        {
            var width = chart.ActualWidth > 0 ? chart.ActualWidth : 1;
            var height = chart.ActualHeight > 0 ? chart.ActualHeight : 1;
            var aspect = width / height;
            if (aspect <= 0 || double.IsNaN(aspect) || double.IsInfinity(aspect))
                aspect = 1;

            var xExtent = outerExtent * Math.Max(1, aspect);
            var yExtent = outerExtent * Math.Max(1, 1 / aspect);
            return new AxisModel(-xExtent, xExtent, -yExtent, yExtent);
        }
    }
}
