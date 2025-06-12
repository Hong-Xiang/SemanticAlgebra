using SemanticAlgebra;
using SemanticAlgebra.Control;
using SemanticAlgebra.Data;
using SemanticAlgebra.Free;
using SemanticAlgebra.Syntax;

namespace LambdaLang.Tests.LambdaLang.Language;

public partial interface Arith
    : IFunctor<Arith>
    , IImplements<Arith, ShowAlgebra, string>
    , IEvalAlgebra<Arith>
{
    [Semantic1]
    public interface ISemantic<in TS, out TR> : ISemantic1<Arith, TS, TR>
    {
        TR Add(TS l, TS r);
        TR Mul(TS l, TS r);
        TR Sub(TS l, TS r);
    }

    static ISemantic1<Arith, string, string> IImplements<Arith, ShowAlgebra, string>.Get()
        => new ArithShowFolder();

    static ISemantic1<Arith, IS<M, ISigValue>, IS<M, ISigValue>> IEvalAlgebra<Arith>.Get<M>()
        => new ArithEvalFolder<M>();

    static ISemantic1<Arith, IS<Free<EvalF>, ISigValue>, IS<Free<EvalF>, ISigValue>> IEvalAlgebra<Arith>.GetFree()
        => new ArithFreeFolder();
}

public sealed class ArithShowFolder : Arith.ISemantic<string, string>
{
    public string Add(string l, string r) => $"({l} + {r})";
    public string Mul(string l, string r) => $"({l} * {r})";
    public string Sub(string l, string r) => $"({l} - {r})";
}

public sealed class ArithEvalFolder<M> : Arith.ISemantic<IS<M, ISigValue>, IS<M, ISigValue>>
    where M : IMonadState<M, System.Collections.Immutable.ImmutableDictionary<Identifier, ISigValue>>
{
    public IS<M, ISigValue> Add(IS<M, ISigValue> l, IS<M, ISigValue> r)
        => from l_ in l
           from r_ in r
           select (l_, r_) switch
           {
               (SigInt li, SigInt ri) => (ISigValue)new SigInt(li.Value + ri.Value),
               _ => throw new EvalRuntimeException($"Add requires two integers, got {l_} and {r_}")
           };

    public IS<M, ISigValue> Mul(IS<M, ISigValue> l, IS<M, ISigValue> r)
        => from l_ in l
           from r_ in r
           select (l_, r_) switch
           {
               (SigInt li, SigInt ri) => (ISigValue)new SigInt(li.Value * ri.Value),
               _ => throw new EvalRuntimeException($"Mul requires two integers, got {l_} and {r_}")
           };

    public IS<M, ISigValue> Sub(IS<M, ISigValue> l, IS<M, ISigValue> r)
        => from l_ in l
           from r_ in r
           select (l_, r_) switch
           {
               (SigInt li, SigInt ri) => (ISigValue)new SigInt(li.Value - ri.Value),
               _ => throw new EvalRuntimeException($"Sub requires two integers, got {l_} and {r_}")
           };
}

public sealed class ArithFreeFolder : Arith.ISemantic<IS<Free<EvalF>, ISigValue>, IS<Free<EvalF>, ISigValue>>
{
    public IS<Free<EvalF>, ISigValue> Add(IS<Free<EvalF>, ISigValue> l, IS<Free<EvalF>, ISigValue> r)
        => from l_ in l
           from r_ in r
           select (l_, r_) switch
           {
               (SigInt li, SigInt ri) => (ISigValue)new SigInt(li.Value + ri.Value),
               _ => throw new EvalRuntimeException($"Add requires two integers, got {l_} and {r_}")
           };

    public IS<Free<EvalF>, ISigValue> Mul(IS<Free<EvalF>, ISigValue> l, IS<Free<EvalF>, ISigValue> r)
        => from l_ in l
           from r_ in r
           select (l_, r_) switch
           {
               (SigInt li, SigInt ri) => (ISigValue)new SigInt(li.Value * ri.Value),
               _ => throw new EvalRuntimeException($"Mul requires two integers, got {l_} and {r_}")
           };

    public IS<Free<EvalF>, ISigValue> Sub(IS<Free<EvalF>, ISigValue> l, IS<Free<EvalF>, ISigValue> r)
        => from l_ in l
           from r_ in r
           select (l_, r_) switch
           {
               (SigInt li, SigInt ri) => (ISigValue)new SigInt(li.Value - ri.Value),
               _ => throw new EvalRuntimeException($"Sub requires two integers, got {l_} and {r_}")
           };
}