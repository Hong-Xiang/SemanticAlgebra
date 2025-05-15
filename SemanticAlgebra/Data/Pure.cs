namespace SemanticAlgebra.Data;

public interface IPure<TF> : IKind1<TF>
    where TF : IPure<TF>
{
    abstract static ICoSemantic1<TF, T, T> Pure<T>();
}

public static partial class PureK<TF>
    where TF : IPure<TF>
{
    public static IS<TF, T> Pure<T>(T value)
        => TF.Pure<T>()
             .CoEvaluate(value, TF.Id<T>().Semantic);
}
