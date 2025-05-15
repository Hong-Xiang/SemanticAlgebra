using SemanticAlgebra.Data;

namespace SemanticAlgebra.Control;

public interface IApply<TF> : IFunctor<TF>
    where TF : IApply<TF>
{
    // apply :: f (a -> b) -> f a -> f b
    abstract static ISemantic1<TF, Func<TS, TR>, IDiSemantic<TF, TS, TR>> Apply<TS, TR>();
}
