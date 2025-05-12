using SemanticAlgebra.CoData;

namespace SemanticAlgebra;

public interface IKind1<TF> where TF : IKind1<TF>
{
    // forall f a, there exists function id :: f a -> f a
    abstract static ISemantic1<TF, T, IS<TF, T>> IdSemantic<T>();
}

public interface IKind2<TF> where TF : IKind2<TF>
{
}
