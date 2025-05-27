using LambdaLang;
using Xunit;

namespace LambdaLang.Tests;

public class LambdaLangTests
{
    [Fact]
    public void LambdaExpressionEvaluation_ReturnsExpectedValues()
    {
        // Act: Call the expression evaluation method
        var result = LambdaExpressionEvaluator.EvaluateExpression();
        
        // Assert: Check that the string representation is not empty
        Assert.False(string.IsNullOrEmpty(result.ShowResult));
        
        // Assert: Check that the evaluation result is 44 (41 + 3)
        Assert.Equal(44, result.EvaluationResult);
    }
}