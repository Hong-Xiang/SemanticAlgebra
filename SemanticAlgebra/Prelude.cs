using SemanticAlgebra.Control;
using SemanticAlgebra.Data;

namespace SemanticAlgebra;

public static class Prelude
{
    public static T Id<T>(T x) => x;
    public static Unit Unit => default;

    public static Func<TA, Func<TB, TC>> Curry<TA, TB, TC>(this Func<TA, TB, TC> f) => a => b => f(a, b);

    public static IS<TF, TR> Select<TF, TS, TR>(this IS<TF, TS> fs, Func<TS, TR> f)
        where TF : IFunctor<TF>
        => fs.Evaluate(TF.Map(f).Semantic);


    public static ISemantic1<TF, TS, TR> CoMap<TF, TS, TI, TR>(this ISemantic1<TF, TI, TR> semantic, Func<TS, TI> f)
        where TF : IFunctor<TF>
        => semantic.DiMap(f, Id);


    public static IS<TF, TR> ZipWith<TF, TA, TB, TR>(this IS<TF, TA> fa, IS<TF, TB> fb, Func<TA, TB, TR> f)
        where TF : IApplicative<TF>
    {
        var a2br = TF.Apply<TA, Func<TB, TR>>().ToFunc();
        //var pf = TF.Pure<Func<TA, Func<TB, TR>>>(a => b => f(a, b));
        var pf = PureK<TF>.Pure(f.Curry());
        var fb2r = a2br(pf).ToFunc()(fa);
        var b2r = TF.Apply<TB, TR>();
        var c = fb2r.Evaluate(b2r);
        return fb.Evaluate(c.Semantic);
    }

    public static IS<TF, TR> SelectMany<TF, TS, TR>(this IS<TF, TS> fs, Func<TS, IS<TF, TR>> f)
        where TF : IMonad<TF>
        => fs.Select(f).Evaluate(TF.Join<TR>());

    public static IS<TF, TR> SelectMany<TF, TS, TM, TR>(
        this IS<TF, TS> source,
        Func<TS, IS<TF, TM>> collectionSelector,
        Func<TS, TM, TR> resultSelector)
        where TF : IMonad<TF> => source.SelectMany(s => collectionSelector(s).Select(m => resultSelector(s, m)));
}