using SemanticAlgebra.Data;

namespace SemanticAlgebra.Control;

public interface IExtract<F> : IFunctor<F>
    where F : IExtract<F>
{
    static abstract ISemantic1<F, T, T> ExtractS<T>();
}
