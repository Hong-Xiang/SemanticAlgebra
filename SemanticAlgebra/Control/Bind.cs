using SemanticAlgebra.Data;

namespace SemanticAlgebra.Control;

public interface IBind<TF> : IFunctor<TF>
    where TF : IBind<TF>
{
    // bind : forall b a. m a -> (a -> m b) -> m b
    //      ~ forall b a. (a -> m b) -> (m a -> m b)
    virtual static IDiSemantic<TF, TA, TB> Bind<TA, TB>(ICoSemantic1<TF, TA, TB> producer)
        => TF.DiSemantic<TA, TB>(fa => fa.Select(TF.ToFunc(producer)).Evaluate(TF.Join<TB>()));

    // join :: forall a. m (m a) -> m a
    virtual static ISemantic1<TF, IS<TF, T>, IS<TF, T>> Join<T>()
        => TF.Bind(TF.Id<T>()).Semantic;
}

public static class BindK<TF>
    where TF : IBind<TF>
{
    public static IS<TF, T> Join<T>(IS<TF, IS<TF, T>> ft)
        => ft.Evaluate(TF.Join<T>());
}
