using SemanticAlgebra.Data;

namespace SemanticAlgebra;

public interface IAlgebra<TAlg, T>
    where TAlg : IAlgebra<TAlg, T>
{
    public static T Fold<F>(Fix.Fix<F> e)
        where F : IFunctor<F>, IWithAlgebra<F, TAlg, T>
        => e.Fold(F.Get());
}

public interface IWithAlgebra<F, TAlg, T> : IFunctor<F>
    where TAlg : IAlgebra<TAlg, T>
    where F : IWithAlgebra<F, TAlg, T>
{
    static abstract ISemantic1<F, T, T> Get();
}
