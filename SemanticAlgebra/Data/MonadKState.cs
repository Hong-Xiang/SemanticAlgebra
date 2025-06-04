using SemanticAlgebra.Control;

namespace SemanticAlgebra.Data;

public interface IMonadKState<M, in TK, TV> : IMonad<M>
    where M : IMonad<M>
{
    static abstract IS<M, TV> Get(TK key);
    static abstract IS<M, Unit> Put(TK key, TV value);
}