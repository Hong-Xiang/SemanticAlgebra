using SemanticAlgebra;
using SemanticAlgebra.Option;
using Xunit;

namespace LambdaLang.Tests;

public class OptionTests
{
    [Fact]
    public void Select_WithSome_ReturnsTransformedValue()
    {
        // Arrange
        var s = Option.Some(40);
        
        // Act
        var e = s.Select(n => n + 2);
        
        // Assert
        // Verify that e is Option.Some containing 42
        var result = e.Evaluate(new OptionExtractSemantic<int>());
        Assert.Equal(42, result);
    }
    
    [Fact]
    public void ZipWith_WithTwoSomes_ReturnsCombinedValue()
    {
        // Arrange
        var s = Option.Some(40);
        var t = Option.Some(2);
        
        // Act
        var z = s.ZipWith(t, (a, b) => a * b);
        
        // Assert
        // Verify that z is Option.Some containing 80 (40 * 2)
        var result = z.Evaluate(new OptionExtractSemantic<int>());
        Assert.Equal(80, result);
    }
    
    [Fact]
    public void SelectMany_WithThreeSomes_ReturnsCombinedValue()
    {
        // Arrange & Act
        var x = from a in Option.Some(40)
                from b in Option.Some(2)
                from c in Option.Some(3)
                select a + b + c;
        
        // Assert
        // Verify that x is Option.Some containing 45 (40 + 2 + 3)
        var result = x.Evaluate(new OptionExtractSemantic<int>());
        Assert.Equal(45, result);
    }
    
    // Helper class to extract the value from Some, or throw an exception for None
    private class OptionExtractSemantic<T> : IOptionSemantic<T, T>
    {
        public T None() => throw new Xunit.Sdk.XunitException("Expected Some but got None");
        public T Some(T value) => value;
    }
}