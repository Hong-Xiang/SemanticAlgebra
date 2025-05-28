namespace SemanticAlgebra.Data;

public interface IFunctor<TF> : IKind1<TF>
    where TF : IFunctor<TF>
{
    // (s -> r) -> (f s -> f r)
    static abstract ISemantic1<TF, TS, IS<TF, TR>> MapS<TS, TR>(Func<TS, TR> f);
}

public static class FunctorK<TF>
    where TF : IFunctor<TF>
{
    public static ISemantic1<TF, TS, IS<TF, TR>> MapS<TS, TR>(Func<TS, TR> f)
        => TF.MapS(f);
}

public static class FunctorExtension
{
    public static ISemantic1<TF, TS, TR> DiMap<TF, TS, TI, TO, TR>(
        this ISemantic1<TF, TI, TO> s,
        Func<TS, TI> f,
        Func<TO, TR> g
    )
        where TF : IFunctor<TF> =>
            TF.Semantic<TS, TR>(fs => g(fs.Evaluate(TF.MapS(f)).Evaluate(s)));

    //public static ICoSemantic1<TF, TS, TR> DiMap<TF, TS, TI, TO, TR>(
    //    ICoSemantic1<TF, TI, TO> c,
    //    Func<TS, TI> f,
    //    Func<TO, TR> g
    //)
    //    where TF : IFunctor<TF>
    //    => TF.CoSemantic<TS, TR>(s => c.CoEvaluate(f(s), TF.Semantic<TO, IS<TF, TR>>(fs => fs.Select(g))));
}