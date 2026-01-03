using System.Diagnostics;
using DataFileReader.Helper;
using DataFileReader.Ingestion;

public static class ShadowValidate_SamsungHealthCsv
{
    public static void Run(string filePath, string fileContent)
    {
        var rawRecords = new List<RawRecord>();

        // Capture RawRecords emitted by the parser (shadow-mode)
        RawRecordFactory.OnCreated = r => rawRecords.Add(r);

        // Run existing parser (no behavior change)
        var metrics = SamsungHealthCsvParser.Parse(filePath, fileContent);

        // Detach to avoid leaking across runs
        RawRecordFactory.OnCreated = null;

        Debug.WriteLine($"Metrics: {metrics.Count}, RawRecords: {rawRecords.Count}");

        if (metrics.Count != rawRecords.Count)
            throw new InvalidOperationException($"Count mismatch: metrics={metrics.Count}, raw={rawRecords.Count}");

        // Basic field parity checks (strict)
        for (var i = 0; i < metrics.Count; i++)
        {
            var m = metrics[i];
            var r = rawRecords[i];

            AssertEq(m.Provider, r, "Provider");
            AssertEq(m.MetricType, r, "MetricType");
            AssertEq(m.MetricSubtype, r, "MetricSubtype");
            AssertEq(m.SourceFile, r, "SourceFile");
            AssertEq(m.RawTimestamp, r, "RawTimestamp");
            AssertEq(m.Unit, r, "Unit");

            // Value can be null; compare safely
            var rv = r.Fields.TryGetValue("Value", out var vo) ? vo : null;
            if (!Equals(m.Value, rv))
                throw new InvalidOperationException($"Value mismatch at index {i}: metric={m.Value}, raw={rv}");
        }

        Debug.WriteLine("Shadow validation PASSED.");
    }

    private static void AssertEq(string expected, RawRecord record, string key)
    {
        var actual = record.Fields.TryGetValue(key, out var v) ? v?.ToString() ?? "" : "<missing>";
        if (!string.Equals(expected ?? "", actual, StringComparison.Ordinal))
            throw new InvalidOperationException($"{key} mismatch: expected='{expected}', actual='{actual}'");
    }
}