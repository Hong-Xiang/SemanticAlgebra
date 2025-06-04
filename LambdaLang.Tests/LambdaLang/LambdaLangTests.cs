using SemanticAlgebra.Data;
using SemanticAlgebra.Fix;
using System.Collections.Immutable;
using LambdaLang.Tests.LambdaLang.Language;
using SemanticAlgebra;

namespace LambdaLang.Tests.LambdaLang;

// encoding (ImmutableDictionary<Identifier, ISigValue> -> (ISigValue, ImmutableDictionary<Identifier, ISigValue>))
using EvalS = StateT<EvalState, ImmutableDictionary<Identifier, ISigValue>>;

sealed class EvalState :
    IMonadKState<EvalState, Identifier, ISigValue>
{
    public static class B
    {
        public static IS<EvalState, T> From<T>(
            Func<ImmutableDictionary<Identifier, ISigValue>,
                    (T, ImmutableDictionary<Identifier, ISigValue>)>
                value)
            => Alias1.B.From<EvalState, T,
                Func<ImmutableDictionary<Identifier, ISigValue>,
                    (T, ImmutableDictionary<Identifier, ISigValue>)>>(value);
    }

    public static ISemantic1<EvalState, T, IS<EvalState, T>> Id<T>()
        => Alias1.Id<EvalState, T,
            Func<ImmutableDictionary<Identifier, ISigValue>,
                (T, ImmutableDictionary<Identifier, ISigValue>)>
        >();

    public static ISemantic1<EvalState, TS, TR> Compose<TS, TI, TR>(ISemantic1<EvalState, TS, TI> s, Func<TI, TR> f)
        => Alias1.Compose<EvalState, TS, TI, TR,
                Func<ImmutableDictionary<Identifier, ISigValue>,
                    (TS, ImmutableDictionary<Identifier, ISigValue>)>>
            (s, f);

    public static ISemantic1<EvalState, TS, IS<EvalState, TR>> MapS<TS, TR>(Func<TS, TR> f)
        => Kind1K<EvalState>.Semantic<TS, IS<EvalState, TR>>(s => B.From(e =>
        {
            var (val, env) = s.Unwrap()(e);
            return (f(val), env);
        }));

    public static IS<EvalState, T> Pure<T>(T x)
        => B.From<T>(e => (x, e));

    public static ISemantic1<EvalState, IS<EvalState, T>, IS<EvalState, T>> JoinS<T>()
        => Kind1K<EvalState>.Semantic<IS<EvalState, T>, IS<EvalState, T>>(ms => B.From<T>(s =>
            {
                var (value, state) = ms.Unwrap()(s);
                return value.Unwrap()(state);
            })
        );

    public static IS<EvalState, ISigValue> Get(Identifier key)
        => B.From<ISigValue>(e => (e[key], e));

    public static IS<EvalState, Unit> Put(Identifier key, ISigValue value)
        => B.From(e => (Prelude.Unit, e.Add(key, value)));
}

static class EvalStateExtension
{
    public static Func<ImmutableDictionary<Identifier, ISigValue>,
        (T, ImmutableDictionary<Identifier, ISigValue>)> Unwrap<T>(this IS<EvalState, T> e)
        => ((Alias1.D.From<
            EvalState,
            T,
            Func<ImmutableDictionary<Identifier, ISigValue>, (T, ImmutableDictionary<Identifier, ISigValue>)>
        >)e).Value;
}

public class LambdaLangTests
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
    public void LambdaExpression_EvaluatesTo44()
    {
        // Arrange & Act
        var expr = BuildLambdaExpression();

        // var el = expr.Fold<Fix<SCore>>(new BindLoweringFolder());
        var S2 =
            Fix<SCore>.SyntaxFactory.Prj();
        var x = new Identifier("x");
        var ef = S2.Lambda(x, S2.Var(x));
        var el = S2.Apply(ef, S2.LitI(3));
        


        var vals = el.Fold<IS<EvalState, ISigValue>>(SCore.ComposeSemantic(
            new LitEvalFolder<EvalState>(),
            new ArithEvalFolder<EvalState>(),
            new LamEvalFolder<EvalState>(),
            new AppEvalFolder<EvalState>()));

        var (valr, _) = vals.Unwrap()(ImmutableDictionary<Identifier, ISigValue>.Empty);
        var result = ((SigInt)valr).Value;

        // Assert
        Assert.Equal(44, result);
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
        var succ = S.Lambda(n, S.Lambda(f2, S.Lambda(x3,
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