using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticAlgebra.Data;

public interface IMonad<TF> : IApplicative<TF>
    where TF : IMonad<TF>
{
    abstract static ISemantic1<TF, IS<TF, T>, IS<TF, T>> JoinSemantic<T>();
}
