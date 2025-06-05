using SemanticAlgebra.Control;

namespace SemanticAlgebra.Data;

public interface IMonadState<M, S> : IMonad<M>
    where M : IMonad<M>
{
    static abstract IS<M, S> Get();
    static abstract IS<M, Unit> Put(S value);

    public static virtual IS<M, T> Local<M, S, T>(
        Func<S, S> f,
        IS<M, T> e)
        where M : IMonadState<M, S>
    {
        var state = M.Get();
        return from s in state
               from _ in M.Put(f(s))
               from r in e
               
               from _2 in M.Put(s)
               select r;
    }
}