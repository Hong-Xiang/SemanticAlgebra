namespace SemanticAlgebra;

public interface IDiSemantic<TF, in TI, out TO>
    : ICoSemantic1<TF, IS<TF, TI>, TO>
    where TF : IKind1<TF>
{
    // Don't directly using subtype ISemantic1
    // to avoid accidentally forget implement concrete semantic 
    ISemantic1<TF, TI, IS<TF, TO>> Semantic { get; }
}

public static class DiSemantic
{
    public static IDiSemantic<TF, TS, TR> FromFunc<TF, TS, TR>(
        Func<IS<TF, TS>, IS<TF, TR>> f
    )
        where TF : IKind1<TF>
        => new FuncDiSemantic<TF, TS, TR>(f);
}

sealed class FuncDiSemantic<TF, TS, TR>(
    Func<IS<TF, TS>, IS<TF, TR>> F) : IDiSemantic<TF, TS, TR>
    where TF : IKind1<TF>
{
    public ISemantic1<TF, TS, IS<TF, TR>> Semantic =>
        TF.Semantic(F);

    public TO CoEvaluate<TSemantic, TO>(IS<TF, TS> x, TSemantic semantic)
        where TSemantic : ISemantic1<TF, TR, TO>
        => F(x).Eval(semantic);
}

sealed class ComposedDiSemantic<TF, TS, TM, TR>(
        ISemantic1<TF, TS, TM> F,
        ICoSemantic1<TF, TM, TR> G
)
    : IDiSemantic<TF, TS, TR>
    where TF : IKind1<TF>
{
    public ISemantic1<TF, TS, IS<TF, TR>> Semantic =>
        TF.Semantic<TS, IS<TF, TR>>(fs => TF.ToFunc(G)(TF.ToFunc(F)(fs)));

    public TO CoEvaluate<TSemantic, TO>(IS<TF, TS> x, TSemantic semantic)
        where TSemantic : ISemantic1<TF, TR, TO>
        => G.CoEvaluate<TSemantic, TO>(x.Eval(F), semantic);
}
