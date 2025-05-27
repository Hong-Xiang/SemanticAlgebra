using SemanticAlgebra;
using SemanticAlgebra.Data;
using SemanticAlgebra.Fix;

namespace LambdaLang;

// implementing a small language with compositional signature
// target
// e ::= \x.e | x | e1 e2 | n | e1 + e2 | let x = e1 in e2 | error


// for binding, although the PHOAS style implementation is possible,
// after investigation it would be hard to use it fluently in C#.
// thus we are going to use first-order encoding for variable binding.



sealed class Lambda<V> : IFunctor<Lambda<V>>
{
    public static ISemantic1<Lambda<V>, T, IS<Lambda<V>, T>> Id<T>()
        => new LambdaIdSemantic<V, T>();

    public static ISemantic1<Lambda<V>, TS, TR> Compose<TS, TI, TR>(ISemantic1<Lambda<V>, TS, TI> s, Func<TI, TR> f)
        => new LambdaComposeFSemantic<V, TS, TI, TR>(s.Prj(), f);

    public static ISemantic1<Lambda<V>, TS, IS<Lambda<V>, TR>> MapS<TS, TR>(Func<TS, TR> f)
        => new LambdaMapSemantic<V, TS, TR>(f);
}

interface ILambdaLang<TV, in TEI, out TEO>
    : ISemantic1<Lambda<TV>, TEI, TEO>
{
    TEO Var(TV x);
    TEO App(TEI f, TEI x);
    TEO Lam(Func<TV, TEI> body);
}

interface ILambdaPSemantic<in TI, out TO>
{
    TO Get(ITerm<TI> termV);
}

interface ITerm<out T>
{
    IS<Lambda<TV>, T> Get<TV>();
}

static class LambdaExtension
{
    public static ILambdaLang<V, TS, TR> Prj<V, TS, TR>(
        this ISemantic1<Lambda<V>, TS, TR> s
    ) => (ILambdaLang<V, TS, TR>)s;
}

sealed record class Var<TV, TE>(TV V)
    : IS<Lambda<TV>, TE>
{
    public TR Evaluate<TR>(ISemantic1<Lambda<TV>, TE, TR> semantic)
        => semantic.Prj().Var(V);
}

sealed record class App<TV, TE>(TE F, TE X)
    : IS<Lambda<TV>, TE>
    , IS<LambdaC, TE>
{
    public TR Evaluate<TR>(ISemantic1<Lambda<TV>, TE, TR> semantic)
        => semantic.Prj().App(F, X);

    public TR Evaluate<TR>(ISemantic1<LambdaC, TE, TR> semantic)
        => semantic.Prj().App(F, X);
}

sealed record class Lam<TV, TE>(Func<TV, TE> L) : IS<Lambda<TV>, TE>
{
    public TR Evaluate<TR>(ISemantic1<Lambda<TV>, TE, TR> semantic)
        => semantic.Prj().Lam(L);
}

sealed class LambdaIdSemantic<TV, T> : ILambdaLang<TV, T, IS<Lambda<TV>, T>>
{
    public IS<Lambda<TV>, T> Var(TV x)
        => new Var<TV, T>(x);

    public IS<Lambda<TV>, T> App(T f, T x)
        => new App<TV, T>(f, x);

    public IS<Lambda<TV>, T> Lam(Func<TV, T> body)
        => new Lam<TV, T>(body);
}

sealed class LambdaMapSemantic<TV, TS, TR>(Func<TS, TR> F)
    : ILambdaLang<TV, TS, IS<Lambda<TV>, TR>>
{
    public IS<Lambda<TV>, TR> Var(TV x)
        => new Var<TV, TR>(x);

    public IS<Lambda<TV>, TR> App(TS f, TS x)
        => new App<TV, TR>(F(f), F(x));

    public IS<Lambda<TV>, TR> Lam(Func<TV, TS> body)
        => new Lam<TV, TR>(v => F(body(v)));
}

sealed class LambdaComposeFSemantic<TV, TS, TI, TR>(
    ILambdaLang<TV, TS, TI> s,
    Func<TI, TR> f
) : ILambdaLang<TV, TS, TR>
{
    public TR Var(TV x)
        => f(s.Var(x));

    public TR App(TS f_, TS x)
        => f(s.App(f_, x));

    public TR Lam(Func<TV, TS> body)
        => f(s.Lam(body));
}

interface ILambdaExpr
{
    Fix<Lambda<T>> Build<T>(ILambdaLang<T, Fix<Lambda<T>>, Fix<Lambda<T>>> builder);
}

static class LambdaTest
{
    public static ILambdaLang<T, Fix<Lambda<T>>, Fix<Lambda<T>>> SyntaxFactory<T>()
        => Fix<Lambda<T>>.SyntaxFactory.Prj();

    static Fix<Lambda<T>> Id<T>()
    {
        var s = SyntaxFactory<T>();
        return s.Lam(x => s.Var(x));
    }
}

// (a b)
// \x.x
// 

sealed class LambdaC : IFunctor<LambdaC>
{
    public static IS<LambdaC, T> Var<T>(string v) => new VarC<T>(v);
    public static IS<LambdaC, T> App<T>(T f, T x) => new App<string, T>(f, x);
    public static IS<LambdaC, T> Lam<T>(string v, T b) => new LamC<T>(v, b);

    public static ISemantic1<LambdaC, TS, TR> Compose<TS, TI, TR>(ISemantic1<LambdaC, TS, TI> s, Func<TI, TR> f)
    {
        throw new NotImplementedException();
    }

    public static ISemantic1<LambdaC, T, IS<LambdaC, T>> Id<T>()
    {
        throw new NotImplementedException();
    }

    public static ISemantic1<LambdaC, TS, IS<LambdaC, TR>> MapS<TS, TR>(Func<TS, TR> f)
    {
        throw new NotImplementedException();
    }
}

sealed class VarC<T>(string Name) : IS<LambdaC, T>
{
    public TR Evaluate<TR>(ISemantic1<LambdaC, T, TR> semantic)
        => semantic.Prj().Var(Name);
}
sealed class LamC<T>(string V, T B) : IS<LambdaC, T>
{
    public TR Evaluate<TR>(ISemantic1<LambdaC, T, TR> semantic)
        => semantic.Prj().Lam(V, B);
}

static class LambdaCExtension
{
    public static ILambdaCSemantic<TS, TR> Prj<TS, TR>(this ISemantic1<LambdaC, TS, TR> s)
        => (ILambdaCSemantic<TS, TR>)s;
}

interface ILambdaCSemantic<in TS, out TR> : ISemantic1<LambdaC, TS, TR>
{
    TR Var(string v);
    TR App(TS f, TS x);
    TR Lam(string v, TS b);
}
sealed class LambdaCIdSemantic<T> : ILambdaCSemantic<T, IS<LambdaC, T>>
{
    public IS<LambdaC, T> App(T f, T x)
    {
        throw new NotImplementedException();
    }

    public IS<LambdaC, T> Lam(string v, T b)
    {
        throw new NotImplementedException();
    }

    public IS<LambdaC, T> Var(string v)
    {
        throw new NotImplementedException();
    }
}