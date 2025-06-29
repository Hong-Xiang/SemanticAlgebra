using SemanticAlgebra.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticAlgebra.Control;

public interface IFoldable<F>
    where F : IKind1<F>
{
    static abstract M FoldMap<T, M>(IMonoid<M> monoid, Func<T, M> f, IS<F, T> ft);
}
