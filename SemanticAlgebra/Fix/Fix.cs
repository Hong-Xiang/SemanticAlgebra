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
        F.Id<Fix<F>>().Compose(static e => e.Fix());

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

// unfold : a -> f ( fix f | a )

// free f a = pure a | nest f (free f a)

// either l r = l | r

// free (const l) r = pure r | nest (const l) * ~ l | r

// f (free f a)

// distributive m f a = free (const (fix f)) (f a) - f (free (const (fix f)) a)

// pure (f a) -> (f a).Select(Pure)
// roll e:(fix f) ->  (e.unfix : f (fix f)).Select(n -> (const n).LiftF())


// == gcata ==
// dist :: forall x. f (w x) -> w (f x)
// alg  :: f (w t) -> t
// fold :: fix f   -> t
public sealed class Folder<F, W, T>(
    IDistributive<F, W> Dist,
    ISemantic1<F, IS<W, T>, T> Alg
)
    where F : IFunctor<F>
    where W : IComonad<W>
{
    Recurse<F, Identity, W, Fix<F>, T> Recurse = new(
        new DistributeIdentityFunctor<F>(),
        e => e.Unfix.Select(Identity.B.From),
        Dist,
        Alg);

    public T Fold(Fix<F> e) => Recurse.Run(e);

    //public T Fold(Fix<F> e) => Step(e).Extract();

    //private IS<W, T> Step(Fix<F> value)
    //{
    //    var fwwt = value.Unfix.Select(fx => Step(fx).Duplicate());
    //    var wfwt = Dist.Distribute(fwwt);
    //    return wfwt.Select(fwt => fwt.Evaluate(Alg));
    //}
}

// == gana ==
// dist   :: forall x. m (f x) -> f (m x)
// co_alg :: t -> f (m t)
// unfold :: t -> fix f
public sealed class UnFolder<F, M, T>(
    IDistributive<M, F> Dist,
    Func<T, IS<F, IS<M, T>>> CoAlg
)
    where F : IFunctor<F>
    where M : IMonad<M>
{
    public Fix<F> Unfold(T v) => Step(M.Pure(v));
    private Fix<F> Step(IS<M, T> e)
    {
        var mfmt = e.Select(CoAlg);
        var fmmt = Dist.Distribute(mfmt);
        var fft = fmmt.Select(x => x.Join());
        return fft.Select(Step).Fix();
    }
}

// ghylo
// dmf   :: forall x. m (f x) -> f (m x)
// dfw   :: forall x. f (w x) -> w (f x)
// alg   :: f (w r) -> r
// coalg :: s -> f (m s)
// s -> r


// gcata is ghylo with s = Fix f,
// m = Identity,
// coalg is Fix f -> f (Fix f), thus unfix
// dmf is Identity (f x) -> f (Identity x), thus (map from) . unwrap

// gana is ghylo with r = Fix f,
// w = Identity,
// alg is f (Fix f) -> Fix f, thus fix
// dfw is f (Identity x) -> Identity (f x), thus from . (map unwarp) 

// TODO : further efficiency on https://blog.sumtypeofway.com/posts/recursion-schemes-part-5.html
public sealed class RecurseFix<F, M, W, TS, TR>(
    IDistributive<M, F> DistMF,
    Func<TS, IS<F, IS<M, TS>>> CoAlg,
    IDistributive<F, W> DistFW,
    ISemantic1<F, IS<W, TR>, TR> Alg)
    where F : IFunctor<F>
    where M : IMonad<M>
    where W : IComonad<W>
{
    private TR Fold(Fix<F> e) => StepR(e).Extract();

    private IS<W, TR> StepR(Fix<F> value)
    {
        var fwwr = value.Unfix.Select(fx => StepR(fx).Duplicate());
        var wfwr = DistFW.Distribute(fwwr);
        return wfwr.Select(fwr => fwr.Evaluate(Alg));
    }
    private Fix<F> Unfold(TS v) => StepS(M.Pure(v));
    private Fix<F> StepS(IS<M, TS> e)
    {
        var mfms = e.Select(CoAlg);
        var fmms = DistMF.Distribute(mfms);
        var ffs = fmms.Select(x => x.Join());
        return ffs.Select(StepS).Fix();
    }
    public TR Run(TS a) => Fold(Unfold(a));
}

