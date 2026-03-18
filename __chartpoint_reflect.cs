using System;
using LiveCharts;
class P {
  static void Main() {
    var t = typeof(ChartPoint);
    foreach (var p in t.GetProperties()) Console.WriteLine($"{p.Name}:{p.PropertyType.FullName}");
  }
}
