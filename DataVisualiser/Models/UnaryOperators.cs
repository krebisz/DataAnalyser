namespace DataVisualiser.Models;

public static class UnaryOperators
{
    public static readonly Func<double, double> Logarithm = x => x <= 0 ? double.NaN : Math.Log(x);

    public static readonly Func<double, double> SquareRoot = x => x < 0 ? double.NaN : Math.Sqrt(x);
}