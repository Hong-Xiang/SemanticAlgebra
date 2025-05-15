using SemanticAlgebra;
using SemanticAlgebra.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LambdaLang;

// f (g a) -> g (f a)
public interface IDistributeSemantic<F, G, T>
    : ICoSemantic1<G, IS<F, IS<G, T>>, IS<F, T>>
    where F : IFunctor<F>
    where G : IFunctor<G>
{
    ISemantic1<F, IS<G, T>, IS<G, IS<F, T>>> Semantic { get; }
}
