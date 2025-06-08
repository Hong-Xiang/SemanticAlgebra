using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SemanticAlgebra.Data;

namespace SemanticAlgebra;

public interface IMergedSemantic1<
    TMS,
    TS1,
    TS2,
    TS3,
    TS4,
    TS5> : IFunctor<TMS>
    where TMS : IMergedSemantic1<TMS, TS1, TS2, TS3, TS4, TS5>, TS1, TS2, TS3, TS4, TS5
    where TS1 : IFunctor<TS1>
    where TS2 : IFunctor<TS2>
    where TS3 : IFunctor<TS3>
    where TS4 : IFunctor<TS4>
    where TS5 : IFunctor<TS5>
{
    static abstract ISemantic1<TMS, TS, TR> MergeSemantic<TS, TR>(
        ISemantic1<TS1, TS, TR> s1,
        ISemantic1<TS2, TS, TR> s2,
        ISemantic1<TS3, TS, TR> s3,
        ISemantic1<TS4, TS, TR> s4,
        ISemantic1<TS5, TS, TR> s5
    );

    static ISemantic1<TMS, TS, TR> IKind1<TMS>.Compose<TS, TI, TR>(
        ISemantic1<TMS, TS, TI> s, Func<TI, TR> f)
        => TMS.MergeSemantic(
            TS1.Compose((ISemantic1<TS1, TS, TI>)s, f),
            TS2.Compose((ISemantic1<TS2, TS, TI>)s, f),
            TS3.Compose((ISemantic1<TS3, TS, TI>)s, f),
            TS4.Compose((ISemantic1<TS4, TS, TI>)s, f),
            TS5.Compose((ISemantic1<TS5, TS, TI>)s, f));

    static ISemantic1<TMS, T, IS<TMS, T>> IKind1<TMS>.Id<T>()
        => TMS.MergeSemantic<T, IS<TMS, T>>(
            TS1.Id<T>().Compose(e => (IS<TMS, T>)e),
            TS2.Id<T>().Compose(e => (IS<TMS, T>)e),
            TS3.Id<T>().Compose(e => (IS<TMS, T>)e),
            TS4.Id<T>().Compose(e => (IS<TMS, T>)e),
            TS5.Id<T>().Compose(e => (IS<TMS, T>)e)
        );

    static ISemantic1<TMS, TS, IS<TMS, TR>> IFunctor<TMS>.MapS<TS, TR>(Func<TS, TR> f)
        => TMS.MergeSemantic<TS, IS<TMS, TR>>(
            TS1.MapS(f).Compose(e => (IS<TMS, TR>)e),
            TS2.MapS(f).Compose(e => (IS<TMS, TR>)e),
            TS3.MapS(f).Compose(e => (IS<TMS, TR>)e),
            TS4.MapS(f).Compose(e => (IS<TMS, TR>)e),
            TS5.MapS(f).Compose(e => (IS<TMS, TR>)e));
}

public interface IMergedSemantic1WithAlgebra<
    TMS,
    TAlg,
    T,
    TS1,
    TS2,
    TS3,
    TS4,
    TS5>
    : IMergedSemantic1<TMS, TS1, TS2, TS3, TS4, TS5>
    , IWithAlgebra<TMS, TAlg, T>
    where TMS : IMergedSemantic1WithAlgebra<TMS, TAlg, T, TS1, TS2, TS3, TS4, TS5>, TS1, TS2, TS3, TS4, TS5
    where TAlg : IAlgebra<TAlg, T>
    where TS1 : IWithAlgebra<TS1, TAlg, T>
    where TS2 : IWithAlgebra<TS2, TAlg, T>
    where TS3 : IWithAlgebra<TS3, TAlg, T>
    where TS4 : IWithAlgebra<TS4, TAlg, T>
    where TS5 : IWithAlgebra<TS5, TAlg, T>
{
    static ISemantic1<TMS, T, T> IWithAlgebra<TMS, TAlg, T>.Get()
        => TMS.MergeSemantic(
            TS1.Get(),
            TS2.Get(),
            TS3.Get(),
            TS4.Get(),
            TS5.Get()
        );
}

