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

    //static virtual ICoSemantic1<TF, TS, TR> CoSemantic<TS, TR>(Func<TS, IS<TF, TR>> f)
    //    => new FuncCoSemantic<TF, TS, TR>(f);

    //static virtual IDiSemantic<TF, TS, TR> DiSemantic<TS, TR>(Func<IS<TF, TS>, IS<TF, TR>> f)
    //    => TF.Semantic(f).AsDiSemantic();
}

public static class Kind1K<TF>
    where TF : IKind1<TF>
{
    public static ISemantic1<TF, T, IS<TF, T>> Id<T>() => TF.Id<T>();
    public static ISemantic1<TF, TS, TR> Semantic<TS, TR>(Func<IS<TF, TS>, TR> f) => TF.Semantic(f);
    public static ISemantic1<TF, TS, TR> Compose<TS, TI, TR>(
            ISemantic1<TF, TS, TI> s,
            Func<TI, TR> f) => TF.Compose(s, f);


    //public static ICoSemantic1<TF, TS, TR> CoSemantic<TS, TR>(Func<TS, IS<TF, TR>> f) => TF.CoSemantic(f);
    //public static IDiSemantic<TF, TS, TR> DiSemantic<TS, TR>(Func<IS<TF, TS>, IS<TF, TR>> f) => TF.DiSemantic(f);
}

public static class Kind1Extension
{
    public static Func<IS<TF, TS>, TR> ToFunc<TF, TS, TR>(this ISemantic1<TF, TS, TR> s)
        where TF : IKind1<TF>
        => fs => fs.Evaluate(s);

    //public static Func<TS, IS<TF, TR>> ToFunc<TF, TS, TR>(this ICoSemantic1<TF, TS, TR> c)
    //    where TF : IKind1<TF>
    //    => s => c.CoEvaluate(s, TF.Id<TR>().Semantic);

    //public static Func<IS<TF, TS>, IS<TF, TR>> ToFunc<TF, TS, TR>(this IDiSemantic<TF, TS, TR> d)
    //    where TF : IKind1<TF>
    //    => d.Semantic.ToFunc();

    //public static IDiSemantic<TF, TS, TR> AsDiSemantic<TF, TS, TR>(this ISemantic1<TF, TS, IS<TF, TR>> semantic)
    //    where TF : IKind1<TF>
    //    => new DiSemanticFromSemantic<TF, TS, TR>(semantic);

    //public static ICoSemantic1<TF, TS, TR> ComposeC<TF, TS, TI, TR>(
    //    this Func<TS, TI> f,
    //    ICoSemantic1<TF, TI, TR> s)
    //    where TF : IKind1<TF>
    //    => TF.CoSemantic<TS, TR>(v => s.CoEvaluate(f(v), TF.Id<TR>().Semantic));

    //public static ISemantic1<TF, TS, TR> ComposeF<TF, TS, TM, TR>(
    //    this ISemantic1<TF, TS, TM> f,
    //    Func<TM, TR> g
    //)
    //    where TF : IKind1<TF>
    //    => TF.ComposeF(f, g);

    //public static IDiSemantic<TF, TS, TR> ComposeC<TF, TS, TM, TR>(
    //    this ISemantic1<TF, TS, TM> f,
    //    ICoSemantic1<TF, TM, TR> g
    //)
    //    where TF : IKind1<TF>
    //    => TF.ComposeF<TS, TM, IS<TF, TR>>(f, m => g.CoEvaluate(m, TF.Id<TR>().Semantic)).AsDiSemantic();

    //public static Func<TS, TR> ComposeSToF<TF, TS, TM, TR>(
    //    this ICoSemantic1<TF, TS, TM> f,
    //    ISemantic1<TF, TM, TR> g
    //)
    //    where TF : IKind1<TF>
    //    => s => f.CoEvaluate(s, g);

    //public static IDiSemantic<TF, TS, TR> ComposeSToD<TF, TS, TM, TR>(
    //    this ICoSemantic1<TF, IS<TF, TS>, TM> f,
    //    ISemantic1<TF, TM, IS<TF, TR>> g)
    //    where TF : IKind1<TF>
    //    => TF.DiSemantic<TS, TR>(fs => f.CoEvaluate(fs, g));

    //public static ISemantic1<TF, TS, TR> ComposeSToS<TF, TS, TM, TR>(
    //    this ICoSemantic1<TF, IS<TF, TS>, TM> f,
    //    ISemantic1<TF, TM, TR> g)
    //    where TF : IKind1<TF>
    //    => TF.Semantic<TS, TR>(fs => f.CoEvaluate(fs, g));
}

public interface IKind2<TF> where TF : IKind2<TF>
{
}