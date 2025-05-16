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

public sealed class DistributeFunctorIdentity<F> : IDistributive<F, Identity>
    where F : IFunctor<F>
{
    public ISemantic1<F, IS<Identity, T>, IS<Identity, IS<F, T>>> Distribute<T>()
        => Kind1K<F>.Semantic<IS<Identity, T>, IS<Identity, IS<F, T>>>(fit =>
            Identity.Wrap(fit.Select(Identity.Unwrap)));
}