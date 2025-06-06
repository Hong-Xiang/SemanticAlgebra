using SemanticAlgebra.Control;

namespace SemanticAlgebra.Data;

public abstract class StateT<M, S> : IMonadState<StateT<M, S>, S>, IMonadFix<StateT<M, S>>
    where M : IMonad<M>
{
    public interface IAlias1<TS> : Alias1.ISpec<StateT<M, S>, TS, Func<S, (TS, S)>>
    {
    }

    public static class B
    {
        public static IS<StateT<M, S>, TR> From<TR>(Func<S, (TR, S)> value)
            //=> Alias1.B.From<StateT<M, S>, TR, Func<S, (TR, S)>>(value);
            //=> Alias1K<StateT<M, S>, TR, IAlias1<TR>>.
            => IAlias1<TR>.From(value);
    }

    public static ISemantic1<StateT<M, S>, T, IS<StateT<M, S>, T>> Id<T>()
        //=> Alias1.Id<StateT<M, S>, T, Func<S, (T, S)>>();
        => IAlias1<T>.Id();

    public static ISemantic1<StateT<M, S>, TS, TR> Compose<TS, TI, TR>(ISemantic1<StateT<M, S>, TS, TI> s,
        Func<TI, TR> f)
        //=> Alias1.Compose<StateT<M, S>, TS, TI, TR, Func<S, (TS, S)>>(s, f);
        => IAlias1<TS>.Compose(s, f);

    public static ISemantic1<StateT<M, S>, TS, IS<StateT<M, S>, TR>> MapS<TS, TR>(Func<TS, TR> f)
        => Id<TS>().Compose(fs => B.From<TR>(s =>
        {
            var (value, state) = fs.Unwrap()(s);
            return (f(value), state);
        }));


    public static IS<StateT<M, S>, T> Pure<T>(T x)
        => B.From(s => (x, s));

    public static ISemantic1<StateT<M, S>, IS<StateT<M, S>, T>, IS<StateT<M, S>, T>> JoinS<T>()
        => Id<IS<StateT<M, S>, T>>().Compose(ffs => B.From(s =>
        {
            var (value, state) = ffs.Unwrap()(s);
            return value.Unwrap()(state);
        }));

    // monad state
    public static IS<StateT<M, S>, S> Get()
        => B.From(s => (s, s));

    public static IS<StateT<M, S>, Unit> Put(S value)
        => B.From(_ => (Unit.Default, value));


    // fix monad
    sealed class LazyCell<T>
    {
        public T? Value { get; set; } = default;
    }

    sealed class RecursiveFixValueNotEvaluatedException : Exception
    {
    }

    public static IS<StateT<M, S>, T> Fix<T>(Func<Lazy<T>, IS<StateT<M, S>, T>> f)
    {
        var cell = new LazyCell<T>();
        return Get().SelectMany(s =>
        {
            var v = new Lazy<T>(() => cell.Value ?? throw new RecursiveFixValueNotEvaluatedException());
            return f(v);
        }).Select(res =>
        {
            cell.Value = res;
            return res;
        });
    }
}

public static partial class StateTExtension
{
    public static Func<TS, (TR, TS)> Unwrap<M, TS, TR>(this IS<StateT<M, TS>, TR> e)
        where M : IMonad<M>
        //=> ((Alias1.D.From<StateT<M, TS>, TR, Func<TS, (TR, TS)>>)e).Value;
        => StateT<M, TS>.IAlias1<TR>.Unwrap(e);

    public static StateT<M, S>.IAlias1<TS>.ISemantic<TR> Prj<M, S, TS, TR>(this
        ISemantic1<StateT<M, S>, TS, TR> s)
        where M : IMonad<M>
        => (StateT<M, S>.IAlias1<TS>.ISemantic<TR>)s;
}