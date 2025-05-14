using SemanticAlgebra.CoData;

namespace SemanticAlgebra;

public interface IKind1<TF>
    where TF : IKind1<TF>
{
    // aligns with the fact that semantic is encoding of f s -> r
    // this implies forall f a, there exists function id :: f a -> f a
    abstract static ISemantic1<TF, TS, TR> Semantic<TS, TR>(Func<IS<TF, TS>, TR> f);

    virtual static ISemantic1<TF, T, IS<TF, T>> IdSemantic<T>()
        => TF.Semantic<T, IS<TF, T>>(static x => x);
}

public interface IKind2<TF> where TF : IKind2<TF>
{
}
