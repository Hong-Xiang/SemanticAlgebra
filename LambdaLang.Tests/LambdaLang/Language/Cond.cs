using SemanticAlgebra;
using SemanticAlgebra.Control;
using SemanticAlgebra.Data;
using SemanticAlgebra.Syntax;

namespace LambdaLang.Tests.LambdaLang.Language;

public partial interface Cond : IFunctor<Cond>
{
    [Semantic1]
    public interface ISemantic<in TS, out TR>
        : ISemantic1<Cond, TS, TR>
    {
        TR If(TS c, TS t, TS fn);
        TR Eq(TS a, TS b);
    }
}

public sealed class CondShowFolder : Cond.ISemantic<string, string>
{
    public string If(string c, string t, string f)
        => $"(if {c}) then {t} else {f}";

    public string Eq(string a, string b)
        => $"({a} == {b})";
}

sealed class EvalRuntimeException(string message) : Exception(message)
{
}

public sealed class CondEvalFolder<M> : Cond.ISemantic<
    IS<M, ISigValue>,
    IS<M, ISigValue>
>
    where M : IMonad<M>
{
    public IS<M, ISigValue> If(IS<M, ISigValue> c, IS<M, ISigValue> t, IS<M, ISigValue> f)
        => from b in c
           let bv = ((SigBool)b).Value
           from r in bv ? t : f
           select r;

    public IS<M, ISigValue> Eq(IS<M, ISigValue> a, IS<M, ISigValue> b)
        => from va in a
           from vb in b
           select (va, vb) switch
           {
               (SigInt ia, SigInt ib) => (ISigValue)new SigBool(ia == ib),
               (SigBool ia, SigBool ib) => new SigBool(ia == ib),
               _ => throw new EvalRuntimeException($"argument type not match for eq, got {va}, {vb}")
           };
}