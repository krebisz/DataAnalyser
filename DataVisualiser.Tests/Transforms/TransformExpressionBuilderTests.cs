using DataVisualiser.Core.Transforms.Expressions;
using DataVisualiser.Shared.Models;
using Xunit;

namespace DataVisualiser.Tests.Transforms
{
    public sealed class TransformExpressionBuilderTests
    {
        [Fact]
        public void BuildFromOperation_ShouldReturnNull_WhenOperationUnknown()
        {
            var expr = TransformExpressionBuilder.BuildFromOperation("Nope", 0);
            Assert.Null(expr);
        }

        [Fact]
        public void BuildFromOperation_ShouldReturnNull_WhenArityMismatch()
        {
            // Add is binary; providing 1 index should fail
            var expr = TransformExpressionBuilder.BuildFromOperation("Add", 0);
            Assert.Null(expr);
        }

        [Fact]
        public void BuildFromOperation_ShouldBuildUnary()
        {
            var expr = TransformExpressionBuilder.BuildFromOperation("Log", 0);

            Assert.NotNull(expr);
            Assert.NotNull(expr!.Operation);
            Assert.Equal("Log", expr.Operation!.Id);
            Assert.Single(expr.Operands);
            Assert.Equal(0, expr.Operands[0].MetricIndex);
        }

        [Fact]
        public void BuildFromOperation_ShouldBuildBinary()
        {
            var expr = TransformExpressionBuilder.BuildFromOperation("Subtract", 0, 1);

            Assert.NotNull(expr);
            Assert.NotNull(expr!.Operation);
            Assert.Equal("Subtract", expr.Operation!.Id);
            Assert.Equal(2, expr.Operands.Count);
            Assert.Equal(0, expr.Operands[0].MetricIndex);
            Assert.Equal(1, expr.Operands[1].MetricIndex);
        }

        [Fact]
        public void BuildChained_ShouldReturnNull_WhenInnerFails()
        {
            var expr = TransformExpressionBuilder.BuildChained("Log", "Nope", 0, 1);
            Assert.Null(expr);
        }

        [Fact]
        public void BuildChained_ShouldReturnNull_WhenOuterNotUnary()
        {
            var expr = TransformExpressionBuilder.BuildChained("Add", "Add", 0, 1);
            Assert.Null(expr);
        }

        [Fact]
        public void BuildChained_ShouldBuildUnaryAppliedToInnerExpression()
        {
            var expr = TransformExpressionBuilder.BuildChained("Log", "Add", 0, 1);

            Assert.NotNull(expr);
            Assert.NotNull(expr!.Operation);
            Assert.Equal("Log", expr.Operation!.Id);
            Assert.Single(expr.Operands);
            Assert.NotNull(expr.Operands[0].Expression);
            Assert.NotNull(expr.Operands[0].Expression!.Operation);
            Assert.Equal("Add", expr.Operands[0].Expression!.Operation!.Id);
        }

        [Fact]
        public void BuildNary_ShouldReturnNull_WhenOperationUnknown()
        {
            var expr = TransformExpressionBuilder.BuildNary("Nope", 0, 1, 2);
            Assert.Null(expr);
        }

        [Fact]
        public void BuildNary_ShouldBuildExpression_ForKnownOperation()
        {
            // Current registry has Add (arity 2) but CreateOperation allows any operand count.
            var expr = TransformExpressionBuilder.BuildNary("Add", 0, 1);

            Assert.NotNull(expr);
            Assert.NotNull(expr!.Operation);
            Assert.Equal("Add", expr.Operation!.Id);
            Assert.Equal(2, expr.Operands.Count);
        }
    }
}
