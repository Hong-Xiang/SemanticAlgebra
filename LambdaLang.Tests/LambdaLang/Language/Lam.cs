using System.Collections.Immutable;
using SemanticAlgebra.Data;
using SemanticAlgebra;
using SemanticAlgebra.Syntax;

namespace LambdaLang.Tests.LambdaLang.Language;

public sealed class Identifier(string name)
{
    public string Name { get; } = name;

    public override string ToString()
    {
        return $"${Name}";
    }
}

public partial interface Lam
    : IFunctor<Lam>
    , IImplements<Lam, ShowAlgebra, string>
    , IEvalAlgebra<Lam>
{
    // public static IS<Lam, T> Var<T>(Identifier name) => new Var<T>(name);
    // public static IS<Lam, T> Lambda<T>(Identifier name, T expr) => new Lambda<T>(name, expr);
    //
    // static ISemantic1<Lam, TS, TR> IKind1<Lam>.Compose<TS, TI, TR>(ISemantic1<Lam, TS, TI> s, Func<TI, TR> f)
    //     => new LamComposeSemantic<TS, TI, TR>(s.Prj(), f);
    //
    // static ISemantic1<Lam, T, IS<Lam, T>> IKind1<Lam>.Id<T>()
    //     => new LamIdSemantic<T>();
    //
    // static ISemantic1<Lam, TS, IS<Lam, TR>> IFunctor<Lam>.MapS<TS, TR>(Func<TS, TR> f)
    //     => new LamMapSemantic<TS, TR>(f);
    //

    static ISemantic1<Lam, string, string> IImplements<Lam, ShowAlgebra, string>.Get()
        => new LamShowFolder();

    static ISemantic1<Lam, IS<M, ISigValue>, IS<M, ISigValue>> IEvalAlgebra<Lam>.Get<M>()
        => new LamEvalFolder<M>();


    [Semantic1]
    public interface ISemantic<in TS, out TR> : ISemantic1<Lam, TS, TR>
    {
        TR Lambda(Identifier name, TS expr);
        TR Var(Identifier name);
    }
}

//
// static class LamExtension
// {
//     public static ILamSemantic<TS, TR> Prj<TS, TR>(this ISemantic1<Lam, TS, TR> s)
//         => (ILamSemantic<TS, TR>)s;
// }
//
// sealed record class Var<T>(Identifier Name)
//     : IS<Lam, T>
// {
//     public TR Evaluate<TR>(ISemantic1<Lam, T, TR> semantic)
//         => semantic.Prj().Var(Name);
// }
//
// sealed record class Lambda<T>(Identifier Name, T Expr)
//     : IS<Lam, T>
// {
//     public TR Evaluate<TR>(ISemantic1<Lam, T, TR> semantic)
//         => semantic.Prj().Lambda(Name, Expr);
// }
//
// public sealed class LamComposeSemantic<TS, TI, TR>(ILamSemantic<TS, TI> S, Func<TI, TR> F) : ILamSemantic<TS, TR>
// {
//     public TR Lambda(Identifier name, TS expr)
//         => F(S.Lambda(name, expr));
//
//     public TR Var(Identifier name)
//         => F(S.Var(name));
// }
//
// public sealed class LamIdSemantic<T>() : ILamSemantic<T, IS<Lam, T>>
// {
//     public IS<Lam, T> Lambda(Identifier name, T expr)
//         => Lam.Lambda(name, expr);
//
//     public IS<Lam, T> Var(Identifier name)
//         => Lam.Var<T>(name);
// }
//
// public sealed class LamMapSemantic<TS, TR>(Func<TS, TR> F) : ILamSemantic<TS, IS<Lam, TR>>
// {
//     public IS<Lam, TR> Lambda(Identifier name, TS expr)
//         => Lam.Lambda(name, F(expr));
//
//     public IS<Lam, TR> Var(Identifier name)
//         => Lam.Var<TR>(name);
// }
//
public sealed class LamShowFolder : Lam.ISemantic<string, string>
{
    string Lam.ISemantic<string, string>.Lambda(Identifier name, string expr)
        => $"(λ{name.Name}.{expr})";

    string Lam.ISemantic<string, string>.Var(Identifier name)
        => name.Name;
}

public sealed class LamEvalFolder<M> : Lam.ISemantic<IS<M, ISigValue>, IS<M, ISigValue>>
    where M : IMonadState<M, ImmutableDictionary<Identifier, ISigValue>>
{
    public IS<M, ISigValue> Lambda(Identifier name, IS<M, ISigValue> expr)
        // => from cenv in M.Get()
        //    select (ISigValue)new SigLam<M>(val =>
        //        expr.WithLocal<M, ImmutableDictionary<Identifier, ISigValue>, ISigValue>(_ =>
        //            cenv.Add(name, val)));
        => from e in M.Get()
           select (ISigValue)(new SigClosure<M, ISigValue>(name, e, expr));

    IS<M, ISigValue> Lam.ISemantic<IS<M, ISigValue>, IS<M, ISigValue>>.Var(Identifier name)
        => M.Get().Select(env =>
            env.TryGetValue(name, out var val)
                ? val
                : throw new EvalRuntimeException($"identifier {name} not found"));
}