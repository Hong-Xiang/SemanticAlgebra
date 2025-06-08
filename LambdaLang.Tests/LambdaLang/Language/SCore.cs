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
        ) => MergeSemantic(Lit, Arith, Lam, App, Cond);

    static IMeragedSemantic<TS, TR> MergeSemantic<TS, TR>(
        ISemantic1<Lit, TS, TR> Lit,
        ISemantic1<Arith, TS, TR> Arith,
        ISemantic1<Lam, TS, TR> Lam,
        ISemantic1<App, TS, TR> App,
        ISemantic1<Cond, TS, TR> Cond
    )
        => new SCoreSemantic<TS, TR>(Lit.Prj(), Arith.Prj(), Lam.Prj(), App.Prj(), Cond.Prj());

    //static ISemantic1<SCore, TS, TR> IKind1<SCore>.Compose<TS, TI, TR>(ISemantic1<SCore, TS, TI> s, Func<TI, TR> f)
    //    => MergeSemantic(Kind1K<Lit>.Compose(s, f),
    //        Kind1K<Arith>.Compose(s, f),
    //        Kind1K<Lam>.Compose(s, f),
    //        Kind1K<App>.Compose(s, f));

    //static ISemantic1<SCore, T, IS<SCore, T>> IKind1<SCore>.Id<T>()
    //    => MergeSemantic<T, IS<SCore, T>>(
    //        Kind1K<Lit>.Id<T>(),
    //        Kind1K<Arith>.Id<T>(),
    //        Kind1K<Lam>.Id<T>(),
    //        Kind1K<App>.Id<T>()
    //    );

    //static ISemantic1<SCore, TS, IS<SCore, TR>> IFunctor<SCore>.MapS<TS, TR>(Func<TS, TR> f)
    //    => MergeSemantic<TS, IS<SCore, TR>>(
    //        FunctorK<Lit>.MapS(f).Prj(),
    //        FunctorK<Arith>.MapS(f).Prj(),
    //        FunctorK<Lam>.MapS(f).Prj(),
    //        FunctorK<App>.MapS(f).Prj());


    public interface IMeragedSemantic<in TI, out TO>
        : ISemantic1<SCore, TI, TO>
        , Lit.ISemantic<TI, TO>
        , Arith.ISemantic<TI, TO>
        , ILamSemantic<TI, TO>
        , IAppSemantic<TI, TO>
        , Cond.ISemantic<TI, TO>
    {
    }
}

public static class SCoreExtension
{
    public static SCore.IMeragedSemantic<TS, TR> Prj<TS, TR>(this ISemantic1<SCore, TS, TR> s)
        => (SCore.IMeragedSemantic<TS, TR>)s;

    public static string Show(this Fix<SCore> e)
        => e.Fold<string>(SCore.MergeSemantic(
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
    ILamSemantic<TS, TR> Lam,
    IAppSemantic<TS, TR> App,
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