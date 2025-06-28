namespace SemanticAlgebra.Data;

public interface INaturalTransform<F, G>
    where F : IFunctor<F>
    where G : IFunctor<G>
{
    IS<G, T> Invoke<T>(IS<F, T> e);
}
