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
    static abstract ISemantic1<TF, TS, IS<TF, TR>> ExtendS<TS, TR>(ISemantic1<TF, TS, TR> f);
}
