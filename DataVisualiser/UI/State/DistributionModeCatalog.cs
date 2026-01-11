using DataVisualiser.Shared.Models;

namespace DataVisualiser.UI.State;

public static class DistributionModeCatalog
{
    public static IReadOnlyList<DistributionModeDefinition> All { get; } = new List<DistributionModeDefinition>
    {
            new(DistributionMode.Weekly,
                    "Weekly",
                    "Weekly Distribution",
                    "Day of Week",
                    new[]
                    {
                            "Monday",
                            "Tuesday",
                            "Wednesday",
                            "Thursday",
                            "Friday",
                            "Saturday",
                            "Sunday"
                    },
                    25),
            new(DistributionMode.Hourly,
                    "Hourly",
                    "Hourly Distribution",
                    "Hours of Day",
                    new[]
                    {
                            "12AM",
                            "1AM",
                            "2AM",
                            "3AM",
                            "4AM",
                            "5AM",
                            "6AM",
                            "7AM",
                            "8AM",
                            "9AM",
                            "10AM",
                            "11AM",
                            "12PM",
                            "1PM",
                            "2PM",
                            "3PM",
                            "4PM",
                            "5PM",
                            "6PM",
                            "7PM",
                            "8PM",
                            "9PM",
                            "10PM",
                            "11PM"
                    },
                    15)
    };

    public static IReadOnlyList<int> IntervalCounts { get; } = new[]
    {
            3,
            5,
            10,
            15,
            25,
            50,
            100
    };

    public static DistributionModeDefinition Get(DistributionMode mode)
    {
        var definition = All.FirstOrDefault(d => d.Mode == mode);
        if (definition != null)
            return definition;

        throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown distribution mode.");
    }
}