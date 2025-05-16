using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticAlgebra.Control;

public interface ICoMonad<TF> : IExtend<TF>
    where TF : ICoMonad<TF>
{
    static abstract ISemantic1<TF, T, T> Extract<T>();
}
