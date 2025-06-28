using SemanticAlgebra.Data;

namespace SemanticAlgebra.Control;

public interface IExtract<F> : IFunctor<F>
    where F : IExtract<F>
{
    static abstract ISemantic1<F, T, T> ExtractS<T>();
}

public static partial class PreludeExtension
{
    public static T Extract<F, T>(this IS<F, T> e)
        where F : IExtract<F>
        => e.Evaluate(F.ExtractS<T>());
}
