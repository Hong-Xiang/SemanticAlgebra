using SemanticAlgebra.Data;
using SemanticAlgebra.Fix;
using System.Collections.Immutable;
using LambdaLang.Tests.LambdaLang.Language;
using SemanticAlgebra;
using Xunit.Abstractions;

namespace LambdaLang.Tests.LambdaLang;

// encoding (ImmutableDictionary<Identifier, ISigValue> -> (ISigValue, ImmutableDictionary<Identifier, ISigValue>))
using EvalS = StateT<Identity, ImmutableDictionary<Identifier, ISigValue>>;

public class LambdaLangTests(ITestOutputHelper Output)
{
    [Fact]
    public void LambdaExpression_GeneratesStringRepresentation()
    {
        // Arrange & Act
        var expr = BuildLambdaExpression();

        var show = expr.Fold<string>(Sig.SigSemantic(
            new LitShowFolder(),
            new ArithShowFolder(),
            new LamShowFolder(),
            new AppShowFolder(),
            new BindShowFolder()));

        // Assert
        Assert.False(string.IsNullOrEmpty(show));
    }

    [Fact]
    public void LambdaExpressionEvalId()
    {
        var S = Fix<SCore>.SyntaxFactory.Prj();
        var x = new Identifier("x");
        var id = S.Lambda(x, S.Var(x));
        var e = S.Apply(id, S.LitI(42));
        var r = e.Fold<IS<EvalS, ISigValue>>(EvalFolder);
        var (rv, _) = r.Unwrap()(ImmutableDictionary<Identifier, ISigValue>.Empty);
        Assert.Equal(42, Assert.IsType<SigInt>(rv).Value);
    }

    [Fact]
    public void LambdaExpressionEvalAdd1()
    {
        var S = Fix<SCore>.SyntaxFactory.Prj();
        var x2 = new Identifier("x2");
        var add1 = S.Lambda(x2, S.Add(S.LitI(1), S.Var(x2)));

        var e = S.Apply(add1, S.LitI(41));
        var r = e.Fold<IS<EvalS, ISigValue>>(EvalFolder);
        var (rv, _) = r.Unwrap()(ImmutableDictionary<Identifier, ISigValue>.Empty);
        Assert.Equal(42, Assert.IsType<SigInt>(rv).Value);
    }

    [Fact]
    public void LambdaExpressionEvalZAdd1()
    {
        var S = Fix<SCore>.SyntaxFactory.Prj();
        var x = new Identifier("x");
        var id = S.Lambda(x, S.Var(x));
        var x2 = new Identifier("x2");
        var add1 = S.Lambda(x2, S.Add(S.LitI(1), S.Var(x2)));
        var f = new Identifier("f");

        var z = S.Lambda(f, id);
        var e = S.Apply(S.Apply(z, add1), S.LitI(42));
        var r = e.Fold<IS<EvalS, ISigValue>>(EvalFolder);
        var (rv, _) = r.Unwrap()(ImmutableDictionary<Identifier, ISigValue>.Empty);
        Assert.Equal(42, Assert.IsType<SigInt>(rv).Value);
    }


    [Fact]
    public void LambdaExpressionEvalSimpleApp()
    {
        var S = Fix<SCore>.SyntaxFactory.Prj();
        var x = new Identifier("x");
        var id = S.Lambda(x, S.Var(x));
        var f2 = new Identifier("f2");
        var x3 = new Identifier("x3");
        var app = S.Lambda(f2, S.Lambda(x3, S.Apply(S.Var(f2), S.Var(x3))));
        var e0 = S.Apply(app, id);
        var r0 = e0.Fold<IS<EvalS, ISigValue>>(EvalFolder);

        var e = S.Apply(S.Apply(app, id), S.LitI(42));
        Output.WriteLine(e.Show());
        var r = e.Fold<IS<EvalS, ISigValue>>(EvalFolder);
        var (rv, _) = r.Unwrap()(ImmutableDictionary<Identifier, ISigValue>.Empty);
        Assert.Equal(42, Assert.IsType<SigInt>(rv).Value);
    }

