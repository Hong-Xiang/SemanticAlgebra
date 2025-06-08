using SemanticAlgebra.Control;
using SemanticAlgebra.Data;
using SemanticAlgebra.Syntax;

namespace SemanticAlgebra.Free;

public abstract partial class Free<F> : IMonad<Free<F>>
    where F : IFunctor<F>
{
    [Semantic1]
    public interface ISemantic<TS, out TR>
        : ISemantic1<Free<F>, TS, TR>
    {
        TR Pure(TS v);
        TR Roll(IS<F, IS<Free<F>, TS>> v);
    }


    public static ISemantic1<Free<F>, IS<Free<F>, T>, IS<Free<F>, T>> JoinS<T>()
        => new JoinSemantic<T>();

    public static ISemantic1<Free<F>, TS, IS<Free<F>, TR>> MapS<TS, TR>(Func<TS, TR> f)
        => new MapSemantic<TS, TR>(f);

    public static IS<Free<F>, T> Pure<T>(T x)
        => B.Pure(x);

    sealed class MapSemantic<TS, TR>(Func<TS, TR> Func) : ISemantic<TS, IS<Free<F>, TR>>
    {
        public IS<Free<F>, TR> Pure(TS v)
            => B.Pure(Func(v));

        public IS<Free<F>, TR> Roll(IS<F, IS<Free<F>, TS>> v)
            => B.Roll(v.Select(vf => vf.Select(Func)));
    }

    sealed class JoinSemantic<T> : ISemantic<IS<Free<F>, T>, IS<Free<F>, T>>
    {
        public IS<Free<F>, T> Pure(IS<Free<F>, T> v)
            => v;

        public IS<Free<F>, T> Roll(IS<F, IS<Free<F>, IS<Free<F>, T>>> v)
            => B.Roll(v.Select(vi => vi.Evaluate(this)));
    }
}