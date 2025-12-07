using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataVisualiser.ViewModels.Events
{
    public class MetricTypesLoadedEventArgs : EventArgs
    {
        public IEnumerable<string> MetricTypes { get; set; } = Enumerable.Empty<string>();
    }

    public class SubtypesLoadedEventArgs : EventArgs
    {
        public IEnumerable<string> Subtypes { get; set; } = Enumerable.Empty<string>();
    }

    public class DateRangeLoadedEventArgs : EventArgs
    {
        public DateTime? MinDate { get; set; }
        public DateTime? MaxDate { get; set; }
    }

    public class DataLoadedEventArgs : EventArgs
    {
        public ChartDataContext DataContext { get; set; } = new();
    }

    public class ChartVisibilityChangedEventArgs : EventArgs
    {
        public string ChartName { get; set; } = "";
        public bool IsVisible { get; set; }
    }

    public class ErrorEventArgs : EventArgs
    {
        public string Message { get; set; } = "";
    }

    public class ChartUpdateRequestedEventArgs : EventArgs
    {
        public bool ShowNormalized { get; set; }
        public bool ShowDifference { get; set; }
        public bool ShowRatio { get; set; }
        public bool ShowWeekly { get; set; }

        // NEW: whether charts should be rendered
        public bool ShouldRenderCharts { get; set; }
    }



}
