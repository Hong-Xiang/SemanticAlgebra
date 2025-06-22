using SemanticAlgebra.Control;
using SemanticAlgebra.Data;
using SemanticAlgebra.Free;

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

// forall a. f (w a) -> w (f a)
// forall b. m (f b) -> f (m b)
// f (w t) -> m t
// Fix f
// m t

// gcata ::
// forall a. f (w a) -> w (f a)
// f (w a) -> a
// Fix f
// a

// gana ::
// forall b. m (f b) -> f (m b)
// b -> f (m b)
// b
// Fix f


// hylo
// Fold
// Unfold
// Refold( fold : f b -> b , unfold : a -> f a ) -> (a -> b)

// when a = Fix f, then unfold is just unfix, Refold is f b -> b -> Fix f -> b
// when b = Fix f, then fold is just fix, Refold is just a -> f a -> a -> Fix b

// if we only consider Free f and CoFree g,
// then we can get simplified distributive low

// e.g. for CoFree g
// forall a. f (Cofree g a) -> (Cofree g) (f a)
// is f (a, g (Cofree g a)) -> (f a, g (Cofree g) (f a))
// which f a could be easily get by fmap fst
// and fmap snd on argument would get f (g (Cofree g a))
// apply distributive, we got g (f (Cofree g a))
// then we need a function to map f (Cofree g a) to (Cofree g) (f a)
// which exactly distributive itself, thus we need recusive in this case

// Let Sum a f x = a | f x
// Free f a = a | f (Free f a) 
// Fix (Sum a f) = (Sum a f) (Fix (Sum a f))
//               = a | f (Fix (Sum a f))


// Free f a = Fix g where g x = a | f x
// Cofree f a = Fix h where h x = (a, f x)

// then gholyf becomes
// ghyloFreeCoFree
//  :: (Functor f, Functor g, Functor h)
//  => (forall x. g(f x) -> f(g x)) -- ^ Distributive law for g over f
//  -> (forall x. f(h x) -> h(f x)) -- ^ Distributive law for f over h
//  -> (f (CoFree h b) -> b)          -- ^ The cofree algebra(fold)
//  -> (a -> f(Free g a))            -- ^ The free coalgebra(unfold)
//  -> a                              -- ^ The initial seed
//  -> b                              -- ^ The final result

// now lets try Free f 's interpret
// interpret :: Functor f, Monad m => f ~> m -> (Free f) ~> m
// thus 
// forall a. 
// forall b. f b -> m b
// Free f a
// m b
