using SemanticAlgebra;
using SemanticAlgebra.Data;
using SemanticAlgebra.Fix;
using SemanticAlgebra.Free;
using System.Collections.Immutable;

namespace LambdaLang.Tests.LambdaLang.Language;

public static class EvalAlgebra
{
    public ref struct FolderWrapper<F>(Fix<F> e)
            where F : IFunctor<F>, IEvalAlgebra<F>
    {
        public IS<M, ISigValue> Fold<M>()
               where M : IMonadState<M, ImmutableDictionary<Identifier, ISigValue>>
               => e.Fold(F.Get<M>());

    }
    public static FolderWrapper<F> MonadicFolder<F>(this Fix<F> e)
           where F : IFunctor<F>, IEvalAlgebra<F>
        => new FolderWrapper<F>(e);

}

public interface IEvalAlgebra<F> : IFunctor<F>
    where F : IEvalAlgebra<F>
{
    static abstract ISemantic1<F, IS<M, ISigValue>, IS<M, ISigValue>> Get<M>()
        where M : IMonadState<M, ImmutableDictionary<Identifier, ISigValue>>;

    static abstract ISemantic1<F, IS<Free<EvalF>, ISigValue>, IS<Free<EvalF>, ISigValue>> GetFree();
}

static class EvalAlgebraK<F>
    where F : IEvalAlgebra<F>
{
    public static ISemantic1<F, IS<Free<EvalF>, ISigValue>, IS<Free<EvalF>, ISigValue>> GetFree()
            => F.GetFree();
}

public interface IMergedSemantic1EvalAlgebra<
    TMS,
    TS1,
    TS2,
    TS3,
    TS4,
    TS5>
    : IMergedSemantic1<TMS, TS1, TS2, TS3, TS4, TS5>
    , IEvalAlgebra<TMS>
    where TMS : IMergedSemantic1EvalAlgebra<TMS, TS1, TS2, TS3, TS4, TS5>, TS1, TS2, TS3, TS4, TS5
    where TS1 : IEvalAlgebra<TS1>
    where TS2 : IEvalAlgebra<TS2>
    where TS3 : IEvalAlgebra<TS3>
    where TS4 : IEvalAlgebra<TS4>
    where TS5 : IEvalAlgebra<TS5>
{

    static ISemantic1<TMS, IS<M, ISigValue>, IS<M, ISigValue>> IEvalAlgebra<TMS>.Get<M>()
        => TMS.MergeSemantic(
            TS1.Get<M>(),
            TS2.Get<M>(),
            TS3.Get<M>(),
            TS4.Get<M>(),
            TS5.Get<M>()
        );
}


public interface IMergedSemantic1EvalAlgebra<
    TMS,
    TS1,
    TS2,
    TS3,
    TS4,
    TS5,
    TS6>
    : IMergedSemantic1<TMS, TS1, TS2, TS3, TS4, TS5, TS6>
    , IEvalAlgebra<TMS>
    where TMS : IMergedSemantic1EvalAlgebra<TMS, TS1, TS2, TS3, TS4, TS5, TS6>, TS1, TS2, TS3, TS4, TS5, TS6
    where TS1 : IEvalAlgebra<TS1>
    where TS2 : IEvalAlgebra<TS2>
    where TS3 : IEvalAlgebra<TS3>
    where TS4 : IEvalAlgebra<TS4>
    where TS5 : IEvalAlgebra<TS5>
    where TS6 : IEvalAlgebra<TS6>
{

    static ISemantic1<TMS, IS<M, ISigValue>, IS<M, ISigValue>> IEvalAlgebra<TMS>.Get<M>()
        => TMS.MergeSemantic(
            TS1.Get<M>(),
            TS2.Get<M>(),
            TS3.Get<M>(),
            TS4.Get<M>(),
            TS5.Get<M>(),
            TS6.Get<M>()
        );

    static ISemantic1<TMS, IS<Free<EvalF>, ISigValue>, IS<Free<EvalF>, ISigValue>> IEvalAlgebra<TMS>.GetFree()
           => TMS.MergeSemantic(
               TS1.GetFree(),
               TS2.GetFree(),
               TS3.GetFree(),
               TS4.GetFree(),
               TS5.GetFree(),
               TS6.GetFree()
               );

}
