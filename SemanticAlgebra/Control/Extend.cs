using SemanticAlgebra.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticAlgebra.Control;

public interface IExtend<TF> : IFunctor<TF>
    where TF : IExtend<TF>
{
    // (f s -> r) -> (f s -> f r)
    static abstract IDiSemantic<TF, TS, TR> Extend<TS, TR>(ISemantic1<TF, TS, TR> s);

    // f s -> f (f s)
    static virtual ISemantic1<TF, T, IS<TF, IS<TF, T>>> Duplicate<T>()
        => TF.Extend(TF.Id<T>().Semantic).Semantic;
}