public sealed class Recurse<F, M, W, TS, TR>(
    IDistributive<M, F> DistMF,
    Func<TS, IS<F, IS<M, TS>>> CoAlg,
    IDistributive<F, W> DistFW,
    ISemantic1<F, IS<W, TR>, TR> Alg)
    where F : IFunctor<F>
    where M : IMonad<M>
    where W : IComonad<W>
{
    private IS<F, IS<M, TS>> StepM(IS<M, TS> ms)
    {
        var mfms = ms.Select(CoAlg);
        var fmms = DistMF.Distribute(mfms);
        var fms = fmms.Select(x => x.Join());
        return fms;
    }
    private IS<W, TR> StepW(IS<F, IS<W, TR>> fwr)
    {
        var fwwr = fwr.Select(wr => wr.Duplicate());
        var wfwr = DistFW.Distribute(fwwr);
        var wr = wfwr.Select(fwr => fwr.Evaluate(Alg));
        return wr;
    }

    private IS<W, TR> Step(IS<M, TS> ms)
    {
        var fms = StepM(ms);
        var fwr = fms.Select(Step);
        var wr = StepW(fwr);
        return wr;
    }

    public TR Run(TS a) => Step(M.Pure(a)).Extract();
}


sealed class Hylo<F, TS, TR>(
    Func<TS, IS<F, TS>> coalg,
    ISemantic1<F, TR, TR> alg
) where F : IFunctor<F>
{
    public TR Run(TS value)
    {
        return coalg(value).Select(Run).Evaluate(alg);
    }
}

//gunfold, gana
//  :: (Corecursive t, Monad m)
//  => k : (forall b. m(Base t b) -> Base t(m b)) -- ^ a distributive law
//  -> f : (a -> Base t(m a))                      -- ^ a(Base t)-m-coalgebra
//  -> a                                        -- ^ seed
//  -> Fix f
//gana k f = g . return . f where
//  g = embed . fmap(g . liftM f . join) . k

// basically 
// fu b : f (m b)
// | pure : m (f (m b))
// | 

// g : m (f (m b)) -> Fix f
// | fd : f (m (m b))
// let h :  m (m b) -> Fix f
// | map h : f (Fix f)
// | fix


// fd : forall x. m (f x) -> f (m x)
// fu : t -> f (m t)
// t -> fix f


// fu t : f (m t)
// pure : m (f (m t))
// fd : f (m (m t))
// -- step
// map join : f (m t)
// map (map fu) : f (m (f (m t)))
// map fd : f (f (m (m t)))
// map (map join) : f (f (m t)) 


// t
// | fu : f (m t)
// | map map fu : f (m (f (m t))
// | map map .... : f m f m f m .... f m t
// dist             f (m f m) f m .....
//                  f (f m m) f m .....
//                  f (f m f m ..... )



//public T Fold(Fix<F> e) => Step(e).Extract();

//private IS<W, T> Step(Fix<F> value)
//{
//    var nested = value.Unfix.Select(fx => Step(fx).Duplicate());
//    var swap = Distribute.Distribute(nested);
//    return swap.Select(fwt => fwt.Evaluate(Semantic));
//}



// dm :: m f x -> f m x
// dw :: f w x -> w f x
// u : a -> f m a
// f : f w b -> b
// a -> b


// a
// f m a
// 


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


// Recurse
// Functor f, Monad m, Comonad w
// => forall a. m (f a) -> f (m a)
// -> forall b. f (w b) -> w (f b)
// -> a -> f (m a)
// -> f (w b) -> b
// -> a -> b

// implementation 



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
