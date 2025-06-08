using Xunit;
using Xunit.Abstractions;

namespace LambdaLang.Tests.IntLang;

public class IntLangTests(ITestOutputHelper Output)
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
        Output.WriteLine(sf.ToString());
    }

    [Fact]
    public void PushDownNegShouldWork()
    {
        var S = IntLang.SyntaxFactory;
        // 8 + (- (1 + 2) ) => 8 + (-1 + -2)
        var e0 = S.Add(S.LitI(8), S.Neg(S.Add(S.LitI(1), S.LitI(2))));
        var e1 = e0.UnfoldA(new PushNegTransform());
        Assert.Equal(S.Add(S.LitI(8), S.Add(S.Neg(S.LitI(1)), S.Neg(S.LitI(2)))), e1);
    }

    [Fact]
    public void PushDownNegFromTopShouldWork()
    {
        var S = IntLang.SyntaxFactory;
        // - (8 + (- (1 + 2) )) => (- 8) + (1 + 2)
        var e0 = S.Neg(S.Add(S.LitI(8), S.Neg(S.Add(S.LitI(1), S.LitI(2)))));
        var e1 = e0.UnfoldA(new PushNegTransform());
        Assert.Equal(S.Add(S.Neg(S.LitI(8)), S.Add(S.LitI(1), S.LitI(2))), e1);
    }
}