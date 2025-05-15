using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticAlgebra.Data;

public interface IFunctor<TF> : IKind1<TF>
    where TF : IFunctor<TF>
{

    abstract static IDiSemantic<TF, TS, TR> Map<TS, TR>(Func<TS, TR> f);

    virtual static ISemantic1<TF, TS, TR> SemanticDiMap<TS, TI, TO, TR>(
        ISemantic1<TF, TI, TO> semantic,
        Func<TS, TI> f,
        Func<TO, TR> g
    ) => TF.Semantic<TS, TR>(fs => g(fs.Select(f).Evaluate(semantic)));

    virtual static ICoSemantic1<TF, TS, TR> CoSemanticDiMap<TS, TI, TO, TR>(
        ICoSemantic1<TF, TI, TO> semantic,
        Func<TS, TI> f,
        Func<TO, TR> g
    ) => TF.CoSemantic<TS, TR>(s => semantic.CoEvaluate(f(s), TF.Semantic<TO, IS<TF, TR>>(fs => fs.Select(g))));
    static ISemantic1<TF, TS, TR> IKind1<TF>.Semantic<TS, TR>(Func<IS<TF, TS>, TR> f)
        => TF.SemanticDiMap<TS, TS, IS<TF, TS>, TR>(TF.Id<TS>().Semantic, Prelude.Id, f);

    abstract static IDiSemantic<TF, T, T> IKind1<TF>.Id<T>();
}

sealed record class Functor1DiMapDiSemantic<TF, TS, TR>(Func<TS, TR> F) : IDiSemantic<TF, TS, TR>
    where TF : IFunctor<TF>
{
    public ISemantic1<TF, TS, IS<TF, TR>> Semantic
        => TF.SemanticDiMap(TF.Id<TR>().Semantic, F, Prelude.Id);

    public TO CoEvaluate<TO>(IS<TF, TS> x, ISemantic1<TF, TR, TO> semantic)
        => x.Evaluate(TF.SemanticDiMap(semantic, F, Prelude.Id));
}



