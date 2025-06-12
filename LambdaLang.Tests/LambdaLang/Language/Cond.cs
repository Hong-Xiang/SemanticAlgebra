using System.Collections.Immutable;
using SemanticAlgebra;
using SemanticAlgebra.Control;
using SemanticAlgebra.Data;
using SemanticAlgebra.Free;
using SemanticAlgebra.Syntax;

namespace LambdaLang.Tests.LambdaLang.Language;

public partial interface Cond
    : IFunctor<Cond>
    , IImplements<Cond, ShowAlgebra, string>
    , IEvalAlgebra<Cond>
{
    [Semantic1]
    public interface ISemantic<in TS, out TR> : ISemantic1<Cond, TS, TR>
    {
        TR If(TS c, TS t, TS fn);
        TR Eq(TS a, TS b);
    }

    static ISemantic1<Cond, string, string> IImplements<Cond, ShowAlgebra, string>.Get()
        => new CondShowFolder();

    static ISemantic1<Cond, IS<M, ISigValue>, IS<M, ISigValue>> IEvalAlgebra<Cond>.Get<M>()
        => new CondEvalFolder<M>();

    static ISemantic1<Cond, IS<Free<EvalF>, ISigValue>, IS<Free<EvalF>, ISigValue>> IEvalAlgebra<Cond>.GetFree()
        => new CondFreeFolder();
}

public sealed class CondShowFolder : Cond.ISemantic<string, string>
{
    public string If(string c, string t, string fn) => $"if {c} then {t} else {fn}";
    public string Eq(string a, string b) => $"({a} == {b})";
}

public sealed class CondEvalFolder<M> : Cond.ISemantic<IS<M, ISigValue>, IS<M, ISigValue>>
    where M : IMonadState<M, ImmutableDictionary<Identifier, ISigValue>>
{
    public IS<M, ISigValue> If(IS<M, ISigValue> c, IS<M, ISigValue> t, IS<M, ISigValue> fn)
        => from cond in c
           from result in cond switch
           {
               SigBool b => b.Value ? t : fn,
               _ => throw new EvalRuntimeException($"If condition must be a boolean, got {cond}")
           }
           select result;

    public IS<M, ISigValue> Eq(IS<M, ISigValue> a, IS<M, ISigValue> b)
        => from a_ in a
           from b_ in b
           select new SigBool(a_.Equals(b_)) as ISigValue;
}

public sealed class CondFreeFolder : Cond.ISemantic<IS<Free<EvalF>, ISigValue>, IS<Free<EvalF>, ISigValue>>
{
    public IS<Free<EvalF>, ISigValue> If(IS<Free<EvalF>, ISigValue> c, IS<Free<EvalF>, ISigValue> t, IS<Free<EvalF>, ISigValue> fn)
        => from cond in c
           from result in cond switch
           {
               SigBool b => b.Value ? t : fn,
               _ => throw new EvalRuntimeException($"If condition must be a boolean, got {cond}")
           }
           select result;

    public IS<Free<EvalF>, ISigValue> Eq(IS<Free<EvalF>, ISigValue> a, IS<Free<EvalF>, ISigValue> b)
        => from a_ in a
           from b_ in b
           select new SigBool(a_.Equals(b_)) as ISigValue;
}