public interface IMergedSemantic1<
    TMS,
    TS1,
    TS2,
    TS3,
    TS4,
    TS5,
    TS6> : IFunctor<TMS>
    where TMS : IMergedSemantic1<TMS, TS1, TS2, TS3, TS4, TS5, TS6>, TS1, TS2, TS3, TS4, TS5, TS6
    where TS1 : IFunctor<TS1>
    where TS2 : IFunctor<TS2>
    where TS3 : IFunctor<TS3>
    where TS4 : IFunctor<TS4>
    where TS5 : IFunctor<TS5>
    where TS6 : IFunctor<TS6>
{
    static abstract ISemantic1<TMS, TS, TR> MergeSemantic<TS, TR>(
        ISemantic1<TS1, TS, TR> s1,
        ISemantic1<TS2, TS, TR> s2,
        ISemantic1<TS3, TS, TR> s3,
        ISemantic1<TS4, TS, TR> s4,
        ISemantic1<TS5, TS, TR> s5,
        ISemantic1<TS6, TS, TR> s6
    );

    static ISemantic1<TMS, TS, TR> IKind1<TMS>.Compose<TS, TI, TR>(
        ISemantic1<TMS, TS, TI> s, Func<TI, TR> f)
        => TMS.MergeSemantic(
            TS1.Compose((ISemantic1<TS1, TS, TI>)s, f),
            TS2.Compose((ISemantic1<TS2, TS, TI>)s, f),
            TS3.Compose((ISemantic1<TS3, TS, TI>)s, f),
            TS4.Compose((ISemantic1<TS4, TS, TI>)s, f),
            TS5.Compose((ISemantic1<TS5, TS, TI>)s, f),
            TS6.Compose((ISemantic1<TS6, TS, TI>)s, f));

    static ISemantic1<TMS, T, IS<TMS, T>> IKind1<TMS>.Id<T>()
        => TMS.MergeSemantic<T, IS<TMS, T>>(
            TS1.Id<T>().Compose(e => (IS<TMS, T>)e),
            TS2.Id<T>().Compose(e => (IS<TMS, T>)e),
            TS3.Id<T>().Compose(e => (IS<TMS, T>)e),
            TS4.Id<T>().Compose(e => (IS<TMS, T>)e),
            TS5.Id<T>().Compose(e => (IS<TMS, T>)e),
            TS6.Id<T>().Compose(e => (IS<TMS, T>)e)
        );

    static ISemantic1<TMS, TS, IS<TMS, TR>> IFunctor<TMS>.MapS<TS, TR>(Func<TS, TR> f)
        => TMS.MergeSemantic<TS, IS<TMS, TR>>(
            TS1.MapS(f).Compose(e => (IS<TMS, TR>)e),
            TS2.MapS(f).Compose(e => (IS<TMS, TR>)e),
            TS3.MapS(f).Compose(e => (IS<TMS, TR>)e),
            TS4.MapS(f).Compose(e => (IS<TMS, TR>)e),
            TS5.MapS(f).Compose(e => (IS<TMS, TR>)e),
            TS6.MapS(f).Compose(e => (IS<TMS, TR>)e)
            );

}

public interface IMergedSemantic1WithAlgebra<
    TMS,
    TAlg,
    T,
    TS1,
    TS2,
    TS3,
    TS4,
    TS5,
    TS6>
    : IMergedSemantic1<TMS, TS1, TS2, TS3, TS4, TS5, TS6>
    , IWithAlgebra<TMS, TAlg, T>
    where TMS : IMergedSemantic1WithAlgebra<TMS, TAlg, T, TS1, TS2, TS3, TS4, TS5, TS6>, TS1, TS2, TS3, TS4, TS5, TS6
    where TAlg : IAlgebra<TAlg, T>
    where TS1 : IWithAlgebra<TS1, TAlg, T>
    where TS2 : IWithAlgebra<TS2, TAlg, T>
    where TS3 : IWithAlgebra<TS3, TAlg, T>
    where TS4 : IWithAlgebra<TS4, TAlg, T>
    where TS5 : IWithAlgebra<TS5, TAlg, T>
    where TS6 : IWithAlgebra<TS6, TAlg, T>
{
    static ISemantic1<TMS, T, T> IWithAlgebra<TMS, TAlg, T>.Get()
        => TMS.MergeSemantic(
            TS1.Get(),
            TS2.Get(),
            TS3.Get(),
            TS4.Get(),
            TS5.Get(),
            TS6.Get()
        );
}