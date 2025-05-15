using SemanticAlgebra.Data;

namespace SemanticAlgebra.Fix;

public interface IDistributive<F, G>
    where F : IFunctor<F>
    where G : IFunctor<G>
{
    ISemantic1<F, IS<G, T>, IS<G, IS<F, T>>> Distribute<T>();
}

public static class DistributiveExtension
{
}
