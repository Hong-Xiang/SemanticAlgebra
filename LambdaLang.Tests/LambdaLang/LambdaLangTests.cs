using SemanticAlgebra.Data;
using SemanticAlgebra.Fix;
using System.Collections.Immutable;
using LambdaLang.Tests.LambdaLang.Language;
using SemanticAlgebra;
using Xunit.Abstractions;
using Xunit;
using SemanticAlgebra.Free;

namespace LambdaLang.Tests.LambdaLang;

using EvalS = StateT<Identity, ImmutableDictionary<Identifier, ISigValue>>;

public class LambdaLangTests(ITestOutputHelper Output)
{
    [Fact]
    public void LambdaExpression_GeneratesStringRepresentation()
    {
        var expr = BuildLambdaExpression();

        var show = ShowAlgebra.Fold(expr);

        Output.WriteLine(show);

        Assert.False(string.IsNullOrEmpty(show));
    }

    [Fact]
    public void IfThenElseShouldWork()
    {
        var S = Fix<Sig>.SyntaxFactory.Prj();
        var e = S.If(S.Eq(S.LitI(1), S.LitI(2)), S.LitI(3), S.LitI(4));
        var v = e.MonadicFolder().Fold<EvalS>();
        var r = v.Run(ImmutableDictionary<Identifier, ISigValue>.Empty);
        Assert.Equal(new SigInt(4), r.Value);
    }

    [Fact]
    public void LazyRefTest()
    {
        object? c = null;
        var lz = new Lazy<object>(() => c);
        c = new SigInt(10);
        Assert.Equal(new SigInt(10), lz.Value);
    }

    [Fact]
    public void FacTest()
    {
        var S = Fix<Sig>.SyntaxFactory.Prj();
        var f = new Identifier("fac");
        var n = new Identifier("n");
        var b = S.Lambda(
            n,
            S.If(
                S.Eq(S.Var(n), S.LitI(0)),
                S.LitI(1),
                S.Mul(
                    S.Var(n),
                    S.Apply(
                        S.Var(f),
                        S.Sub(S.Var(n), S.LitI(1))))
            )
        );
        var d = S.LetRec(f, b, S.Var(f));
        {
            var e = S.LetRec(f, b, S.Apply(S.Var(f), S.LitI(0)));
            var v = e.MonadicFolder().Fold<EvalS>();
            var r = v.Run(ImmutableDictionary<Identifier, ISigValue>.Empty);
            Assert.Equal(new SigInt(1), r.Value);
        }
        {
            var e = S.LetRec(f, b, S.Apply(S.Var(f), S.LitI(2)));
            var v = e.MonadicFolder().Fold<EvalS>();
            var r = v.Run(ImmutableDictionary<Identifier, ISigValue>.Empty);
            Assert.Equal(new SigInt(2), r.Value);
        }
        {
            var e = S.LetRec(f, b, S.Apply(S.Var(f), S.LitI(3)));
            var v = e.MonadicFolder().Fold<EvalS>();
            var r = v.Run(ImmutableDictionary<Identifier, ISigValue>.Empty);
            Assert.Equal(new SigInt(6), r.Value);
        }
    }

    [Fact]
    public void MergeSemanticShouldWork()
    {
        var expr = Lit.B.LitI<Fix<Sig>>(10);
        var e = expr.Evaluate(Kind1K<Sig>.Id<Fix<Sig>>());
    }

    [Fact]
    public void LambdaExpressionEvalId()
    {
        var S = Fix<Sig>.SyntaxFactory.Prj();
        var x = new Identifier("x");
        var id = S.Lambda(x, S.Var(x));
        var e = S.Apply(id, S.LitI(42));
        var v = e.MonadicFolder().Fold<EvalS>();
        var r = v.Run(ImmutableDictionary<Identifier, ISigValue>.Empty);
        Assert.Equal(new SigInt(42), r.Value);
    }

    [Fact]
    public void LambdaExpressionEvalAdd1()
    {
        var S = Fix<Sig>.SyntaxFactory.Prj();
        var x2 = new Identifier("x2");
        var add1 = S.Lambda(x2, S.Add(S.LitI(1), S.Var(x2)));

        var e = S.Apply(add1, S.LitI(41));
        var v = e.MonadicFolder().Fold<EvalS>();
        var r = v.Run(ImmutableDictionary<Identifier, ISigValue>.Empty);
        Assert.Equal(new SigInt(42), r.Value);
    }

