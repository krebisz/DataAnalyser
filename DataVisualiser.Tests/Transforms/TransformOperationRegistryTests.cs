using DataVisualiser.Core.Transforms.Operations;

namespace DataVisualiser.Tests.Transforms;

public sealed class TransformOperationRegistryTests
{
    [Fact]
    public void GetOperation_ShouldReturnKnownOperations()
    {
        Assert.NotNull(TransformOperationRegistry.GetOperation("Log"));
        Assert.NotNull(TransformOperationRegistry.GetOperation("Sqrt"));
        Assert.NotNull(TransformOperationRegistry.GetOperation("Add"));
        Assert.NotNull(TransformOperationRegistry.GetOperation("Subtract"));
    }

    [Fact]
    public void GetUnaryOperations_ShouldContainLogAndSqrt()
    {
        var unary = TransformOperationRegistry.GetUnaryOperations();

        Assert.Contains(unary, o => o.Id == "Log");
        Assert.Contains(unary, o => o.Id == "Sqrt");
        Assert.All(unary, o => Assert.Equal(1, o.Arity));
    }

    [Fact]
    public void GetBinaryOperations_ShouldContainAddAndSubtract()
    {
        var binary = TransformOperationRegistry.GetBinaryOperations();

        Assert.Contains(binary, o => o.Id == "Add");
        Assert.Contains(binary, o => o.Id == "Subtract");
        Assert.All(binary, o => Assert.Equal(2, o.Arity));
    }

    [Fact]
    public void Register_ShouldThrow_WhenNullOrMissingId()
    {
        Assert.Throws<ArgumentException>(() => TransformOperationRegistry.Register(null!));
        Assert.Throws<ArgumentException>(() => TransformOperationRegistry.Register(new TransformOperation
        {
                Id = ""
        }));
    }

    [Fact]
    public void Register_ShouldOverrideOperationWithSameId()
    {
        var op = TransformOperation.Unary("Log", "Logarithm", x => 123.0);

        TransformOperationRegistry.Register(op);

        var fetched = TransformOperationRegistry.GetOperation("Log");
        Assert.NotNull(fetched);

        var v = fetched!.Execute(new[]
        {
                0.0
        });
        Assert.Equal(123.0, v);
    }

    [Fact]
    public void GetAllOperations_ShouldBeNonEmpty()
    {
        var all = TransformOperationRegistry.GetAllOperations();
        Assert.NotEmpty(all);
        Assert.True(all.Any(o => o.Id == "Add"));
    }
}