    private SCore.ISemantic<IS<EvalS, ISigValue>, IS<EvalS, ISigValue>> EvalFolder
        => SCore.MergeSemantic(
            new LitEvalFolder<EvalS>(),
            new ArithEvalFolder<EvalS>(),
            new LamEvalFolder<EvalS>(),
            new AppEvalFolder<EvalS>());


    [Fact]
    public void ISigValueStateTypeShouldWork()
    {
        // the following should be compiler error
        // IS<EvalS, ISigValue> r = EvalS.Pure(new SigInt(10)); 

        IS<EvalS, ISigValue> r = EvalS.Pure<ISigValue>(new SigInt(10));
        var (v, _) = r.Unwrap()(ImmutableDictionary<Identifier, ISigValue>.Empty);
        Assert.Equal(10, Assert.IsType<SigInt>(v).Value);
    }

    [Fact]
    public void LambdaExpression_EvaluatesTo44()
    {
        // Arrange & Act
        var expr = BuildLambdaExpression();

        var el = expr.Fold<Fix<SCore>>(new BindLoweringFolder());
        var S2 =
            Fix<SCore>.SyntaxFactory.Prj();
        var x = new Identifier("x");
        var ef = S2.Lambda(x, S2.Var(x));
        // var el = S2.Apply(ef, S2.LitI(3));


        var vals = el.Fold<IS<EvalS, ISigValue>>(EvalFolder);

        var (valr, _) = vals.Unwrap()(ImmutableDictionary<Identifier, ISigValue>.Empty);
        var result = ((SigInt)valr).Value;

        // Assert
        Assert.Equal(44, result);
    }

    [Fact]
    public void LambdaExpressionWithBindEvaluatesTo44()
    {
        // Arrange & Act
        var expr = BuildLambdaExpression();
        var folder = Sig.SigSemantic(
            new LitEvalFolder<EvalS>(),
            new ArithEvalFolder<EvalS>(),
            new LamEvalFolder<EvalS>(),
            new AppEvalFolder<EvalS>(),
            new BindEvalFolder<EvalS>()
        );

        // var el = expr.Fold<Fix<SCore>>(new BindLoweringFolder());
        // var S2 =
        //     Fix<SCore>.SyntaxFactory.Prj();
        // var x = new Identifier("x");
        // var ef = S2.Lambda(x, S2.Var(x));
        // // var el = S2.Apply(ef, S2.LitI(3));


        var vals = expr.Fold<IS<EvalS, ISigValue>>(folder);

        var (valr, _) = vals.Unwrap()(ImmutableDictionary<Identifier, ISigValue>.Empty);
        Assert.Equal(new SigInt(44), valr);
    }

    private Fix<Sig> BuildLambdaExpression()
    {
        var S =
            Fix<Sig>.SyntaxFactory.Prj();

        var x = new Identifier("x");
        var add1 = S.Lambda(x, S.Add(S.LitI(1), S.Var(x)));

        var f = new Identifier("f");
        var x2 = new Identifier("x2");
        var z = S.Lambda(f, S.Lambda(x2, S.Var(x2)));

        var f2 = new Identifier("f2");
        var n = new Identifier("n");
        var x3 = new Identifier("x3");
        var succ = S.Lambda(n,
            S.Lambda(f2,
                S.Lambda(x3,
                    S.Apply(S.Var(f2),
                        S.Apply(
                            S.Apply(S.Var(n), S.Var(f2)),
                            S.Var(x3)))
                )));

        var c1 = S.Apply(succ, z);
        var c2 = S.Apply(succ, c1);
        var c3 = S.Apply(succ, c2);

        var l41 = S.LitI(41);

        var expr = S.Apply(S.Apply(c3, add1), l41);

        return expr;
    }
}