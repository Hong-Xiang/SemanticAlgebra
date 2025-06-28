using SemanticAlgebra.Data;

namespace SemanticAlgebra.Fix;

/// <summary>
/// encoding <c>forall a. (f (g a)) -> (g (f a))</c>
/// </summary>
/// <typeparam name="F"></typeparam>
/// <typeparam name="G"></typeparam>
public interface IDistributive<F, G>
    where F : IFunctor<F>
    where G : IFunctor<G>
{
    IS<G, IS<F, T>> Distribute<T>(IS<F, IS<G, T>> fg);
}

