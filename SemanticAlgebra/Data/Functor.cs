using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticAlgebra.Data;

public interface IFunctor<TF> : IKind1<TF>
    where TF : IFunctor<TF>
{
    abstract static ISemantic1<TF, TS, IS<TF, TR>> MapSemantic<TS, TR>(Func<TS, TR> f);
}
