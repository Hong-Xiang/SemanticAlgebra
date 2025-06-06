using System.Collections.Immutable;
using SemanticAlgebra;
using SemanticAlgebra.Data;
using SemanticAlgebra.Fix;

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

public sealed record SigClosure<M, T>(
    Identifier Name,
    ImmutableDictionary<Identifier, ISigValue> Env,
    IS<M, T> Body
) : ISigValue
    where M : IMonadState<M, ImmutableDictionary<Identifier, ISigValue>>
{
}

public sealed record SigLam<M>(Func<ISigValue, IS<M, ISigValue>> F) : ISigValue
    where M : IMonadState<M, ImmutableDictionary<Identifier, ISigValue>>
{
}