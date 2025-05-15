namespace SemanticAlgebra;

public interface IKind1<TF>
    where TF : IKind1<TF>
{
    // aligns with the fact that semantic is encoding of f s -> r
    // this implies forall f a, there exists function id :: f a -> f a
    abstract static ISemantic1<TF, TS, TR> Semantic<TS, TR>(Func<IS<TF, TS>, TR> f);
    virtual static ICoSemantic1<TF, TS, TR> CoSemantic<TS, TR>(Func<TS, IS<TF, TR>> f)
        => new FuncCoSemantic<TF, TS, TR>(f);

    virtual static IDiSemantic<TF, TS, TR> DiSemantic<TS, TR>(Func<IS<TF, TS>, IS<TF, TR>> f)
        => new FuncDiSemantic<TF, TS, TR>(f);

    virtual static IDiSemantic<TF, TS, TR> Compose<TS, TM, TR>(
        ISemantic1<TF, TS, TM> f,
        ICoSemantic1<TF, TM, TR> g
    ) => new ComposedDiSemantic<TF, TS, TM, TR>(f, g);
    virtual static Func<TS, TR> Compose<TS, TM, TR>(
        ICoSemantic1<TF, TS, TM> f,
        ISemantic1<TF, TM, TR> g
    ) => s => f.CoEvaluate(s, g);

    virtual static IDiSemantic<TF, TS, TR> ComposeF<TS, TM, TR>(
        ICoSemantic1<TF, IS<TF, TS>, TM> f,
        ISemantic1<TF, TM, IS<TF, TR>> g
    ) => TF.DiSemantic<TS, TR>(fs => f.CoEvaluate(fs, g));

    virtual static Func<TS, IS<TF, TR>> ToFunc<TS, TR>(ICoSemantic1<TF, TS, TR> coSemantic)
        => s => coSemantic.CoEvaluate(s, TF.Id<TR>().Semantic);
    virtual static Func<IS<TF, TS>, TR> ToFunc<TS, TR>(ISemantic1<TF, TS, TR> semantic)
        => fs => fs.Evaluate(semantic);
    virtual static Func<IS<TF, TS>, IS<TF, TR>> ToFunc<TS, TR>(IDiSemantic<TF, TS, TR> semantic)
        => TF.ToFunc(semantic.Semantic);

    virtual static IDiSemantic<TF, T, T> Id<T>()
        => TF.DiSemantic<T, T>(static x => x);
}

public static class Kind1K<TF>
    where TF : IKind1<TF>

{
    public static IDiSemantic<TF, T, T> Id<T>() => TF.Id<T>();
}

sealed record class DiSemanticFromSemantic<TF, TS, TR>(ISemantic1<TF, TS, IS<TF, TR>> Semantic)
    : IDiSemantic<TF, TS, TR>
    where TF : IKind1<TF>
{
    public TO CoEvaluate<TO>(IS<TF, TS> x, ISemantic1<TF, TR, TO> semantic)
        => x.Evaluate(Semantic).Evaluate(semantic);
}

public interface IKind2<TF> where TF : IKind2<TF>
{
}

