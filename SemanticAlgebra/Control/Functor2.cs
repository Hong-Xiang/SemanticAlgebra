namespace SemanticAlgebra.Control;

public interface IFunctor2<F>
    where F : IFunctor2<F>
{
    static abstract ISemantic2<F, TA, TB, TR> Semantic<TA, TB, TR>(Func<IS2<F, TA, TB>, TR> f);
}
