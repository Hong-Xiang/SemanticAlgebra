using SemanticAlgebra.Data;
namespace SemanticAlgebra.CoData;

public interface IDiMapSemantic<TF> 
    : IFunctor<TF>
    where TF : IDiMapSemantic<TF>
{
    abstract static ISemantic1<TF, TS, TR> DiMap<TS, TI, TO, TR>(
        ISemantic1<TF, TI, TO> semantic,
        Func<TS, TI> f,
        Func<TO, TR> g
    );
    static ISemantic1<TF, TS, IS<TF, TR>> IFunctor<TF>.MapSemantic<TS, TR>(Func<TS, TR> f)
        => TF.DiMap(TF.IdSemantic<TR>(), f, Prelude.Id);
}
