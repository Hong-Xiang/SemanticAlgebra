using SemanticAlgebra.Control;

namespace SemanticAlgebra.Data;

public abstract class StateT<M, S> : IMonadState<StateT<M, S>, S>
    where M : IMonad<M>
{
    public interface IAlias1<TS> : Alias1.ISpec<StateT<M, S>, TS, Func<S, IS<M, (TS Value, S State)>>>
    {
    }

    public static class B
    {
        public static IS<StateT<M, S>, TR> From<TR>(Func<S, IS<M, (TR, S)>> value)
            => IAlias1<TR>.From(value);
    }

    public static ISemantic1<StateT<M, S>, T, IS<StateT<M, S>, T>> Id<T>()
        => IAlias1<T>.Id();

    public static ISemantic1<StateT<M, S>, TS, TR> Compose<TS, TI, TR>(ISemantic1<StateT<M, S>, TS, TI> s,
            Func<TI, TR> f)
        => IAlias1<TS>.Compose(s, f);

    public static ISemantic1<StateT<M, S>, TS, IS<StateT<M, S>, TR>> MapS<TS, TR>(Func<TS, TR> f)
        => IAlias1<TS>.Semantic(fs => B.From(s
            => from x in fs(s)
               select (f(x.Value), x.State)));


    public static IS<StateT<M, S>, T> Pure<T>(T x)
        => B.From(s => M.Pure((x, s)));

    public static ISemantic1<StateT<M, S>, IS<StateT<M, S>, T>, IS<StateT<M, S>, T>> JoinS<T>()
        => IAlias1<IS<StateT<M, S>, T>>.Semantic(fs => B.From(s
            => from x in fs(s)
               from r in x.Value.Unwrap()(x.State)
               select r));

    // monad state
    public static IS<StateT<M, S>, S> Get()
        => B.From(s => M.Pure((s, s)));

    public static IS<StateT<M, S>, Unit> Put(S value)
        => B.From(_ => M.Pure((Unit.Default, value)));
}

public static partial class StateTExtension
{
    public static Func<TS, IS<M, (TR, TS)>> Unwrap<M, TS, TR>(this IS<StateT<M, TS>, TR> e)
        where M : IMonad<M>
        => StateT<M, TS>.IAlias1<TR>.Unwrap(e);

    public static StateT<M, S>.IAlias1<TS>.ISemantic<TR> Prj<M, S, TS, TR>(this
        ISemantic1<StateT<M, S>, TS, TR> s)
        where M : IMonad<M>
        => StateT<M, S>.IAlias1<TS>.Prj(s);

    public static (T Value, S State) Run<S, T>(this IS<StateT<Identity, S>, T> e, S s)
        => e.Unwrap()(s).Unwrap();

    public static T Eval<S, T>(this IS<StateT<Identity, S>, T> e, S s)
        => e.Run(s).Value;

    public static S Exec<S, T>(this IS<StateT<Identity, S>, T> e, S s)
        => e.Run(s).State;
}

public static class State<S>
{
    public static IS<StateT<Identity, S>, Unit> Put(S value)
        => StateT<Identity, S>.Put(value);

    public static IS<StateT<Identity, S>, S> Get()
        => StateT<Identity, S>.Get();
}