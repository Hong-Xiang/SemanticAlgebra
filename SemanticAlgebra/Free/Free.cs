using SemanticAlgebra.Control;
using SemanticAlgebra.Data;

namespace SemanticAlgebra.Free;

public abstract class Free<F> : IMonad<Free<F>>
    where F : IFunctor<F>
{
    public static ISemantic1<Free<F>, Func<TS, TR>, ISemantic1<Free<F>, TS, IS<Free<F>, TR>>> ApplyS<TS, TR>()
    {
        throw new NotImplementedException();
    }

    public static ISemantic1<Free<F>, TS, TR> Compose<TS, TI, TR>(ISemantic1<Free<F>, TS, TI> s, Func<TI, TR> f)
    {
        throw new NotImplementedException();
    }

    public static ISemantic1<Free<F>, T, IS<Free<F>, T>> Id<T>()
    {
        throw new NotImplementedException();
    }

    public static ISemantic1<Free<F>, IS<Free<F>, T>, IS<Free<F>, T>> JoinS<T>()
    {
        throw new NotImplementedException();
    }

    public static ISemantic1<Free<F>, TS, IS<Free<F>, TR>> MapS<TS, TR>(Func<TS, TR> f)
    {
        throw new NotImplementedException();
    }

    public static IS<Free<F>, T> Pure<T>(T x)
    {
        throw new NotImplementedException();
    }
}

public interface IFreeSemantic<F, in TI, out TO>
    : ISemantic1<Free<F>, TI, TO>
    where F : IFunctor<F>
{
    TO Pure(TI p);
    TO Roll(IS<F, IS<Free<F>, TI>> v);
}


// FreeF f a r
// | Pure a
// | Roll f r


// Fix (FreeF f a)
// = FreeF f a (Fix (FreeF f a)
// | Pure a
// | Roll f (Fix (FreeF f a))

// interpreter ::
//  Functor f, Monad m =>
//  f ~> m -> Free f ~> m


// Exp a
// | Var a
// | App (Exp a) (Exp a)
// | Lam (Exp (Maybe a))

// ExpF a
// | App (ExpF a) (ExpF a)
// | Lam (ExpF (Maybe a))

// Free f a
// | Pure a
// | 