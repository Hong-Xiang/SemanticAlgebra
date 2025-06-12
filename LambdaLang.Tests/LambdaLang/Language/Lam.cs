using System.Collections.Immutable;
using SemanticAlgebra;
using SemanticAlgebra.Control;
using SemanticAlgebra.Data;
using SemanticAlgebra.Free;
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
    [Semantic1]
    public interface ISemantic<in TS, out TR> : ISemantic1<Lam, TS, TR>
    {
        TR Lambda(Identifier name, TS expr);
        TR Var(Identifier name);
    }

    static ISemantic1<Lam, string, string> IImplements<Lam, ShowAlgebra, string>.Get()
        => new LamShowFolder();

    static ISemantic1<Lam, IS<M, ISigValue>, IS<M, ISigValue>> IEvalAlgebra<Lam>.Get<M>()
        => new LamEvalFolder<M>();

    static ISemantic1<Lam, IS<Free<EvalF>, ISigValue>, IS<Free<EvalF>, ISigValue>> IEvalAlgebra<Lam>.GetFree()
        => new LamFreeFolder();
}

public sealed class LamShowFolder : Lam.ISemantic<string, string>
{
    public string Lambda(Identifier name, string expr) => $"λ{name}.{expr}";
    public string Var(Identifier name) => name.Name;
}

public sealed class LamEvalFolder<M> : Lam.ISemantic<IS<M, ISigValue>, IS<M, ISigValue>>
    where M : IMonadState<M, ImmutableDictionary<Identifier, ISigValue>>
{
    public IS<M, ISigValue> Lambda(Identifier name, IS<M, ISigValue> expr)
        => from env in M.Get()
           select new SigClosure<M, ISigValue>(name, env, expr) as ISigValue;

    public IS<M, ISigValue> Var(Identifier name)
        => from env in M.Get()
           select env.TryGetValue(name, out var value)
               ? value
               : throw new EvalRuntimeException($"Unbound variable: {name}");
}

public sealed class LamFreeFolder : Lam.ISemantic<IS<Free<EvalF>, ISigValue>, IS<Free<EvalF>, ISigValue>>
{
    public IS<Free<EvalF>, ISigValue> Lambda(Identifier name, IS<Free<EvalF>, ISigValue> expr)
        => from env in EvalF.B.Get(Prelude.Id).LiftF()
           select new SigClosureF<EvalF, ISigValue>(name, env, expr) as ISigValue;

    public IS<Free<EvalF>, ISigValue> Var(Identifier name)
        => from env in EvalF.B.Get(Prelude.Id).LiftF()
           select env.TryGetValue(name, out var value)
               ? value
               : throw new EvalRuntimeException($"Unbound variable: {name}");
}