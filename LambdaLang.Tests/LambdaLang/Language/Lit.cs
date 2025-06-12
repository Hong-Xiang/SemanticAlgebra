using SemanticAlgebra;
using SemanticAlgebra.Control;
using SemanticAlgebra.Data;
using SemanticAlgebra.Free;
using SemanticAlgebra.Syntax;

namespace LambdaLang.Tests.LambdaLang.Language;

public partial interface Lit
    : IFunctor<Lit>
    , IImplements<Lit, ShowAlgebra, string>
    , IEvalAlgebra<Lit>
{
    [Semantic1]
    public interface ISemantic<in TS, out TR> : ISemantic1<Lit, TS, TR>
    {
        TR LitI(int value);
    }

    static ISemantic1<Lit, string, string> IImplements<Lit, ShowAlgebra, string>.Get()
        => new LitShowFolder();

    static ISemantic1<Lit, IS<M, ISigValue>, IS<M, ISigValue>> IEvalAlgebra<Lit>.Get<M>()
        => new LitEvalFolder<M>();

    static ISemantic1<Lit, IS<Free<EvalF>, ISigValue>, IS<Free<EvalF>, ISigValue>> IEvalAlgebra<Lit>.GetFree()
        => new LitFreeFolder();
}

public sealed class LitShowFolder : Lit.ISemantic<string, string>
{
    public string LitI(int value) => value.ToString();
}

public sealed class LitEvalFolder<M> : Lit.ISemantic<IS<M, ISigValue>, IS<M, ISigValue>>
    where M : IMonadState<M, System.Collections.Immutable.ImmutableDictionary<Identifier, ISigValue>>
{
    public IS<M, ISigValue> LitI(int value)
        => M.Pure<ISigValue>(new SigInt(value));
}

public sealed class LitFreeFolder : Lit.ISemantic<IS<Free<EvalF>, ISigValue>, IS<Free<EvalF>, ISigValue>>
{
    public IS<Free<EvalF>, ISigValue> LitI(int value)
        => Free<EvalF>.Pure<ISigValue>(new SigInt(value));
}