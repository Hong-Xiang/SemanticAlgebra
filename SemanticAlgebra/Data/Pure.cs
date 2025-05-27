namespace SemanticAlgebra.Data;

public interface IPure<TF> : IKind1<TF>
    where TF : IPure<TF>
{
    static abstract IS<TF, T> Pure<T>(T x);
}
