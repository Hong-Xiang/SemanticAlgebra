using SemanticAlgebra.Control;

namespace SemanticAlgebra.Data;

public sealed class Func2B
{
    public static class B
    {
        public static IS<K1<TS>, TR> From<TS, TR>(Func<TS, TR> value)
            => K1<TS>.IAlias1<TR>.From(value);
    }

    public sealed class K1<S>
        : IMonad<K1<S>>
    {
        public interface IAlias1<TS> : Alias1.ISpec<K1<S>, TS, Func<S, TS>>
        {
        }


        public static ISemantic1<K1<S>, T, IS<K1<S>, T>> Id<T>()
            => IAlias1<T>.Id();

        public static ISemantic1<K1<S>, TS, TR> Compose<TS, TI, TR>(ISemantic1<K1<S>, TS, TI> s, Func<TI, TR> f)
            => IAlias1<TS>.Compose(s, f);

        public static ISemantic1<K1<S>, TS, IS<K1<S>, TR>> MapS<TS, TR>(Func<TS, TR> f)
            => Id<TS>().Compose(fs => B.From<S, TR>(s => f(fs.Unwrap()(s))));

        public static ISemantic1<K1<S>, Func<TS, TR>, ISemantic1<K1<S>, TS, IS<K1<S>, TR>>> ApplyS<TS, TR>()
            => Id<Func<TS, TR>>().Compose(ff =>
                Kind1K<K1<S>>.Semantic<TS, IS<K1<S>, TR>>(fs => B.From<S, TR>(s =>
                    {
                        var fsr = ff.Unwrap()(s);
                        var ts = fs.Unwrap()(s);
                        return fsr(ts);
                    })
                ));

        public static IS<K1<S>, T> Pure<T>(T x)
            => B.From<S, T>(_ => x);

        public static ISemantic1<K1<S>, IS<K1<S>, T>, IS<K1<S>, T>> JoinS<T>()
            => Id<IS<K1<S>, T>>().Compose(ns => B.From<S, T>(s => ns.Unwrap()(s).Unwrap()(s)));
    }
}

public static partial class Func2BExtension
{
    public static Func<TS, TR> Unwrap<TS, TR>(this IS<Func2B.K1<TS>, TR> e)
        => Func2B.K1<TS>.IAlias1<TR>.Unwrap(e);
}