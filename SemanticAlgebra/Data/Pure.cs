namespace SemanticAlgebra.Data;

public interface IPure<TF> : IKind1<TF>
    where TF : IPure<TF>
{
    virtual static IS<TF, T> Pure<T>(T value)
        => TF.PureCoSemantic<T>()
             .CoEvaluate<ISemantic1<TF, T, IS<TF, T>>, IS<TF, T>>(value, TF.IdSemantic<T>());
    abstract static ICoSemantic1<TF, T, T> PureCoSemantic<T>();
}
