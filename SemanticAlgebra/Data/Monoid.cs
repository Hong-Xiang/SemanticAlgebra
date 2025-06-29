namespace SemanticAlgebra.Data;

public interface IMonoid<T>
{
    T Empty();
    T Concat(T l, T r);
}
