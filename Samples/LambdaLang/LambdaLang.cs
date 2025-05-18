using SemanticAlgebra;
using SemanticAlgebra.Data;
using SemanticAlgebra.Fix;

namespace LambdaLang;

sealed class Lambda<V> : IFunctor<Lambda<V>>
{
    public static IDiSemantic<Lambda<V>, T, T> Id<T>()
        => new LambdaIdSemantic<V, T>().AsDiSemantic();

    public static ISemantic1<Lambda<V>, TS, TR> ComposeF<TS, TI, TR>(ISemantic1<Lambda<V>, TS, TI> s, Func<TI, TR> f)
        => new LambdaComposeFSemantic<V, TS, TI, TR>(s.Prj(), f);

    public static IDiSemantic<Lambda<V>, TS, TR> Map<TS, TR>(Func<TS, TR> f)
    {
        throw new NotImplementedException();
    }
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

sealed class LambdaP : IFunctor<LambdaP>
{
    public static IDiSemantic<LambdaP, T, T> Id<T>()
    {
        throw new NotImplementedException();
    }

    public static ISemantic1<LambdaP, TS, TR> ComposeF<TS, TI, TR>(ISemantic1<LambdaP, TS, TI> s, Func<TI, TR> f)
    {
        throw new NotImplementedException();
    }

    public static IDiSemantic<LambdaP, TS, TR> Map<TS, TR>(Func<TS, TR> f)
    {
        throw new NotImplementedException();
    }
}

static class LambdaPExtension
{
    public static ILambdaPSemantic<TI, TO> Prj<TI, TO>(
        this ISemantic1<LambdaP, TI, TO> s
    ) => (ILambdaPSemantic<TI, TO>)s;
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

sealed record class Var<TV, TE>(TV V) : IS<Lambda<TV>, TE>
{
    public TR Evaluate<TR>(ISemantic1<Lambda<TV>, TE, TR> semantic)
        => semantic.Prj().Var(V);
}

sealed record class App<TV, TE>(TE F, TE X)
    : IS<Lambda<TV>, TE>
{
    public TR Evaluate<TR>(ISemantic1<Lambda<TV>, TE, TR> semantic)
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