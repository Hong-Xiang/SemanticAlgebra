using SemanticAlgebra.Data;

namespace SemanticAlgebra;

public static class Alias1
{
    //public interface ISemantic<F, in TS, out TR, in T> : ISemantic1<F, TS, TR>
    //    where F : IKind1<F>
    //{
    //    TR From(T value);
    //}

    public interface ISpec<F, in TS, TV>
        where F : IKind1<F>
    {
        public static TV Unwrap(IS<F, TS> e) => ((D.From<F, TS, TV>)e).Value;
        public static ISemantic1<F, TS, TR> Compose<TI, TR>(ISemantic1<F, TS, TI> s, Func<TI, TR> f)
              => Alias1.Compose<F, TS, TI, TR, TV>(s, f);
        public static ISemantic1<F, TS, IS<F, TS>> Id()
            => Alias1.Id<F, TS, TV>();
        public static IS<F, TS> From(TV value) => B.From<F, TS, TV>(value);

        public interface ISemantic<out TR> : ISemantic1<F, TS, TR>
        {
            TR From(TV value);
        }
        public static ISemantic<TR> Semantic<TR>(Func<TV, TR> f)
            => new AliasFuncSemantic<F, TS, TV, TR>(f);
        public static ISemantic<TR> Prj<TR>(ISemantic1<F, TS, TR> s)
            => (ISemantic<TR>)s;
    }

    sealed class AliasFuncSemantic<F, TS, TV, TR>(Func<TV, TR> Func) : ISpec<F, TS, TV>.ISemantic<TR>
        where F : IKind1<F>
    {
        public TR From(TV value)
            => Func(value);
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
                => ((ISpec<F, T, TV>.ISemantic<TR>)semantic).From(Value);
        }
    }

    public static ISemantic1<F, T, IS<F, T>> Id<F, T, V>()
        where F : IKind1<F>
        => new IdSemantic<F, T, V>();

    public static ISemantic1<F, TS, TR> Compose<F, TS, TI, TR, TV>(ISemantic1<F, TS, TI> s, Func<TI, TR> f)
        where F : IKind1<F>
        => new ComposeSemantic<F, TS, TI, TR, TV>(s, f);

    sealed class IdSemantic<F, T, TV>
        : ISpec<F, T, TV>.ISemantic<IS<F, T>>
        where F : IKind1<F>
    {
        public IS<F, T> From(TV value)
            => new D.From<F, T, TV>(value);
    }

    sealed record class ComposeSemantic<F, TS, TI, TR, TV>(
        ISemantic1<F, TS, TI> s,
        Func<TI, TR> f
    ) : ISpec<F, TS, TV>.ISemantic<TR>
        where F : IKind1<F>
    {
        public TR From(TV value) =>
            f(ISpec<F, TS, TV>.Prj(s).From(value));
    }
}
