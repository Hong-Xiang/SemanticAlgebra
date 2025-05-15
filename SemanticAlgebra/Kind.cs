namespace SemanticAlgebra;

public interface IKind1<TF>
    where TF : IKind1<TF>
{
    // aligns with the fact that semantic is encoding of f s -> r
    // this implies forall f a, there exists function id :: f a -> f a
    abstract static ISemantic1<TF, TS, TR> Semantic<TS, TR>(Func<IS<TF, TS>, TR> f);
    virtual static ICoSemantic1<TF, TS, TR> CoSemantic<TS, TR>(Func<TS, IS<TF, TR>> f)
        => new FuncCoSemantic<TF, TS, TR>(f);

    virtual static IDiSemantic<TF, TS, TR> DiSemantic<TS, TR, TFS, TFR>(Func<IS<TF, TS>, IS<TF, TR>> f)
        => new FuncDiSemantic<TF, TS, TR>(f);

    virtual static IDiSemantic<TF, TS, TR> Compose<TS, TM, TR>(
        ISemantic1<TF, TS, TM> f,
        ICoSemantic1<TF, TM, TR> g
    ) => new ComposedDiSemantic<TF, TS, TM, TR>(f, g);
    virtual static Func<TS, TR> Compose<TS, TM, TR>(
        ICoSemantic1<TF, TS, TM> f,
        ISemantic1<TF, TM, TR> g
    ) => s => f.CoEvaluate<ISemantic1<TF, TM, TR>, TR>(s, g);
    virtual static Func<TS, IS<TF, TR>> ToFunc<TS, TR>(ICoSemantic1<TF, TS, TR> coSemantic)
        => s => coSemantic.CoEvaluate<ISemantic1<TF, TR, IS<TF, TR>>, IS<TF, TR>>(s, TF.IdSemantic<TR>());
    virtual static Func<IS<TF, TS>, TR> ToFunc<TS, TR>(ISemantic1<TF, TS, TR> semantic)
        => fs => fs.Evaluate<ISemantic1<TF, TS, TR>, TR>(semantic);

    virtual static Func<IS<TF, TS>, IS<TF, TR>> ToFunc<TS, TR>(IDiSemantic<TF, TS, TR> semantic)
        => TF.ToFunc(semantic.Semantic);

    virtual static ISemantic1<TF, T, IS<TF, T>> IdSemantic<T>()
        => TF.Semantic<T, IS<TF, T>>(static x => x);
}

public interface IKind2<TF> where TF : IKind2<TF>
{
}

