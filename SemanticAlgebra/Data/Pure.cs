namespace SemanticAlgebra.Data;

public interface IPure<TF> : IKind1<TF>
    where TF : IPure<TF>
{
    static abstract IS<TF, T> Pure<T>(T x);
}

public static class PureK<F>
    where F : IPure<F>
{
    public static IS<F, T> Pure<T>(T x) => F.Pure(x);
}
