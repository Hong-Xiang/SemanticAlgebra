using SemanticAlgebra.Control;
using SemanticAlgebra.Data;

namespace SemanticAlgebra.Fix;

public sealed record class Fix<F>(IS<F, Fix<F>> Unfix)
    where F : IFunctor<F>
{
    public T FoldW<W, T>(Folder<F, W, T> folder)
        where W : IComonad<W>
        => folder.Fold(this);

    public T Fold<T>(ISemantic1<F, T, T> folder)
        => FoldW(new Folder<F, Identity, T>(
            new DistributeFunctorIdentity<F>(),
            // folder.DiMap<F, IS<Identity, T>, T, T, T>(Identity.Unwrap, Prelude.Id)
            folder.CoMap<F, IS<Identity, T>, T, T>(static e => e.Extract())
        ));

    public static ISemantic1<F, Fix<F>, Fix<F>> SyntaxFactory =>
        F.Id<Fix<F>>().Compose(e => e.Fix());

    public static Fix<F> Unfold<T>(Func<T, IS<F, T>> f, T value)
        => f(value).Select(t => Unfold(f, t)).Fix();

    sealed class UnfoldAEitherSemantic<T>(
        Func<T, IS<F, IS<Either<Fix<F>>, T>>> f)
        : Either<Fix<F>>.ISemantic<T, Fix<F>>
    {
        public Fix<F> Left(Fix<F> value)
            => value;

        public Fix<F> Right(T value)
            => Fix<F>.UnfoldA(f, value);
    }

    public static Fix<F> UnfoldA<T>(Func<T, IS<F, IS<Either<Fix<F>>, T>>> f, T value)
        => f(value).Select(e => e.Evaluate(new UnfoldAEitherSemantic<T>(f))).Fix();

    public Fix<F> UnfoldA(ISemantic1<F, Fix<F>, IS<F, IS<Either<Fix<F>>, Fix<F>>>> f)
        => Fix<F>.UnfoldA<Fix<F>>(e => e.Unfix.Evaluate(f), this);

    public override string ToString() => Unfix.ToString();

    public T FoldP<T>(ISemantic1<F, (Fix<F>, T), T> semantic)
    {
        var inner = Unfix.Select(fe => fe.FoldP(semantic));
        return inner.Select(t => (this, t)).Evaluate(semantic);
    }

    public Fix<F> BottomUp(ISemantic1<F, Fix<F>, IS<F, Fix<F>>> s)
        => Unfix.Select(e => e.BottomUp(s)).Evaluate(s).Fix();

    public Fix<F> TopDown(ISemantic1<F, Fix<F>, IS<F, Fix<F>>> s)
        => Unfix.Evaluate(s).Select(e => e.TopDown(s)).Fix();
}

public static class FixExtension
{
    public static Fix<F> Fix<F>(this IS<F, Fix<F>> e)
        where F : IFunctor<F>
        => new(e);


    public static IS<Either<Fix<F>>, Fix<F>> UnfoldRecursive<F>(this Fix<F> e)
        where F : IFunctor<F>
        => Either<Fix<F>>.B.Right(e);

    public static IS<Either<Fix<F>>, Fix<F>> UnfoldReturn<F>(this Fix<F> e)
        where F : IFunctor<F>
        => Either<Fix<F>>.B.Left<Fix<F>>(e);
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