using SemanticAlgebra.Control;
using SemanticAlgebra.Data;
using SemanticAlgebra.Syntax;
using SemanticAlgebra.Control;

namespace SemanticAlgebra.Free;

public abstract partial class Free<F> : IMonad<Free<F>>
    where F : IFunctor<F>
{
    [Semantic1]
    public interface IFreeSemantic<in TS, out TR>
        : ISemantic1<Free<F>, TS, TR>
    {
        TR Pure(TS p);
        TR Roll(IS<F, IS<Free<F>, TS>> v);
    }

    public static ISemantic1<Free<F>, Func<TS, TR>, ISemantic1<Free<F>, TS, IS<Free<F>, TR>>> ApplyS<TS, TR>()
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
        => B.Pure(x);
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