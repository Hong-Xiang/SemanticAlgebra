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
    ISemantic1<F, IS<G, T>, IS<G, IS<F, T>>> Distribute<T>();
}

public static class DistributiveExtension
{
}

public sealed class DistributeFunctorIdentity<G> : IDistributive<G, Identity>
    where G : IFunctor<G>
{
    public ISemantic1<G, IS<Identity, T>, IS<Identity, IS<G, T>>> Distribute<T>()
        => Kind1K<G>.Semantic<IS<Identity, T>, IS<Identity, IS<G, T>>>(fit =>
            Identity.Pure(fit.Select(static e => e.Extract())));
}