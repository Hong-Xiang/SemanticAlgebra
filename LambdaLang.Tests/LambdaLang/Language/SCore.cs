using SemanticAlgebra;
using SemanticAlgebra.Data;
using SemanticAlgebra.Fix;

namespace LambdaLang.Tests.LambdaLang.Language;

public interface SCore
    : IFunctor<SCore>
    , IMergedSemantic1<SCore, Lit, Arith, Lam, App, Cond>
    , Lit, Arith, Lam, App, Cond
{
    static ISemantic1<SCore, TS, TR> IMergedSemantic1<SCore, Lit, Arith, Lam, App, Cond>.MergeSemantic<TS, TR>(
        ISemantic1<Lit, TS, TR> Lit,
        ISemantic1<Arith, TS, TR> Arith,
        ISemantic1<Lam, TS, TR> Lam,
        ISemantic1<App, TS, TR> App,
        ISemantic1<Cond, TS, TR> Cond
    ) => CreateMergeSemantic(Lit, Arith, Lam, App, Cond);

    static IMeragedSemantic<TS, TR> CreateMergeSemantic<TS, TR>(
        ISemantic1<Lit, TS, TR> Lit,
        ISemantic1<Arith, TS, TR> Arith,
        ISemantic1<Lam, TS, TR> Lam,
        ISemantic1<App, TS, TR> App,
        ISemantic1<Cond, TS, TR> Cond
    )
        => new SCoreSemantic<TS, TR>(Lit.Prj(), Arith.Prj(), Lam.Prj(), App.Prj(), Cond.Prj());


    public interface IMeragedSemantic<in TI, out TO>
        : ISemantic1<SCore, TI, TO>
        , Lit.ISemantic<TI, TO>
        , Arith.ISemantic<TI, TO>
        , Lam.ISemantic<TI, TO>
        , App.ISemantic<TI, TO>
        , Cond.ISemantic<TI, TO>
    {
    }
}

public static class SCoreExtension
{
    public static SCore.IMeragedSemantic<TS, TR> Prj<TS, TR>(this ISemantic1<SCore, TS, TR> s)
        => (SCore.IMeragedSemantic<TS, TR>)s;

    public static string Show(this Fix<SCore> e)
        => e.Fold<string>(SCore.CreateMergeSemantic(
            new LitShowFolder(),
            new ArithShowFolder(),
            new LamShowFolder(),
            new AppShowFolder(),
            new CondShowFolder()
        ));
}

public sealed class SCoreSemantic<TS, TR>(
    Lit.ISemantic<TS, TR> Lit,
    Arith.ISemantic<TS, TR> Arith,
    Lam.ISemantic<TS, TR> Lam,
    App.ISemantic<TS, TR> App,
    Cond.ISemantic<TS, TR> Cond
) : SCore.IMeragedSemantic<TS, TR>
{
    public TR LitI(int value) => Lit.LitI(value);

    public TR Add(TS l, TS r)
        => Arith.Add(l, r);

    public TR Mul(TS l, TS r)
        => Arith.Mul(l, r);

    public TR Lambda(Identifier name, TS expr)
        => Lam.Lambda(name, expr);

    public TR Var(Identifier name)
        => Lam.Var(name);

    public TR Apply(TS f, TS x)
        => App.Apply(f, x);

    public TR If(TS c, TS t, TS fn)
        => Cond.If(c, t, fn);

    public TR Eq(TS a, TS b)
        => Cond.Eq(a, b);

    public TR Sub(TS l, TS r)
        => Arith.Sub(l, r);
}