namespace DataVisualiser.Models;

public class BinaryOperatorRegistry
{
    private readonly Dictionary<string, Func<double, double, double>> _ops = new(StringComparer.OrdinalIgnoreCase);

    public IEnumerable<string> Names => _ops.Keys;

    public void Register(string name, Func<double, double, double> op)
    {
        _ops[name] = op;
    }

    public bool TryGet(string name, out Func<double, double, double>? op)
    {
        return _ops.TryGetValue(name, out op);
    }
}