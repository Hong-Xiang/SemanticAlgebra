using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticAlgebra.Data;

public interface IApplicative<TF> : IFunctor<TF>, IPure<TF>
    where TF : IApplicative<TF>
{
    abstract static ISemantic1<TF, Func<TS, TR>, IDiSemantic<TF, TS, TR>> ApplySemantic<TS, TR>();
}
