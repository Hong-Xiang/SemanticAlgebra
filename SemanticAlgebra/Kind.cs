namespace SemanticAlgebra;

public interface IKind1<TF>
    where TF : IKind1<TF>
{
    // forall a. id :: f a -> f a
    static abstract ISemantic1<TF, T, IS<TF, T>> Id<T>();

    // forall s, i, r. compose :: (f s -> i) -> (i -> r) -> (f s -> r)
    static abstract ISemantic1<TF, TS, TR> Compose<TS, TI, TR>(
        ISemantic1<TF, TS, TI> s,
        Func<TI, TR> f);

    // semantic is encoding of f s -> r
    static virtual ISemantic1<TF, TS, TR> Semantic<TS, TR>(Func<IS<TF, TS>, TR> f)
        => TF.Compose(TF.Id<TS>(), f);
}

public static class Kind1K<TF>
    where TF : IKind1<TF>
{
    public static ISemantic1<TF, T, IS<TF, T>> Id<T>() => TF.Id<T>();
    public static ISemantic1<TF, TS, TR> Semantic<TS, TR>(Func<IS<TF, TS>, TR> f) => TF.Semantic(f);

    public static ISemantic1<TF, TS, TR> Compose<TS, TI, TR>(
        ISemantic1<TF, TS, TI> s,
        Func<TI, TR> f) => TF.Compose(s, f);

    public static ISemantic1<TF, TS, TR> Const<TS, TR>(TR value) => TF.Id<TS>().Compose(_ => value);
}

public static class Kind1Extension
{
    public static Func<IS<TF, TS>, TR> ToFunc<TF, TS, TR>(this ISemantic1<TF, TS, TR> s)
        where TF : IKind1<TF>
        => fs => fs.Evaluate(s);
}