    [Fact]
    public void LambdaExpressionEvalZAdd1()
    {
        var S = Fix<Sig>.SyntaxFactory.Prj();
        var x = new Identifier("x");
        var id = S.Lambda(x, S.Var(x));
        var x2 = new Identifier("x2");
        var add1 = S.Lambda(x2, S.Add(S.LitI(1), S.Var(x2)));
        var f = new Identifier("f");

        var z = S.Lambda(f, id);
        var e = S.Apply(S.Apply(z, add1), S.LitI(42));
        var v = e.MonadicFolder().Fold<EvalS>();
        var r = v.Run(ImmutableDictionary<Identifier, ISigValue>.Empty);
        Assert.Equal(new SigInt(42), r.Value);
    }


    [Fact]
    public void LambdaExpressionEvalSimpleApp()
    {
        var S = Fix<Sig>.SyntaxFactory.Prj();
        var x = new Identifier("x");
        var id = S.Lambda(x, S.Var(x));
        var f2 = new Identifier("f2");
        var x3 = new Identifier("x3");
        var app = S.Lambda(f2, S.Lambda(x3, S.Apply(S.Var(f2), S.Var(x3))));
        var e0 = S.Apply(app, id);
        var e = S.Apply(e0, S.LitI(42));
        var v = e.MonadicFolder().Fold<EvalS>();
        var r = v.Run(ImmutableDictionary<Identifier, ISigValue>.Empty);
        Assert.Equal(new SigInt(42), r.Value);
    }

    private SCore.IMeragedSemantic<IS<EvalS, ISigValue>, IS<EvalS, ISigValue>> EvalFolder
        => SCore.CreateMergeSemantic(
            new LitEvalFolder<EvalS>(),
            new ArithEvalFolder<EvalS>(),
            new LamEvalFolder<EvalS>(),
            new AppEvalFolder<EvalS>(),
            new CondEvalFolder<EvalS>());


    [Fact]
    public void ISigValueStateTypeShouldWork()
    {
        // the following should be compiler error
        // IS<EvalS, ISigValue> r = EvalS.Pure(new SigInt(10)); 

        IS<EvalS, ISigValue> r = EvalS.Pure<ISigValue>(new SigInt(10));
        var (v, _) = r.Unwrap()(ImmutableDictionary<Identifier, ISigValue>.Empty).Unwrap();
        Assert.Equal(10, Assert.IsType<SigInt>(v).Value);
    }

    [Fact]
    public void LambdaExpression_EvaluatesTo44()
    {
        var expr = BuildLambdaExpression();

        var el = expr.Fold<Fix<SCore>>(new BindLoweringFolder());

        var v = el.Fold<IS<EvalS, ISigValue>>(EvalFolder);

        var (r, _) = v.Run(ImmutableDictionary<Identifier, ISigValue>.Empty);

        Assert.Equal(new SigInt(44), r);
    }

    [Fact]
    public void LambdaExpressionWithBindEvaluatesTo44()
    {
        var expr = BuildLambdaExpression();

        var v = expr.MonadicFolder().Fold<EvalS>();
        var r = v.Run(ImmutableDictionary<Identifier, ISigValue>.Empty);

        Assert.Equal(new SigInt(44), r.Value);
    }

    [Fact]
    public void LambdaExpressionFreeFolderShouldWork()
    {
        var expr = BuildLambdaExpression();

        var v = expr.Fold(EvalAlgebraK<Sig>.GetFree());
        var r = v.Interp(new EvalFToStateTNaturalTransform());
        var (rv, _) = r.Run(ImmutableDictionary<Identifier, ISigValue>.Empty);
        Assert.Equal(new SigInt(44), rv);
    }    [Fact]
    public void LambdaExpressionEvalIdUsingFree()
    {
        var S = Fix<Sig>.SyntaxFactory.Prj();
        var x = new Identifier("x");
        var id = S.Lambda(x, S.Var(x));
        var e = S.Apply(id, S.LitI(42));

        var v = e.Fold(EvalAlgebraK<Sig>.GetFree());
        var r = v.Interp(new EvalFToStateTNaturalTransform());
        var (rv, _) = r.Run(ImmutableDictionary<Identifier, ISigValue>.Empty);
        Assert.Equal(new SigInt(42), rv);
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