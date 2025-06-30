using SemanticAlgebra.Control;
using SemanticAlgebra.Data;

namespace SemanticAlgebra.Fix;

public sealed class Recurse<F, TS, TR>(
    Func<TS, IS<F, TS>> CoAlg,
    Func<IS<F, TR>, TR> Alg
)
    where F : IFunctor<F>
{
    public TR Run(TS a) => Alg(CoAlg(a).Select(Run));
}
// functor f, monad m, comonad w
// codist : forall x. m (f x) -> f (m x)
//   dist : froall x. f (w x) -> w (f x)
//  coalg : s -> f (m s)
//    alg : f (w r) -> r
// => (s -> r)
public sealed class Recurse<F, M, W, TS, TR>(
    IDistributeTransform<M, F> CoDist,
    Func<TS, IS<F, IS<M, TS>>> CoAlg,
    IDistributeTransform<F, W> Dist,
    Func<IS<F, IS<W, TR>>, TR> Alg)
    where F : IFunctor<F>
    where M : IMonad<M>
    where W : IComonad<W>
{
    private IS<F, IS<M, TS>> StepM(IS<M, TS> ms)
    {
        var mfms = ms.Select(CoAlg);
        var fmms = CoDist.Distribute(mfms);
        var fms = fmms.Select(x => x.Join());
        return fms;
    }
    private IS<W, TR> StepW(IS<F, IS<W, TR>> fwr)
    {
        var fwwr = fwr.Select(wr => wr.Duplicate());
        var wfwr = Dist.Distribute(fwwr);
        var wr = wfwr.Select(Alg);
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

public static class Recursive
{
    public static Recurse<F, Identity, Identity, TS, TR> Create<F, TS, TR>(
        Func<TS, IS<F, TS>> coalg,
        Func<IS<F, TR>, TR> alg
    )
        where F : IFunctor<F>
        => new(Identity.DistributeTransformFromIdentity<F>(),
            s => coalg(s).Select(Identity.B.From),
            Identity.DistributeTransformToIdentity<F>(),
            fwr => alg(fwr.Select(wr => wr.Unwrap())));

    public static Recurse<F, Identity, W, TS, TR> CreateW<F, W, TS, TR>(
        IDistributeTransform<F, W> dist,
        Func<TS, IS<F, TS>> coalg,
        Func<IS<F, IS<W, TR>>, TR> alg
    )
        where F : IFunctor<F>
        where W : IComonad<W>
        => new(Identity.DistributeTransformFromIdentity<F>(),
            s => coalg(s).Select(Identity.B.From),
            dist,
            alg);

    public static Recurse<F, M, Identity, TS, TR> CreateM<F, M, TS, TR>(
        IDistributeTransform<M, F> dist,
        Func<TS, IS<F, IS<M, TS>>> coalg,
        Func<IS<F, TR>, TR> alg
    )
        where F : IFunctor<F>
        where M : IMonad<M>
        => new(
            dist,
            coalg,
            Identity.DistributeTransformToIdentity<F>(),
            fwr => alg(fwr.Select(wr => wr.Unwrap())));
}


// functor f, monad m, comonad w
// codist : forall x. m (f x) -> f (m x)
//   dist : froall x. f (w x) -> w (f x)
//  coalg : s -> f (m s)
//    alg : f (w r) -> r
// => (s -> r)

// add attribute : fix f -> cofree f a
//                     s -> r
// there would be two direction - top down/bottom up
// typical top-down : add scope level attribute
// typical bottom-up : used labels/values

// lets start with bottom up - using alg
// ideally our alg would be
// f a -> a
// however if we direct substitue our result into recurse we would get
// 

// interprete : free f a -> m a

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



// Free f a = Fix g where g x = a | f x
// Cofree f a = Fix h where h x = (a, f x)


