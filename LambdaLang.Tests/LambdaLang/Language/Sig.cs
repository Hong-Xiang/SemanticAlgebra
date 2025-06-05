using System.Collections.Immutable;
using SemanticAlgebra;
using SemanticAlgebra.Control;
using SemanticAlgebra.Data;
using SemanticAlgebra.Fix;

namespace LambdaLang.Tests.LambdaLang.Language;

public interface Sig
    : IFunctor<Sig>
    , Lit, Arith, Lam, App, Bind
{
    static ISigSemantic<TS, TR> SigSemantic<TS, TR>(
        ISemantic1<Lit, TS, TR> Lit,
        ISemantic1<Arith, TS, TR> Arith,
        ISemantic1<Lam, TS, TR> Lam,
        ISemantic1<App, TS, TR> App,
        ISemantic1<Bind, TS, TR> Bind
    )
        => new SigSemantic<TS, TR>(Lit.Prj(), Arith.Prj(), Lam.Prj(), App.Prj(), Bind.Prj());

    static ISemantic1<Sig, TS, TR> IKind1<Sig>.Compose<TS, TI, TR>(ISemantic1<Sig, TS, TI> s, Func<TI, TR> f)
        => SigSemantic(Kind1K<Lit>.Compose(s, f),
            Kind1K<Arith>.Compose(s, f),
            Kind1K<Lam>.Compose(s, f),
            Kind1K<App>.Compose(s, f),
            Kind1K<Bind>.Compose(s, f));

    static ISemantic1<Sig, T, IS<Sig, T>> IKind1<Sig>.Id<T>()
        => SigSemantic<T, IS<Sig, T>>(
            Kind1K<Lit>.Id<T>(),
            Kind1K<Arith>.Id<T>(),
            Kind1K<Lam>.Id<T>(),
            Kind1K<App>.Id<T>(),
            Kind1K<Bind>.Id<T>()
        );

    static ISemantic1<Sig, TS, IS<Sig, TR>> IFunctor<Sig>.MapS<TS, TR>(Func<TS, TR> f)
        => SigSemantic<TS, IS<Sig, TR>>(FunctorK<Lit>.MapS(f).Prj(),
            FunctorK<Arith>.MapS(f).Prj(),
            FunctorK<Lam>.MapS(f).Prj(),
            FunctorK<App>.MapS(f).Prj(),
            FunctorK<Bind>.MapS(f).Prj());
}

public interface ISigSemantic<in TI, out TO>
    : ISemantic1<Sig, TI, TO>
    , ILitSemantic<TI, TO>
    , IArithSemantic<TI, TO>
    , ILamSemantic<TI, TO>
    , IAppSemantic<TI, TO>
    , Bind.ISemantic<TI, TO>
{
}

public static class SigExtension
{
    public static ISigSemantic<TS, TR> Prj<TS, TR>(this ISemantic1<Sig, TS, TR> s)
        => (ISigSemantic<TS, TR>)s;
}

public sealed class SigSemantic<TS, TR>(
    ILitSemantic<TS, TR> Lit,
    IArithSemantic<TS, TR> Arith,
    ILamSemantic<TS, TR> Lam,
    IAppSemantic<TS, TR> App,
    Bind.ISemantic<TS, TR> Bind
) : ISigSemantic<TS, TR>
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

    public TR Let(Identifier name, TS expr, TS body)
        => Bind.Let(name, expr, body);
}

public sealed class BindLoweringFolder
    : ISigSemantic<Fix<SCore>, Fix<SCore>>
{
    public Fix<SCore> LitI(int value)
        => Lit.LitI<Fix<SCore>>(value).Fix();

    public Fix<SCore> Add(Fix<SCore> l, Fix<SCore> r)
        => Arith.Add(l, r).Fix();

    public Fix<SCore> Mul(Fix<SCore> l, Fix<SCore> r)
        => Arith.Mul(l, r).Fix();

    public Fix<SCore> Lambda(Identifier name, Fix<SCore> expr)
        => Lam.Lambda(name, expr).Fix();

    public Fix<SCore> Var(Identifier name)
        => Lam.Var<Fix<SCore>>(name).Fix();

    public Fix<SCore> Apply(Fix<SCore> f, Fix<SCore> x)
        => App.Apply(f, x).Fix();

    public Fix<SCore> Let(Identifier name, Fix<SCore> expr, Fix<SCore> body)
        => Apply(Lambda(name, expr), body);
}

public interface ISigValue
{
}

public sealed record SigInt(int Value) : ISigValue
{
}

public sealed record SigClosure<M>(
    Identifier Name,
    ImmutableDictionary<Identifier, ISigValue> Env,
    IS<M, ISigValue> Body
) : ISigValue
    where M : IMonadState<M, ImmutableDictionary<Identifier, ISigValue>>
{
}

public sealed record SigLam<M>(Func<ISigValue, IS<M, ISigValue>> F) : ISigValue
    where M : IMonadState<M, ImmutableDictionary<Identifier, ISigValue>>
{
}