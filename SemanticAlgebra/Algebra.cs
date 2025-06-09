using SemanticAlgebra.Control;
using SemanticAlgebra.Data;

namespace SemanticAlgebra;

public interface IAlgebra<TAlg, T>
    where TAlg : IAlgebra<TAlg, T>
{
    public static T Fold<F>(Fix.Fix<F> e)
        where F : IFunctor<F>, IImplements<F, TAlg, T>
        => e.Fold(F.Get());
}

public interface IImplements<F, TAlg, T> : IFunctor<F>
    where TAlg : IAlgebra<TAlg, T>
    where F : IImplements<F, TAlg, T>
{
    static abstract ISemantic1<F, T, T> Get();

}

public interface IImplementsM<F, S, T> : IFunctor<F>
    where F : IImplementsM<F, S, T>
{
    static abstract ISemantic1<F, IS<M, T>, IS<M, T>> Get_<M>()
        where M : IMonadState<M, S>;

    public static ISemantic1<F, IS<M, T>, IS<M, T>> Get<M>()
        where M : IMonadState<M, S>
        => F.Get_<M>();

}

