using SemanticAlgebra.Data;
namespace SemanticAlgebra.CoData;

public interface IDiMapSemantic<TF> : IFunctor<TF>
    where TF : IDiMapSemantic<TF>
{
    abstract static ISemantic1<TF, TS, TR> DiMap<TS, TI, TO, TR>(
        ISemantic1<TF, TI, TO> semantic,
        Func<TS, TI> f,
        Func<TO, TR> g
    );
    static IDiSemantic<TF, TS, TR> IFunctor<TF>.MapSemantic<TS, TR>(Func<TS, TR> f)
        => new DiMapDiSemantic<TF, TS, TR>(f);
}

sealed record class DiMapDiSemantic<TF, TS, TR>(Func<TS, TR> F) : IDiSemantic<TF, TS, TR>
    where TF : IDiMapSemantic<TF>
{
    public ISemantic1<TF, TS, IS<TF, TR>> Semantic
        => TF.DiMap(TF.IdSemantic<TR>(), F, Prelude.Id);

    public TO CoEvaluate<TSemantic, TO>(IS<TF, TS> x, TSemantic semantic)
        where TSemantic : ISemantic1<TF, TR, TO>
        => x.Eval(TF.DiMap(semantic, F, Prelude.Id));
}
