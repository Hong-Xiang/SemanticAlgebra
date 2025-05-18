using SemanticAlgebra.Control;
using SemanticAlgebra.Data;

namespace SemanticAlgebra.Free;

public abstract class Free<F> : IMonad<Free<F>> 
    where F : IFunctor<F>
{
    public static IDiSemantic<Free<F>, T, T> Id<T>()
    {
        throw new NotImplementedException();
    }

    public static ISemantic1<Free<F>, TS, TR> ComposeF<TS, TI, TR>(ISemantic1<Free<F>, TS, TI> s, Func<TI, TR> f)
    {
        throw new NotImplementedException();
    }

    public static IDiSemantic<Free<F>, TS, TR> Map<TS, TR>(Func<TS, TR> f)
    {
        throw new NotImplementedException();
    }

    public static ISemantic1<Free<F>, Func<TS, TR>, IDiSemantic<Free<F>, TS, TR>> Apply<TS, TR>()
    {
        throw new NotImplementedException();
    }

    public static ICoSemantic1<Free<F>, T, T> Pure<T>()
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