using SemanticAlgebra.Control;
using SemanticAlgebra.Data;

namespace SemanticAlgebra.Fix;

public sealed record class Fix<F>(IS<F, Fix<F>> Unfix)
    where F : IFunctor<F>
{
    public T Fold<W, T>(Folder<F, W, T> folder)
        where W : IComonad<W>
        => folder.Fold(this);

    public T Fold<T>(ISemantic1<F, T, T> folder)
        => Fold(new Folder<F, Identity, T>(
            new DistributeFunctorIdentity<F>(),
            // folder.DiMap<F, IS<Identity, T>, T, T, T>(Identity.Unwrap, Prelude.Id)
            folder.CoMap<F, IS<Identity, T>, T, T>(static e => e.Extract())
        ));

    public static ISemantic1<F, Fix<F>, Fix<F>> SyntaxFactory =>
        F.Id<Fix<F>>().Compose(e => e.Fix());
}

public static class FixExtension
{
    public static Fix<F> Fix<F>(this IS<F, Fix<F>> e)
        where F : IFunctor<F>
        => new(e);
}

// given forall a. f w a -> w f a
//   and f w t -> t
// fold :: fix f -> t
public sealed record class Folder<F, W, T>(
    IDistributive<F, W> Distribute,
    ISemantic1<F, IS<W, T>, T> Semantic
)
    where F : IFunctor<F>
    where W : IComonad<W>
{
    public T Fold(Fix<F> e) => Step(e).Evaluate(W.ExtractS<T>());

    private IS<W, T> Step(Fix<F> value)
    {
        var nested = value.Unfix.Select(fx => Step(fx).Duplicate());
        var swap = nested.Evaluate(Distribute.Distribute<IS<W, T>>());
        return swap.Select(fwt => fwt.Evaluate(Semantic));
    }
}