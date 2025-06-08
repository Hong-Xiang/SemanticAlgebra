using SemanticAlgebra.Control;

namespace SemanticAlgebra.Data;

public interface IMonadState<M, S> : IMonad<M>
    where M : IMonadState<M, S>
{
    static abstract IS<M, S> Get();
    static abstract IS<M, Unit> Put(S value);

    static virtual IS<M, Unit> Modify(Func<S, S> f)
        => from s in M.Get()
           from _ in M.Put(f(s))
           select Unit.Default;

    static virtual IS<M, T> Local<T>(
        Func<S, S> f,
        IS<M, T> e
    ) =>
        from s in M.Get()
        from _1 in M.Put(f(s))
        from r in e
        from _2 in M.Put(s)
        select r;
}

public static class MonadState
{
    public static IS<M, T> WithLocal<M, S, T>(
        this IS<M, T> e,
        Func<S, S> f
    )
        where M : IMonadState<M, S>
        => M.Local(f, e);
}