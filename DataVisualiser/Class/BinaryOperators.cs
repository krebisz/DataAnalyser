namespace DataVisualiser.Class
{
    public static class BinaryOperators
    {
        public static readonly Func<double, double, double> Difference =
            (a, b) => a - b;

        public static readonly Func<double, double, double> Ratio =
            (a, b) => b == 0 ? double.NaN : a / b;

        public static readonly Func<double, double, double> Sum =
            (a, b) => a + b;

        public static readonly Func<double, double, double> Product =
            (a, b) => a * b;

        public static readonly Func<double, double, double> Average =
            (a, b) => (a + b) / 2.0;

        public static readonly Func<double, double, double> Max =
            (a, b) => Math.Max(a, b);

        public static readonly Func<double, double, double> Min =
            (a, b) => Math.Min(a, b);
    }

}
