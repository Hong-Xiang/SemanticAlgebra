using SemanticAlgebra.Data;

namespace SemanticAlgebra;

public static class Alias1
{
    public interface ISemantic<F, in TS, out TR, in T> : ISemantic1<F, TS, TR>
        where F : IKind1<F>
    {
        TR From(T value);
    }

    public static class B
    {
        public static IS<F, T> From<F, T, TV>(TV value)
            where F : IKind1<F>
            => new D.From<F, T, TV>(value);
    }

    public static class D
    {
        public sealed record class From<F, T, TV>(TV Value) : IS<F, T>
            where F : IKind1<F>
        {
            public TR Evaluate<TR>(ISemantic1<F, T, TR> semantic)
            {
                var sem = (Alias1.ISemantic<F, T, TR, TV>)semantic;
                return sem.From(Value);
            }
        }
    }

    public static ISemantic1<F, T, IS<F, T>> Id<F, T, V>()
        where F : IKind1<F>
        => new IdSemantic<F, T, V>();

    public static ISemantic1<F, TS, TR> Compose<F, TS, TI, TR, TV>(ISemantic1<F, TS, TI> s, Func<TI, TR> f)
        where F : IKind1<F>
        => new ComposeSemantic<F, TS, TI, TR, TV>(s, f);

    sealed class IdSemantic<F, T, TV>
        : ISemantic<F, T, IS<F, T>, TV>
        where F : IKind1<F>
    {
        public IS<F, T> From(TV value)
            => new D.From<F, T, TV>(value);
    }

    sealed record class ComposeSemantic<F, TS, TI, TR, TV>(
        ISemantic1<F, TS, TI> s,
        Func<TI, TR> f
    )
        : ISemantic<F, TS, TR, TV>
        where F : IKind1<F>
    {
        public TR From(TV value) =>
            f(((ISemantic<F, TS, TI, TV>)s).From(value));
    }
}