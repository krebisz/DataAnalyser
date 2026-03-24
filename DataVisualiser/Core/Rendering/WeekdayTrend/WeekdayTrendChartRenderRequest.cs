using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;

namespace DataVisualiser.Core.Rendering.WeekdayTrend;

public sealed record WeekdayTrendChartRenderRequest(
    WeekdayTrendRenderingRoute Route,
    WeekdayTrendResult Result,
    ChartState ChartState);
