using SemanticAlgebra.Data;
using SemanticAlgebra;

namespace LambdaLang.Language;

sealed class Identifier(string name)
{
    public string Name { get; } = name;
}

interface Lam : IFunctor<Lam>
{
    public static IS<Lam, T> Var<T>(Identifier name) => new Var<T>(name);
    public static IS<Lam, T> Lambda<T>(Identifier name, T expr) => new Lambda<T>(name, expr);

    static ISemantic1<Lam, TS, TR> IKind1<Lam>.Compose<TS, TI, TR>(ISemantic1<Lam, TS, TI> s, Func<TI, TR> f)
        => new LamComposeSemantic<TS, TI, TR>(s.Prj(), f);

    static ISemantic1<Lam, T, IS<Lam, T>> IKind1<Lam>.Id<T>()
        => new LamIdSemantic<T>();

    static ISemantic1<Lam, TS, IS<Lam, TR>> IFunctor<Lam>.MapS<TS, TR>(Func<TS, TR> f)
        => new LamMapSemantic<TS, TR>(f);
}

interface ILamSemantic<in TS, out TR> : ISemantic1<Lam, TS, TR>
{
    TR Lambda(Identifier name, TS expr);
    TR Var(Identifier name);
}

static class LamExtension
{
    public static ILamSemantic<TS, TR> Prj<TS, TR>(this ISemantic1<Lam, TS, TR> s)
        => (ILamSemantic<TS, TR>)s;
}

sealed record class Var<T>(Identifier Name)
    : IS<Lam, T>
{
    public TR Evaluate<TR>(ISemantic1<Lam, T, TR> semantic)
        => semantic.Prj().Var(Name);
}

sealed record class Lambda<T>(Identifier Name, T Expr)
    : IS<Lam, T>
{
    public TR Evaluate<TR>(ISemantic1<Lam, T, TR> semantic)
        => semantic.Prj().Lambda(Name, Expr);
}

sealed class LamComposeSemantic<TS, TI, TR>(ILamSemantic<TS, TI> S, Func<TI, TR> F) : ILamSemantic<TS, TR>
{
    public TR Lambda(Identifier name, TS expr)
        => F(S.Lambda(name, expr));

    public TR Var(Identifier name)
        => F(S.Var(name));
}

sealed class LamIdSemantic<T>() : ILamSemantic<T, IS<Lam, T>>
{
    public IS<Lam, T> Lambda(Identifier name, T expr)
        => Lam.Lambda(name, expr);

    public IS<Lam, T> Var(Identifier name)
        => Lam.Var<T>(name);
}

sealed class LamMapSemantic<TS, TR>(Func<TS, TR> F) : ILamSemantic<TS, IS<Lam, TR>>
{
    public IS<Lam, TR> Lambda(Identifier name, TS expr)
        => Lam.Lambda(name, F(expr));

    public IS<Lam, TR> Var(Identifier name)
        => Lam.Var<TR>(name);
}

sealed class LamShowFolder : ILamSemantic<string, string>
{
    string ILamSemantic<string, string>.Lambda(Identifier name, string expr)
        => $"(λ{name.Name}.{expr})";

    string ILamSemantic<string, string>.Var(Identifier name)
        => name.Name;
}

sealed class LamEvalFolder : ILamSemantic<SigEvalData, SigEvalData>
{
    public SigEvalData Lambda(Identifier name, SigEvalData expr)
    {
        return SigEvalState.From(s =>
        {
            return (s, (ISigValue)new SigLam(val =>
            {
                var s_ = s.Add(name, val);
                var r = expr.Run(s_);
                return r.Data;
            }));
        });
    }

    public SigEvalData Var(Identifier name) => SigEvalState.From(s => (s, s[name]));
}