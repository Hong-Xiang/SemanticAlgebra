using SemanticAlgebra.Control;

namespace SemanticAlgebra.Data;

public interface IMonadState<M, S> : IMonad<M>
    where M : IMonad<M>
{
    static abstract IS<M, S> Get();
    static abstract IS<M, Unit> Put(S value);
}