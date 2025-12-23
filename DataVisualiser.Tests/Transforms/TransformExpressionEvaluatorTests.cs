using System;
using System.Collections.Generic;
using System.Linq;
using DataVisualiser.Helper;
using DataVisualiser.Models;
using Xunit;

namespace DataVisualiser.Tests.Transforms
{
    public sealed class TransformExpressionEvaluatorTests
    {
        private static List<HealthMetricData> Series(params (DateTime ts, decimal? v)[] points)
            => points.Select(p => new HealthMetricData { NormalizedTimestamp = p.ts, Value = p.v, Unit = "u" }).ToList();

        [Fact]
        public void Evaluate_ShouldThrow_WhenExpressionNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                TransformExpressionEvaluator.Evaluate(null!, new List<IReadOnlyList<HealthMetricData>> { new List<HealthMetricData>() }));
        }

        [Fact]
        public void Evaluate_ShouldThrow_WhenMetricsNullOrEmpty()
        {
            var expr = TransformExpression.Metric(0);

            Assert.Throws<ArgumentException>(() => TransformExpressionEvaluator.Evaluate(expr, null!));
            Assert.Throws<ArgumentException>(() => TransformExpressionEvaluator.Evaluate(expr, new List<IReadOnlyList<HealthMetricData>>()));
        }

        [Fact]
        public void Evaluate_ShouldThrow_WhenMetricsNotAligned()
        {
            var expr = TransformExpression.Metric(0);

            var m0 = Series((new DateTime(2024, 1, 1), 1m));
            var m1 = Series((new DateTime(2024, 1, 1), 2m), (new DateTime(2024, 1, 2), 3m));

            Assert.Throws<ArgumentException>(() =>
                TransformExpressionEvaluator.Evaluate(expr, new List<IReadOnlyList<HealthMetricData>> { m0, m1 }));
        }

        [Fact]
        public void Evaluate_ShouldReturnLeafMetricValues()
        {
            var expr = TransformExpression.Metric(0);

            var m0 = Series(
                (new DateTime(2024, 1, 1), 10m),
                (new DateTime(2024, 1, 2), null),
                (new DateTime(2024, 1, 3), 30m));

            var res = TransformExpressionEvaluator.Evaluate(expr, new List<IReadOnlyList<HealthMetricData>> { m0 });

            Assert.Equal(3, res.Count);
            Assert.Equal(10.0, res[0]);
            Assert.True(double.IsNaN(res[1]));
            Assert.Equal(30.0, res[2]);
        }

        [Fact]
        public void Evaluate_ShouldExecuteUnaryOperation()
        {
            var op = TransformOperationRegistry.GetOperation("Sqrt");
            Assert.NotNull(op);

            var expr = TransformExpression.Unary(op!, TransformOperand.Metric(0));

            var m0 = Series(
                (new DateTime(2024, 1, 1), 9m),
                (new DateTime(2024, 1, 2), 16m));

            var res = TransformExpressionEvaluator.Evaluate(expr, new List<IReadOnlyList<HealthMetricData>> { m0 });

            Assert.Equal(2, res.Count);
            Assert.Equal(3.0, res[0], 10);
            Assert.Equal(4.0, res[1], 10);
        }

        [Fact]
        public void Evaluate_ShouldExecuteBinaryOperation()
        {
            var op = TransformOperationRegistry.GetOperation("Add");
            Assert.NotNull(op);

            var expr = TransformExpression.Binary(op!, TransformOperand.Metric(0), TransformOperand.Metric(1));

            var m0 = Series((new DateTime(2024, 1, 1), 10m), (new DateTime(2024, 1, 2), 20m));
            var m1 = Series((new DateTime(2024, 1, 1), 1m), (new DateTime(2024, 1, 2), 2m));

            var res = TransformExpressionEvaluator.Evaluate(expr, new List<IReadOnlyList<HealthMetricData>> { m0, m1 });

            Assert.Equal(new[] { 11.0, 22.0 }, res);
        }

        [Fact]
        public void Evaluate_ShouldSupportChainedExpression()
        {
            // log(A + B)
            var add = TransformOperationRegistry.GetOperation("Add");
            var log = TransformOperationRegistry.GetOperation("Log");
            Assert.NotNull(add);
            Assert.NotNull(log);

            var inner = TransformExpression.Binary(add!, TransformOperand.Metric(0), TransformOperand.Metric(1));
            var expr = TransformExpression.Unary(log!, TransformOperand.FromExpression(inner));

            var m0 = Series((new DateTime(2024, 1, 1), 9m));
            var m1 = Series((new DateTime(2024, 1, 1), 1m));

            var res = TransformExpressionEvaluator.Evaluate(expr, new List<IReadOnlyList<HealthMetricData>> { m0, m1 });

            Assert.Single(res);
            Assert.False(double.IsNaN(res[0]));
        }

        [Fact]
        public void GenerateLabel_ShouldReturnMetricLabel_ForLeaf()
        {
            var expr = TransformExpression.Metric(0);
            var label = TransformExpressionEvaluator.GenerateLabel(expr, new List<string> { "A" });

            Assert.Equal("A", label);
        }

        [Fact]
        public void GenerateLabel_ShouldIncludeOperationSymbols()
        {
            var add = TransformOperationRegistry.GetOperation("Add");
            Assert.NotNull(add);

            var expr = TransformExpression.Binary(add!, TransformOperand.Metric(0), TransformOperand.Metric(1));
            var label = TransformExpressionEvaluator.GenerateLabel(expr, new List<string> { "A", "B" });

            Assert.Contains("[Transform]", label);
            Assert.Contains("A", label);
            Assert.Contains("B", label);
            Assert.Contains("+", label);
        }

        [Fact]
        public void AlignMetricsByTimestamp_ShouldKeepOnlyCommonTimestamps()
        {
            var a = Series(
                (new DateTime(2024, 1, 1), 1m),
                (new DateTime(2024, 1, 2), 2m),
                (new DateTime(2024, 1, 3), 3m));

            var b = Series(
                (new DateTime(2024, 1, 2), 20m),
                (new DateTime(2024, 1, 3), 30m),
                (new DateTime(2024, 1, 4), 40m));

            var (a2, b2) = TransformExpressionEvaluator.AlignMetricsByTimestamp(a, b);

            Assert.Equal(2, a2.Count);
            Assert.Equal(2, b2.Count);
            Assert.Equal(new DateTime(2024, 1, 2), a2[0].NormalizedTimestamp);
            Assert.Equal(new DateTime(2024, 1, 3), a2[1].NormalizedTimestamp);
            Assert.Equal(new DateTime(2024, 1, 2), b2[0].NormalizedTimestamp);
            Assert.Equal(new DateTime(2024, 1, 3), b2[1].NormalizedTimestamp);
        }
    }
}
