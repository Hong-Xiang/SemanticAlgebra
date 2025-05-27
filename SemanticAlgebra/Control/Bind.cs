using SemanticAlgebra.Data;

namespace SemanticAlgebra.Control;

public interface IBind<TF> : IFunctor<TF>
    where TF : IBind<TF>
{
    // join :: forall a. m (m a) -> m a
    static abstract ISemantic1<TF, IS<TF, T>, IS<TF, T>> JoinS<T>();
}
