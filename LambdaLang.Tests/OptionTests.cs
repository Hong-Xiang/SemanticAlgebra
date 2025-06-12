using SemanticAlgebra;
using SemanticAlgebra.Data;
using SemanticAlgebra.Free;
using SemanticAlgebra.Option;
using Xunit;

namespace LambdaLang.Tests;

public class OptionTests
{
    [Fact]
    public void Select_WithSome_ReturnsTransformedValue()
    {
        // Arrange
        var s = Option.B.Some(40);

        // Act
        var e = s.Select(n => n + 2);

        // Assert
        Assert.Equal(Option.B.Some(42), e);
    }
    [Fact]
    public void ZipWith_WithTwoSomes_ReturnsCombinedValue()
    {
        // Arrange
        var s = Option.B.Some(40);
        var t = Option.B.Some(2);

        // Act
        var z = s.ZipWith(t, (a, b) => a * b);

        // Assert
        Assert.Equal(Option.B.Some(80), z);
    }
    [Fact]
    public void SelectMany_WithThreeSomes_ReturnsCombinedValue()
    {
        // Arrange & Act
        var x = from a in Option.B.Some(40)
                from b in Option.B.Some(2)
                from c in Option.B.Some(3)
                select a + b + c;

        // Assert
        Assert.Equal(Option.B.Some(45), x);
    }

    sealed class FreeConstantToOption
        : INaturalTransform<Constant<Unit>, Option>
    {
        public IS<Option, T> Invoke<T>(IS<Constant<Unit>, T> e)
            => Option.B.None<T>();
    }

    [Fact]
    public void FromConstShouldWorkForAllSome()
    {
        // Arrange & Act
        var x = from a in Free<Constant<Unit>>.B.Pure(40)
                from b in Free<Constant<Unit>>.B.Pure(2)
                from c in Free<Constant<Unit>>.B.Pure(3)
                select a + b + c;
        var v = x.Interp(new FreeConstantToOption());

        // Assert
        Assert.Equal(Option.B.Some(45), v);
    }

    [Fact]
    public void FromConstShouldWorkForCaseWithNone()
    {
        // Arrange & Act
        var x = from a in Free<Constant<Unit>>.B.Pure(40)
                from b in Constant<Unit>.B.From<int>(Unit.Default).LiftF()
                from c in Free<Constant<Unit>>.B.Pure(3)
                select a + b + c;
        var v = x.Interp(new FreeConstantToOption());

        // Assert
        Assert.Equal(Option.B.None<int>(), v);
    }
}