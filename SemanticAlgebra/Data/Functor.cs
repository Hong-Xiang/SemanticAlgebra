using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticAlgebra.Data;

public interface IFunctor<TF> : IKind1<TF>
    where TF : IFunctor<TF>
{
    static abstract IDiSemantic<TF, TS, TR> Map<TS, TR>(Func<TS, TR> f);
}

public static class FunctorExtension
{
    public static ISemantic1<TF, TS, TR> DiMap<TF, TS, TI, TO, TR>(
        this ISemantic1<TF, TI, TO> s,
        Func<TS, TI> f,
        Func<TO, TR> g
    )
        where TF : IFunctor<TF> =>
        TF.Map(f).ComposeSToS(s.ComposeF(g));

    public static ICoSemantic1<TF, TS, TR> DiMap<TF, TS, TI, TO, TR>(
        ICoSemantic1<TF, TI, TO> c,
        Func<TS, TI> f,
        Func<TO, TR> g
    )
        where TF : IFunctor<TF>
        => TF.CoSemantic<TS, TR>(s => c.CoEvaluate(f(s), TF.Semantic<TO, IS<TF, TR>>(fs => fs.Select(g))));
}