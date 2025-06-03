using SemanticAlgebra.Control;

namespace SemanticAlgebra.Data;

public sealed class Func2B
{
    public static class B
    {
        public static IS<K1<TS>, TR> From<TS, TR>(Func<TS, TR> value)
            => Alias1.B.From<K1<TS>, TR, Func<TS, TR>>(value);
    }

    public sealed class K1<S>
        : IMonad<K1<S>>
    {
        public interface ISemantic<in TS, out TR>
            : Alias1.ISemantic<K1<S>, TS, TR, Func<S, TS>>
        {
        }

        public static ISemantic1<K1<S>, T, IS<K1<S>, T>> Id<T>()
            => Alias1.Id<K1<S>, T, Func<S, T>>();

        public static ISemantic1<K1<S>, TS, TR> Compose<TS, TI, TR>(ISemantic1<K1<S>, TS, TI> s, Func<TI, TR> f)
            => Alias1.Compose<K1<S>, TS, TI, TR, Func<S, TS>>(s, f);

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
        => ((Alias1.D.From<Func2B.K1<TS>, TR, Func<TS, TR>>)e).Value;
}