using SemanticAlgebra.CoData;
using SemanticAlgebra.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticAlgebra;

public static class Prelude
{
    public static T Id<T>(T x) => x;
    public static Unit Unit => default;

    public static TR Eval<TF, TS, TR>(this IS<TF, TS> e, ISemantic1<TF, TS, TR> semantic)
        where TF : IKind1<TF>
        => e.Evaluate<ISemantic1<TF, TS, TR>, TR>(semantic);

    public static IS<TF, TR> Select<TF, TS, TR>(this IS<TF, TS> fs, Func<TS, TR> f)
        where TF : IFunctor<TF>
        => fs.Eval(TF.MapSemantic(f));

    public static ISemantic1<TF, TS, TR> DiMap<TF, TS, TI, TO, TR>(
        this ISemantic1<TF, TI, TO> semantic,
        Func<TS, TI> f,
        Func<TO, TR> g
    )
        where TF : IDiMapSemantic<TF>
        => TF.DiMap(semantic, f, g);

}
