using SemanticAlgebra;
using SemanticAlgebra.Control;
using SemanticAlgebra.Data;
using SemanticAlgebra.Syntax;

namespace LambdaLang.Tests.LambdaLang.Language;

public partial interface Arith
    : IFunctor<Arith>
    , IImplements<Arith, ShowAlgebra, string>
    , IEvalAlgebra<Arith>
{
    static ISemantic1<Arith, string, string> IImplements<Arith, ShowAlgebra, string>.Get()
        => new ArithShowFolder();

    static ISemantic1<Arith, IS<M, ISigValue>, IS<M, ISigValue>> IEvalAlgebra<Arith>.Get<M>()
        => new ArithEvalFolder<M>();

    [Semantic1]
    public partial interface ISemantic<in TS, out TR>
        : ISemantic1<Arith, TS, TR>
    {
        TR Add(TS l, TS r);
        TR Sub(TS l, TS r);
        TR Mul(TS l, TS r);
    }
}

public sealed class ArithShowFolder : Arith.ISemantic<string, string>
{
    public string Sub(string l, string r)
        => $"({l} - {r})";

    string Arith.ISemantic<string, string>.Add(string l, string r)
        => $"({l} + {r})";

    string Arith.ISemantic<string, string>.Mul(string l, string r)
        => $"({l} * {r})";
}

public sealed class ArithEvalFolder<M> : Arith.ISemantic<IS<M, ISigValue>, IS<M, ISigValue>>
    where M : IMonad<M>
{
    public IS<M, ISigValue> Add(IS<M, ISigValue> l, IS<M, ISigValue> r)
        => from l_ in l
           from r_ in r
           select (l_, r_) switch
           {
               (SigInt left, SigInt right) => (ISigValue)new SigInt(left.Value + right.Value),
               _ => throw new EvalRuntimeException($"Add requires int arguments, got {l_}, {r_}")
           };

    public IS<M, ISigValue> Mul(IS<M, ISigValue> l, IS<M, ISigValue> r)
        => from l_ in l
           from r_ in r
           select (l_, r_) switch
           {
               (SigInt left, SigInt right) => (ISigValue)new SigInt(left.Value * right.Value),
               _ => throw new EvalRuntimeException($"Mul requires int arguments, got {l_}, {r_}")
           };

    public IS<M, ISigValue> Sub(IS<M, ISigValue> l, IS<M, ISigValue> r)
        => from l_ in l
           from r_ in r
           select (l_, r_) switch
           {
               (SigInt left, SigInt right) => (ISigValue)new SigInt(left.Value - right.Value),
               _ => throw new EvalRuntimeException($"Sub requires int arguments, got {l_}, {r_}")
           };
}