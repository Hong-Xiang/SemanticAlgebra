using SemanticAlgebra.Data;

namespace SemanticAlgebra.Control;

public interface IApply<TF> : IFunctor<TF>
    where TF : IApply<TF>
{
    // apply :: f (a -> b) -> f a -> f b
    static abstract ISemantic1<TF, Func<TS, TR>, ISemantic1<TF, TS, IS<TF, TR>>> ApplyS<TS, TR>();
}
