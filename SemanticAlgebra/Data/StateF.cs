using SemanticAlgebra.Fix;
using SemanticAlgebra.Free;
using SemanticAlgebra.Syntax;

namespace SemanticAlgebra.Data;

public abstract partial class StateF<S> : IFunctor<StateF<S>>
{
    [Semantic1]
    public interface ISemantic<in TS, out TR>
        : ISemantic1<StateF<S>, TS, TR>
    {
        TR Get(Func<S, TS> next);
        TR Put(S state, TS next);
    }

    public static ISemantic1<StateF<S>, TS, IS<StateF<S>, TR>> MapS<TS, TR>(Func<TS, TR> f)
        => new MapSemantic<TS, TR>(f);

    public sealed class MapSemantic<TS, TR>(
        Func<TS, TR> Func
    ) : ISemantic<TS, IS<StateF<S>, TR>>
    {
        public IS<StateF<S>, TR> Get(Func<S, TS> next)
            => B.Get(s => Func(next(s)));

        public IS<StateF<S>, TR> Put(S state, TS next)
            => B.Put(state, Func(next));
    }
}

public static partial class StateFExtension
{
    sealed class StateFRunFreeSemantic<S, T>
        : Free<StateF<S>>.ISemantic<T, Func<S, (S, T)>>
        , StateF<S>.ISemantic<IS<Free<StateF<S>>, T>, Func<S, (S, T)>>

    {
        public Func<S, (S, T)> Pure(T v)
            => s => (s, v);

        public Func<S, (S, T)> Roll(IS<StateF<S>, IS<Free<StateF<S>>, T>> v)
            => v.Evaluate<Func<S, (S, T)>>(this);

        public Func<S, (S, T)> Get(Func<S, IS<Free<StateF<S>>, T>> next)
            => s => next(s).Run(s);

        public Func<S, (S, T)> Put(S state, IS<Free<StateF<S>>, T> next)
            => _ => next.Run(state);
    }
    public static (S State, T Value) Run<S, T>(this IS<Free<StateF<S>>, T> e, S state)
        => e.Evaluate<Func<S, (S, T)>>(new StateFRunFreeSemantic<S, T>())(state);

    sealed class StateFRunFreeFSemantic<S, T>
        //: FreeF<StateF<S>, T>.ISemantic<Func<S, (S, T)>, Func<S, (S, T)>>
        : StateF<S>.ISemantic<Func<S, (S, T)>, Func<S, (S, T)>>

    {
        public Func<S, (S, T)> Nest(IS<StateF<S>, Func<S, (S, T)>> nest)
            => nest.Evaluate<Func<S, (S, T)>>(this);

        public Func<S, (S, T)> Pure(T value)
            => s => (s, value);

        public Func<S, (S, T)> Put(S state, Func<S, (S, T)> next)
            => _ => next(state);

        public Func<S, (S, T)> Get(Func<S, Func<S, (S, T)>> next)
            => s => next(s)(s);
    }

    public static (S State, T Value) Run<S, T>(this Fix<FreeF<StateF<S>, T>> e, S state)
        => e.Fold(
            new FreeF<StateF<S>, T>.SemanticF<Func<S, (S, T)>>(
                new StateFRunFreeFSemantic<S, T>(),
                t => s => (s, t)
            ))(state);
}
