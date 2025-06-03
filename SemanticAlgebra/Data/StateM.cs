using SemanticAlgebra.Control;

namespace SemanticAlgebra.Data;

// state f a b
//    get :: f a b
//    put :: a -> f a b

// valid impl :
//  f a b = f s s = m s

//    run :: (s, f a) -> a
public interface IMonadState<M, S> : IMonad<M>
    where M : IMonad<M>
{
    static abstract IS<M, S> Get();
    static abstract IS<M, Unit> Put(S value);
}

public abstract class StateT<M, S> : IMonadState<StateT<M, S>, S>
    where M : IMonad<M>
{
    public static class B
    {
        public static IS<StateT<M, S>, TR> From<TR>(Func<S, (TR, S)> value)
            => Alias1.B.From<StateT<M, S>, TR, Func<S, (TR, S)>>(value);
    }

    public static ISemantic1<StateT<M, S>, T, IS<StateT<M, S>, T>> Id<T>()
        => Alias1.Id<StateT<M, S>, T, Func<S, (T, S)>>();

    public static ISemantic1<StateT<M, S>, TS, TR> Compose<TS, TI, TR>(ISemantic1<StateT<M, S>, TS, TI> s,
        Func<TI, TR> f)
        => Alias1.Compose<StateT<M, S>, TS, TI, TR, Func<S, (TS, S)>>(s, f);

    public static ISemantic1<StateT<M, S>, TS, IS<StateT<M, S>, TR>> MapS<TS, TR>(Func<TS, TR> f)
        => Id<TS>().Compose(fs =>
        {
            var fsv = fs.Unwrap();
            return B.From<TR>(s =>
            {
                var (value, state) = fsv(s);
                return (f(value), state);
            });
        });

    // public static ISemantic1<StateT<M, S>, Func<TS, TR>, ISemantic1<StateT<M, S>, TS, IS<StateT<M, S>, TR>>> ApplyS<TS,
    //     TR>()
    //     => Id<Func<TS, TR>>().Compose(fsr =>
    //         Kind1K<StateT<M, S>>.Semantic<TS, IS<StateT<M, S>, TR>>(fs =>
    //             from func in fsr
    //             from s in fs
    //             select func(s)
    //         ));

    public static IS<StateT<M, S>, T> Pure<T>(T x)
        => B.From<T>(s => (x, s));

    public static ISemantic1<StateT<M, S>, IS<StateT<M, S>, T>, IS<StateT<M, S>, T>> JoinS<T>()
        => Id<IS<StateT<M, S>, T>>().Compose(ffs => B.From<T>(s =>
        {
            var (value, state) = ffs.Unwrap()(s);
            return value.Unwrap()(state);
        }));

    public static IS<StateT<M, S>, S> Get()
        => B.From(s => (s, s));

    public static IS<StateT<M, S>, Unit> Put(S value)
        => B.From(_ => (Prelude.Unit, value));
}

public static partial class StateTExtension
{
    public static Func<TS, (TR, TS)> Unwrap<M, TS, TR>(this IS<StateT<M, TS>, TR> e)
        where M : IMonad<M>
        => ((Alias1.D.From<StateT<M, TS>, TR, Func<TS, (TR, TS)>>)e).Value;
}