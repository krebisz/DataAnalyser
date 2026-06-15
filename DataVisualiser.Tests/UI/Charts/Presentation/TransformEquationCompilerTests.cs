using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.Tests.UI.Charts.Presentation;

public sealed class TransformEquationCompilerTests
{
    [Fact]
    public void Compile_ShouldCreateSequentialBinarySteps()
    {
        var result = TransformEquationCompiler.Compile(
            [
                new TransformEquationTerm("None", "None", 0, "Fat"),
                new TransformEquationTerm("Add", "Add", 1, "Muscle"),
                new TransformEquationTerm("Divide", "Divide", 2, "Bone")
            ],
            inputCount: 3);

        Assert.True(result.IsValid);
        Assert.Equal("Fat + Muscle / Bone", result.Title);
        Assert.Equal(2, result.Steps.Count);
        Assert.Equal(SeriesOperationKind.Sum, result.Steps[0].Operation.Kind);
        Assert.Equal([0, 1], result.Steps[0].Operation.InputIndexes);
        Assert.Equal(SeriesOperationKind.Ratio, result.Steps[1].Operation.Kind);
        Assert.Equal([3, 2], result.Steps[1].Operation.InputIndexes);
    }

    [Fact]
    public void Compile_ShouldCreateUnaryFirstStep()
    {
        var result = TransformEquationCompiler.Compile(
            [new TransformEquationTerm("Sqrt", "Square Root", 0, "Fat")],
            inputCount: 1);

        Assert.True(result.IsValid);
        Assert.Equal("Sqrt(Fat)", result.Title);
        var step = Assert.Single(result.Steps);
        Assert.Equal(SeriesOperationKind.SquareRoot, step.Operation.Kind);
        Assert.Equal([0], step.Operation.InputIndexes);
    }

    [Fact]
    public void Compile_ShouldRejectUnaryOperationAfterFirstTerm()
    {
        var result = TransformEquationCompiler.Compile(
            [
                new TransformEquationTerm("None", "None", 0, "Fat"),
                new TransformEquationTerm("Log", "Logarithm", 1, "Muscle")
            ],
            inputCount: 2);

        Assert.False(result.IsValid);
        Assert.Contains("binary operation", result.Error);
    }

    [Fact]
    public void BuildExpression_ShouldFormatGeneratedEquation()
    {
        var expression = TransformEquationCompiler.BuildExpression(
            [
                new TransformEquationTerm("None", "None", 0, "Fat"),
                new TransformEquationTerm("Add", "Add", 1, "Muscle"),
                new TransformEquationTerm("Divide", "Divide", 2, "Bone")
            ]);

        Assert.Equal("((Fat) + Muscle) / Bone", expression);
    }

    [Fact]
    public void BuildExpression_ShouldPreferExpressionLabelsWithoutChangingCompiledLabels()
    {
        var terms = new[]
        {
            new TransformEquationTerm("None", "None", 0, "Weight - Fat Mass", "S\u2081"),
            new TransformEquationTerm("Add", "Add", 1, "Weight - Fat Free Mass", "S\u2082")
        };

        var expression = TransformEquationCompiler.BuildExpression(terms);
        var compiled = TransformEquationCompiler.Compile(terms, inputCount: 2);

        Assert.Equal("(S\u2081) + S\u2082", expression);
        Assert.True(compiled.IsValid);
        Assert.Equal("Weight - Fat Mass + Weight - Fat Free Mass", compiled.Title);
    }
}
