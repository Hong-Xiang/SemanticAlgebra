using SemanticAlgebra;
using SemanticAlgebra.Data;

namespace LambdaLang.Tests.LambdaLang.Language;

public interface SCore
    : IFunctor<SCore>
    , Lit, Arith, Lam, App
{
    static ISemantic<TS, TR> ComposeSemantic<TS, TR>(
        ISemantic1<Lit, TS, TR> Lit,
        ISemantic1<Arith, TS, TR> Arith,
        ISemantic1<Lam, TS, TR> Lam,
        ISemantic1<App, TS, TR> App
    )
        => new SCoreSemantic<TS, TR>(Lit.Prj(), Arith.Prj(), Lam.Prj(), App.Prj());

    static ISemantic1<SCore, TS, TR> IKind1<SCore>.Compose<TS, TI, TR>(ISemantic1<SCore, TS, TI> s, Func<TI, TR> f)
        => ComposeSemantic(Kind1K<Lit>.Compose(s, f),
            Kind1K<Arith>.Compose(s, f),
            Kind1K<Lam>.Compose(s, f),
            Kind1K<App>.Compose(s, f));

    static ISemantic1<SCore, T, IS<SCore, T>> IKind1<SCore>.Id<T>()
        => ComposeSemantic<T, IS<SCore, T>>(
            Kind1K<Lit>.Id<T>(),
            Kind1K<Arith>.Id<T>(),
            Kind1K<Lam>.Id<T>(),
            Kind1K<App>.Id<T>()
        );

    static ISemantic1<SCore, TS, IS<SCore, TR>> IFunctor<SCore>.MapS<TS, TR>(Func<TS, TR> f)
        => ComposeSemantic<TS, IS<SCore, TR>>(
            FunctorK<Lit>.MapS(f).Prj(),
            FunctorK<Arith>.MapS(f).Prj(),
            FunctorK<Lam>.MapS(f).Prj(),
            FunctorK<App>.MapS(f).Prj());


    public interface ISemantic<in TI, out TO>
        : ISemantic1<SCore, TI, TO>
        , ILitSemantic<TI, TO>
        , IArithSemantic<TI, TO>
        , ILamSemantic<TI, TO>
        , IAppSemantic<TI, TO>
    {
    }
}

public static class SCoreExtension
{
    public static SCore.ISemantic<TS, TR> Prj<TS, TR>(this ISemantic1<SCore, TS, TR> s)
        => (SCore.ISemantic<TS, TR>)s;
}

public sealed class SCoreSemantic<TS, TR>(
    ILitSemantic<TS, TR> Lit,
    IArithSemantic<TS, TR> Arith,
    ILamSemantic<TS, TR> Lam,
    IAppSemantic<TS, TR> App
) : SCore.ISemantic<TS, TR>
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
}