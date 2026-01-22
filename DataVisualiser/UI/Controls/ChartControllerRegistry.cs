namespace DataVisualiser.UI.Controls;

public sealed class ChartControllerRegistry : IChartControllerRegistry
{
    private readonly Dictionary<string, IChartController> _controllers = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<IChartController> _ordered = new();

    public void Register(IChartController controller)
    {
        if (controller == null)
            throw new ArgumentNullException(nameof(controller));

        if (string.IsNullOrWhiteSpace(controller.Key))
            throw new ArgumentException("Controller key is required.", nameof(controller));

        if (_controllers.ContainsKey(controller.Key))
            throw new InvalidOperationException($"Chart controller already registered for key '{controller.Key}'.");

        _controllers.Add(controller.Key, controller);
        _ordered.Add(controller);
    }

    public IChartController Get(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key is required.", nameof(key));

        if (_controllers.TryGetValue(key, out var controller))
            return controller;

        throw new KeyNotFoundException($"Chart controller not found for key '{key}'.");
    }

    public IReadOnlyList<IChartController> All()
    {
        return _ordered;
    }
}