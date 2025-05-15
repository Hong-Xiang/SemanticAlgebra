using SemanticAlgebra.Data;

namespace SemanticAlgebra;

public static class Prelude
{
    public static T Id<T>(T x) => x;
    public static Unit Unit => default;

    public static Func<TA, Func<TB, TC>> Curry<TA, TB, TC>(this Func<TA, TB, TC> f) => a => b => f(a, b);

    public static TR Eval<TF, TS, TR>(this IS<TF, TS> e, ISemantic1<TF, TS, TR> semantic)
        where TF : IKind1<TF>
        => e.Evaluate<ISemantic1<TF, TS, TR>, TR>(semantic);

    public static IS<TF, TR> Select<TF, TS, TR>(this IS<TF, TS> fs, Func<TS, TR> f)
        where TF : IFunctor<TF>
        => fs.Eval(TF.MapSemantic(f).Semantic);

    public static ISemantic1<TF, T, IS<TF, T>> IdSemantic<TF, T>()
        where TF : IKind1<TF>
        => TF.IdSemantic<T>();
    public static IDiSemantic<TF, TS, TR> DiSemantic<TF, TS, TR>(Func<IS<TF, TS>, IS<TF, TR>> f)
        where TF : IKind1<TF>
        => new FuncDiSemantic<TF, TS, TR>(f);

    public static IS<TF, TR> ZipWith<TF, TA, TB, TR>(this IS<TF, TA> fa, IS<TF, TB> fb, Func<TA, TB, TR> f)
        where TF : IApplicative<TF>
    {
        var a2br = TF.ToFunc(TF.ApplySemantic<TA, Func<TB, TR>>());
        //var pf = TF.Pure<Func<TA, Func<TB, TR>>>(a => b => f(a, b));
        var pf = TF.Pure(f.Curry());
        var fb2r = TF.ToFunc(a2br(pf))(fa);
        var b2r = TF.ApplySemantic<TB, TR>();
        var c = fb2r.Eval(b2r);
        return fb.Eval(c.Semantic);
    }

    public static IS<TF, TR> SelectMany<TF, TS, TR>(this IS<TF, TS> fs, Func<TS, IS<TF, TR>> f)
        where TF : IMonad<TF>
        => fs.Select(f).Eval(TF.JoinSemantic<TR>());
    public static IS<TF, TR> SelectMany<TF, TS, TM, TR>(
               this IS<TF, TS> source,
               Func<TS, IS<TF, TM>> collectionSelector,
               Func<TS, TM, TR> resultSelector)
        where TF : IMonad<TF> => source.SelectMany(s => collectionSelector(s).Select(m => resultSelector(s, m)));
}
