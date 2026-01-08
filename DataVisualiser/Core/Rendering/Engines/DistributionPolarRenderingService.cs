using System.Collections.Generic;
using System.Linq;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WPF;
using SkiaSharp;

namespace DataVisualiser.Core.Rendering.Engines;

public sealed class DistributionPolarRenderingService
{
    public void RenderPolarChart(DistributionRangeResult rangeResult, DistributionModeDefinition definition, PolarChart chart)
    {
        var bucketCount = definition.XAxisLabels.Count;
        if (bucketCount <= 0)
            return;

        chart.AngleAxes = new[]
        {
                new PolarAxis
                {
                        Labels = definition.XAxisLabels.ToArray(),
                        LabelsRotation = 20,
                        MinLimit = 0,
                        MaxLimit = bucketCount
                }
        };

        var range = rangeResult.GlobalMax - rangeResult.GlobalMin;
        var radiusMinLimit = rangeResult.GlobalMin - range;

        chart.RadiusAxes = new[]
        {
                new PolarAxis
                {
                        MinLimit = radiusMinLimit,
                        MaxLimit = rangeResult.GlobalMax
                }
        };

        chart.Series = new ISeries[]
        {
                CreateSeries("Min", rangeResult.Mins, bucketCount, new SKColor(70, 130, 180), new SKColor(70, 130, 180, 80)),
                CreateSeries("Max", rangeResult.Maxs, bucketCount, new SKColor(220, 80, 80), new SKColor(220, 80, 80, 80))
        };
    }

    private static PolarLineSeries<ObservablePoint> CreateSeries(string name, IReadOnlyList<double> values, int bucketCount, SKColor strokeColor, SKColor fillColor)
    {
        return new PolarLineSeries<ObservablePoint>
        {
                Name = name,
                Values = BuildSeriesValues(values, bucketCount),
                LineSmoothness = 0.2,
                Stroke = new SolidColorPaint(strokeColor, 2),
                Fill = new SolidColorPaint(fillColor),
                GeometrySize = 6,
                GeometryStroke = new SolidColorPaint(strokeColor),
                GeometryFill = new SolidColorPaint(strokeColor)
        };
    }

    private static IReadOnlyCollection<ObservablePoint> BuildSeriesValues(IReadOnlyList<double> values, int bucketCount)
    {
        var seriesValues = new List<ObservablePoint>();
        if (values.Count < bucketCount)
            return seriesValues;

        for (var i = 0; i < bucketCount; i++)
        {
            var value = double.IsNaN(values[i]) ? 0.0 : values[i];
            seriesValues.Add(new ObservablePoint(i, value));
        }

        // Close the loop.
        if (bucketCount > 0)
            seriesValues.Add(new ObservablePoint(bucketCount, double.IsNaN(values[0]) ? 0.0 : values[0]));

        return seriesValues;
    }
}
