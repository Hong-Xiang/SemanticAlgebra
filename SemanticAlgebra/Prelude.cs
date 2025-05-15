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

    public static ISemantic1<TF, TS, TR> DiMap<TF, TS, TI, TO, TR>(this ISemantic1<TF, TI, TO> semantic, Func<TS, TI> f, Func<TO, TR> g)
        where TF : IFunctor<TF>
        => TF.SemanticDiMap(semantic, f, g);


    public static IDiSemantic<TF, TS, TR> DiSemantic<TF, TS, TR>(Func<IS<TF, TS>, IS<TF, TR>> f)
        where TF : IKind1<TF>
        => new FuncDiSemantic<TF, TS, TR>(f);

    public static IS<TF, TR> ZipWith<TF, TA, TB, TR>(this IS<TF, TA> fa, IS<TF, TB> fb, Func<TA, TB, TR> f)
        where TF : IApplicative<TF>
    {
        var a2br = TF.ToFunc(TF.Apply<TA, Func<TB, TR>>());
        //var pf = TF.Pure<Func<TA, Func<TB, TR>>>(a => b => f(a, b));
        var pf = PureK<TF>.Pure(f.Curry());
        var fb2r = TF.ToFunc(a2br(pf))(fa);
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


    public static IDiSemantic<TF, TS, TR> AsDiSemantic<TF, TS, TR>(this ISemantic1<TF, TS, IS<TF, TR>> semantic)
        where TF : IKind1<TF>
        => new DiSemanticFromSemantic<TF, TS, TR>(semantic);

    public static ISemantic1<TF, TS, TR> MapR<TF, TS, TI, TR>(this ISemantic1<TF, TS, TI> semantic, Func<TI, TR> f)
        where TF : IFunctor<TF>
        => TF.SemanticDiMap<TS, TS, TI, TR>(semantic, Id, f);
}
