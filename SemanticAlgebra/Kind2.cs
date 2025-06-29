using SemanticAlgebra.Control;
using SemanticAlgebra.Data;

namespace SemanticAlgebra;

public interface ISemantic2<F, TA, in TB, out TR>
    : ISemantic1<K2<F, TA>, TB, TR>
    where F : IFunctor2<F>
{
}

public interface IS2<F, TA, TB>
    : IS<K2<F, TA>, TB>
    where F : IFunctor2<F>
{
    TR Evaluate<TR>(ISemantic2<F, TA, TB, TR> semantic);

    IS2<F, TRA, TRB> Select<TRA, TRB>(Func<TA, TRA> f, Func<TB, TRB> g);
    TR IS<K2<F, TA>, TB>.Evaluate<TR>(ISemantic1<K2<F, TA>, TB, TR> semantic)
        => Evaluate(semantic.Prj());
}

public interface K2<F, TA> : IFunctor<K2<F, TA>>
    where F : IFunctor2<F>
{
    static ISemantic1<K2<F, TA>, TS, TR> IKind1<K2<F, TA>>.Compose<TS, TI, TR>(ISemantic1<K2<F, TA>, TS, TI> s, Func<TI, TR> f)
       => F.Semantic<TA, TS, TR>(e => f(e.Evaluate(s.Prj())));

    static ISemantic1<K2<F, TA>, T, IS<K2<F, TA>, T>> IKind1<K2<F, TA>>.Id<T>()
       => F.Semantic<TA, T, IS<K2<F, TA>, T>>(static e => e);

    static ISemantic1<K2<F, TA>, TS, IS<K2<F, TA>, TR>> IFunctor<K2<F, TA>>.MapS<TS, TR>(Func<TS, TR> f)
       => F.Semantic<TA, TS, IS<K2<F, TA>, TR>>(e => e.Select(f));
}

public static class K2Extension
{
    public static ISemantic2<F, TA, TB, TR> Prj<F, TA, TB, TR>(
        this ISemantic1<K2<F, TA>, TB, TR> semantic
    )
        where F : IFunctor2<F>

        => (ISemantic2<F, TA, TB, TR>)semantic;

    public static IS2<F, TA, TB> Prj<F, TA, TB>(
        this IS<K2<F, TA>, TB> e
    )
        where F : IFunctor2<F>
        => (IS2<F, TA, TB>)e;
}
