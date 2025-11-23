//using LiveChartsCore;
//using LiveChartsCore.Defaults;
//using LiveChartsCore.SkiaSharpView;

//namespace DataVisualiser
//{
//    public class ChartViewModel
//    {
//        public ISeries[] Series { get; set; }
//        public Axis[] XAxes { get; set; } = { new Axis { Labeler = value => new DateTime((long)value).ToString("MM-dd HH:mm") } };
//        public Axis[] YAxes { get; set; } = { new Axis { Name = "Value" } };

//        public void LoadData(IEnumerable<dynamic> data, string[] tables)
//        {
//            var seriesList = new List<ISeries>();

//            foreach (var table in tables)
//            {
//                var points = data.Select(d => new DateTimePoint((DateTime)d.datetime, (double)(d[table] ?? 0))).ToList();
//                seriesList.Add(new LineSeries<DateTimePoint> { Values = points, Name = table });
//            }

//            Series = seriesList.ToArray();
//        }
//    }

//}