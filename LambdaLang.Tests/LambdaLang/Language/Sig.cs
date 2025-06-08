using System.Collections.Immutable;
using SemanticAlgebra;
using SemanticAlgebra.Control;
using SemanticAlgebra.Data;
using SemanticAlgebra.Fix;

namespace LambdaLang.Tests.LambdaLang.Language;

public interface Sig
    : IFunctor<Sig>
    , IMergedSemantic1<Sig, Lit, Arith, Lam, App, Bind, Cond>
    , IMergedSemantic1WithAlgebra<Sig, ShowAlgebra, string, Lit, Arith, Lam, App, Bind, Cond>
    , IMergedSemantic1EvalAlgebra<Sig, Lit, Arith, Lam, App, Bind, Cond>
    , Lit, Arith, Lam, App, Bind, Cond
{
    static ISemantic1<Sig, TS, TR> IMergedSemantic1<Sig, Lit, Arith, Lam, App, Bind, Cond>.MergeSemantic<TS, TR>(
        ISemantic1<Lit, TS, TR> Lit,
        ISemantic1<Arith, TS, TR> Arith,
        ISemantic1<Lam, TS, TR> Lam,
        ISemantic1<App, TS, TR> App,
        ISemantic1<Bind, TS, TR> Bind,
        ISemantic1<Cond, TS, TR> Cond
    ) => MergeSemantic(Lit, Arith, Lam, App, Bind, Cond);

    public static ISemantic1<Sig, TS, TR> MergeSemantic<TS, TR>(
        ISemantic1<Lit, TS, TR> Lit,
        ISemantic1<Arith, TS, TR> Arith,
        ISemantic1<Lam, TS, TR> Lam,
        ISemantic1<App, TS, TR> App,
        ISemantic1<Bind, TS, TR> Bind,
        ISemantic1<Cond, TS, TR> Cond
    )
        => new SigSemantic<TS, TR>(Lit.Prj(), Arith.Prj(), Lam.Prj(), App.Prj(), Bind.Prj(), Cond.Prj());

    //static ISemantic1<Sig, TS, TR> IKind1<Sig>.Compose<TS, TI, TR>(ISemantic1<Sig, TS, TI> s, Func<TI, TR> f)
    //    => SigSemantic(Kind1K<Lit>.Compose(s, f),
    //        Kind1K<Arith>.Compose(s, f),
    //        Kind1K<Lam>.Compose(s, f),
    //        Kind1K<App>.Compose(s, f),
    //        Kind1K<Bind>.Compose(s, f));

    //static ISemantic1<Sig, T, IS<Sig, T>> IKind1<Sig>.Id<T>()
    //    => SigSemantic<T, IS<Sig, T>>(
    //        Kind1K<Lit>.Id<T>(),
    //        Kind1K<Arith>.Id<T>(),
    //        Kind1K<Lam>.Id<T>(),
    //        Kind1K<App>.Id<T>(),
    //        Kind1K<Bind>.Id<T>()
    //);

    // static ISemantic1<Sig, TS, IS<Sig, TR>> IFunctor<Sig>.MapS<TS, TR>(Func<TS, TR> f)
    //     => MergeSemantic<TS, IS<Sig, TR>>(FunctorK<Lit>.MapS(f).Prj(),
    //         FunctorK<Arith>.MapS(f).Prj(),
    //         FunctorK<Lam>.MapS(f).Prj(),
    //         FunctorK<App>.MapS(f).Prj(),
    //         FunctorK<Bind>.MapS(f).Prj());


    public interface IMergedSemantic<in TI, out TO>
        : ISemantic1<Sig, TI, TO>
        , Lit.ISemantic<TI, TO>
        , Arith.ISemantic<TI, TO>
        , ILamSemantic<TI, TO>
        , IAppSemantic<TI, TO>
        , Bind.ISemantic<TI, TO>
        , Cond.ISemantic<TI, TO>
    {
    }
}

public static partial class SigExtension
{
    public static Sig.IMergedSemantic<TS, TR> Prj<TS, TR>(this ISemantic1<Sig, TS, TR> s)
        => (Sig.IMergedSemantic<TS, TR>)s;
}


public sealed class SigSemantic<TS, TR>(
    Lit.ISemantic<TS, TR> Lit,
    Arith.ISemantic<TS, TR> Arith,
    ILamSemantic<TS, TR> Lam,
    IAppSemantic<TS, TR> App,
    Bind.ISemantic<TS, TR> Bind,
    Cond.ISemantic<TS, TR> Cond
) : Sig.IMergedSemantic<TS, TR>
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

    public TR LetRec(Identifier name, TS expr, TS body)
        => Bind.LetRec(name, expr, body);

    public TR If(TS c, TS t, TS fn)
        => Cond.If(c, t, fn);

    public TR Eq(TS a, TS b)
        => Cond.Eq(a, b);

    public TR Sub(TS l, TS r)
        => Arith.Sub(l, r);
}

public sealed class SigHasBindFolder : Sig.IMergedSemantic<bool, bool>
{
    public bool Add(bool l, bool r)
        => l || r;

    public bool Apply(bool f, bool x)
        => f || x;

    public bool Eq(bool a, bool b)
        => a || b;

    public bool If(bool c, bool t, bool fn)
        => c || t || fn;

    public bool Lambda(Identifier name, bool expr)
        => expr;

    public bool Let(Identifier name, bool expr, bool body)
        => true;

    public bool LetRec(Identifier name, bool expr, bool body)
        => true;

    public bool LitI(int value)
        => false;

    public bool Mul(bool l, bool r)
        => l || r;

    public bool Sub(bool l, bool r)
        => l || r;

    public bool Var(Identifier name)
        => false;
}

public sealed class BindLoweringFolder
    : Sig.IMergedSemantic<Fix<SCore>, Fix<SCore>>
{
    public Fix<SCore> LitI(int value)
        => Lit.B.LitI<Fix<SCore>>(value).Fix();

    public Fix<SCore> Add(Fix<SCore> l, Fix<SCore> r)
        => Arith.B.Add(l, r).Fix();

    public Fix<SCore> Mul(Fix<SCore> l, Fix<SCore> r)
        => Arith.B.Mul(l, r).Fix();

    public Fix<SCore> Lambda(Identifier name, Fix<SCore> expr)
        => Lam.Lambda(name, expr).Fix();

    public Fix<SCore> Var(Identifier name)
        => Lam.Var<Fix<SCore>>(name).Fix();

    public Fix<SCore> Apply(Fix<SCore> f, Fix<SCore> x)
        => App.Apply(f, x).Fix();

    public Fix<SCore> Let(Identifier name, Fix<SCore> expr, Fix<SCore> body)
        => Apply(Lambda(name, expr), body);

    public Fix<SCore> LetRec(Identifier name, Fix<SCore> expr, Fix<SCore> body)
    {
        throw new NotImplementedException();
    }

    public Fix<SCore> If(Fix<SCore> c, Fix<SCore> t, Fix<SCore> fn)
        => Cond.B.If(c, t, fn).Fix();

    public Fix<SCore> Eq(Fix<SCore> a, Fix<SCore> b)
        => Cond.B.Eq(a, b).Fix();

    public Fix<SCore> Sub(Fix<SCore> l, Fix<SCore> r)
        => Arith.B.Sub(l, r).Fix();
}