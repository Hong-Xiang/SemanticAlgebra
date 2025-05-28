namespace LambdaLang.Tests.IntLang;

public class IntLangTests
{
    [Fact]
    public void IntLangFold_ReturnsExpectedResult()
    {
        // Arrange
        var sf = IntLang.SyntaxFactory;
        var fe = sf.LitI(40);
        var fadd = sf.Add(fe, fe);
        
        // Act
        var fv = fadd.Fold(new IntFolder());
        
        // Assert
        Assert.Equal(80, fv);
    }
}