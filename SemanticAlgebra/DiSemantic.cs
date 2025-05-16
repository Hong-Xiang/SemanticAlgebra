namespace SemanticAlgebra;

public interface IDiSemantic<TF, in TI, out TO>
    : ICoSemantic1<TF, IS<TF, TI>, TO>
    where TF : IKind1<TF>
{
    // Don't directly using subtype ISemantic1
    // to avoid accidentally forget implement concrete semantic 
    ISemantic1<TF, TI, IS<TF, TO>> Semantic { get; }
}

sealed record class DiSemanticFromSemantic<TF, TS, TR>(ISemantic1<TF, TS, IS<TF, TR>> Semantic)
    : IDiSemantic<TF, TS, TR>
    where TF : IKind1<TF>
{
    public TO CoEvaluate<TO>(IS<TF, TS> x, ISemantic1<TF, TR, TO> s)
        => x.Evaluate(Semantic).Evaluate(s);
}