using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticAlgebra.Data;

public interface IFunctor<TF> : IKind1<TF>
    where TF : IFunctor<TF>
{

    virtual static IDiSemantic<TF, TS, TR> MapSemantic<TS, TR>(Func<TS, TR> f)
        => new Functor1DiMapDiSemantic<TF, TS, TR>(f);

    virtual static ISemantic1<TF, TS, TR> SemanticDiMap<TS, TI, TO, TR>(
        ISemantic1<TF, TI, TO> semantic,
        Func<TS, TI> f,
        Func<TO, TR> g
    ) => TF.Semantic<TS, TR>(fs => g(fs.Select(f).Eval(semantic)));

    virtual static ICoSemantic1<TF, TS, TR> CoSemanticDiMap<TS, TI, TO, TR>(
        ICoSemantic1<TF, TI, TO> semantic,
        Func<TS, TI> f,
        Func<TO, TR> g
    ) => TF.CoSemantic<TS, TR>(s => semantic.CoEvaluate<ISemantic1<TF, TO, IS<TF, TR>>, IS<TF, TR>>(f(s), TF.Semantic<TO, IS<TF, TR>>(fs => fs.Select(g))));
}

sealed record class Functor1DiMapDiSemantic<TF, TS, TR>(Func<TS, TR> F) : IDiSemantic<TF, TS, TR>
    where TF : IFunctor<TF>
{
    public ISemantic1<TF, TS, IS<TF, TR>> Semantic
        => TF.SemanticDiMap(TF.IdSemantic<TR>(), F, Prelude.Id);

    public TO CoEvaluate<TSemantic, TO>(IS<TF, TS> x, TSemantic semantic)
        where TSemantic : ISemantic1<TF, TR, TO>
        => x.Eval(TF.SemanticDiMap(semantic, F, Prelude.Id));
}

