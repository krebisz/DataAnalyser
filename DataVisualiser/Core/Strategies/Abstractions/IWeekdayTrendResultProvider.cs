using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Strategies.Abstractions;

public interface IWeekdayTrendResultProvider
{
    WeekdayTrendResult? ExtendedResult { get; }
}
