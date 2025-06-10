using System.Collections.Immutable;
using SemanticAlgebra;
using SemanticAlgebra.Data;
using SemanticAlgebra.Fix;
using SemanticAlgebra.Free;

namespace LambdaLang.Tests.LambdaLang.Language;

public interface ISigValue
{
}

public sealed record SigInt(int Value) : ISigValue
{
}

public sealed record SigBool(bool Value) : ISigValue
{
}

public sealed class SigClosure<M, T>(
    Identifier Name,
    ImmutableDictionary<Identifier, ISigValue> Env,
    IS<M, T> Body
) : ISigValue
    where M : IMonadState<M, ImmutableDictionary<Identifier, ISigValue>>
{
    public Identifier Name { get; } = Name;
    public ImmutableDictionary<Identifier, ISigValue> Env { get; internal set; } = Env;
    public IS<M, T> Body { get; } = Body;
}

public sealed class SigClosureF<F, T>(
    Identifier Name,
    ImmutableDictionary<Identifier, ISigValue> Env,
    IS<Free<F>, T> Body
) : ISigValue
    where F : IFunctor<F>
{
    public Identifier Name { get; } = Name;
    public ImmutableDictionary<Identifier, ISigValue> Env { get; internal set; } = Env;
    public IS<Free<F>, T> Body { get; } = Body;
}


sealed class EvalRuntimeException(string message) : Exception(message)
{
}