using SemanticAlgebra.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticAlgebra.Data;

public sealed partial class Pair<TL>
    : IComonad<Pair<TL>>
{
    public static ISemantic1<Pair<TL>, TS, TR> Compose<TS, TI, TR>(ISemantic1<Pair<TL>, TS, TI> s, Func<TI, TR> f)
        => IAlias<TS>.Compose(s, f);

    public static ISemantic1<Pair<TL>, TS, IS<Pair<TL>, TR>> ExtendS<TS, TR>(ISemantic1<Pair<TL>, TS, TR> f)
        => IAlias<TS>.Semantic(x => B.From(x.Item1, f.Prj().From(x)));


    public static ISemantic1<Pair<TL>, T, T> ExtractS<T>()
        => IAlias<T>.Semantic(x => x.Item2);

    public static ISemantic1<Pair<TL>, T, IS<Pair<TL>, T>> Id<T>()
        => IAlias<T>.Id();

    public static ISemantic1<Pair<TL>, TS, IS<Pair<TL>, TR>> MapS<TS, TR>(Func<TS, TR> f)
        => IAlias<TS>.Semantic(x => B.From(x.Item1, f(x.Item2)));

    public interface IAlias<TR> : Alias1.ISpec<Pair<TL>, TR, (TL, TR)> { }

    public static class B
    {
        public static IS<Pair<TL>, TR> From<TR>(TL l, TR r)
            => IAlias<TR>.From((l, r));
    }

}

public static partial class PairExtension
{
    public static Pair<TL>.IAlias<TS>.ISemantic<TR> Prj<TL, TS, TR>(this ISemantic1<Pair<TL>, TS, TR> s)
          => (Pair<TL>.IAlias<TS>.ISemantic<TR>)s;

    public static (TL, TR) Unwrap<TL, TR>(this IS<Pair<TL>, TR> e)
        => Pair<TL>.IAlias<TR>.Unwrap(e);

}
