using SemanticAlgebra;
using SemanticAlgebra.Option;
using Xunit;

namespace LambdaLang.Tests;

public class OptionTests
{    [Fact]
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
}