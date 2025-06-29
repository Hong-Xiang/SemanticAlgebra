using SemanticAlgebra.Data;

namespace SemanticAlgebra;

// encoding f a b, where f is profunctor
public interface IFunctor11<TF>
    where TF : IFunctor11<TF>
{
    // semantic is encoding of f a b -> r
    static abstract ISemantic11<TF, TA, TB, TR> Semantic<TA, TB, TR>(Func<IS11<TF, TA, TB>, TR> f);
}

// f a b
public interface ISemantic11<F, TA, in TB, out TR>
    : ISemantic1<K111<F, TA>, TB, TR>
    where F : IFunctor11<F>
{
}

public interface IS11<F, TA, TB>
    : IS<K111<F, TA>, TB>
    where F : IFunctor11<F>
{
    TR Evaluate<TR>(ISemantic11<F, TA, TB, TR> semantic);
    IS11<F, TSA, TRB> Select<TSA, TRB>(Func<TSA, TA> f, Func<TB, TRB> g);
}

public abstract class K111<F, TA> : IFunctor<K111<F, TA>>
    where F : IFunctor11<F>
{
    public static ISemantic1<K111<F, TA>, TS, TR> Compose<TS, TI, TR>(ISemantic1<K111<F, TA>, TS, TI> s, Func<TI, TR> f)
    {
        throw new NotImplementedException();
    }

    public static ISemantic1<K111<F, TA>, T, IS<K111<F, TA>, T>> Id<T>()
        => F.Semantic<TA, T, IS<K111<F, TA>, T>>(static x => x);

    public static ISemantic1<K111<F, TA>, TS, IS<K111<F, TA>, TR>> MapS<TS, TR>(Func<TS, TR> f)
        => F.Semantic<TA, TS, IS<K111<F, TA>, TR>>(x => x.Select<TA, TR>(Prelude.Id, f));
}

