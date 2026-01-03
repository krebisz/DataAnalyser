using DataFileReader.App;
using DataFileReader.Parsers;
using DataFileReader.Services;

internal class Program
{
    private static void Main(string[] args)
    {
        var parsers = new IHealthFileParser[]
        {
                new SamsungJsonParser(),
                new SamsungCsvParser(),
                new LegacyJsonParser()
        };

        var aggregator = new MetricAggregator();
        var fileProcessor = new FileProcessingService(parsers);

        var app = new HealthDataApp(aggregator, fileProcessor);

        app.Run();

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}