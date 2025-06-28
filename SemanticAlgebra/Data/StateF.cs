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

    public static IS<Free<StateF<S>>, S> Get()
        => B.Get(Prelude.Id).LiftF();
    public static IS<Free<StateF<S>>, Unit> Put(S state)
        => B.Put(state, Unit.Default).LiftF();

    public static IS<Free<StateF<S>>, Unit> Modify(Func<S, S> update)
        => from s in Get()
           let s_ = update(s)
           from _ in Put(s_)
           select _;
}

public static partial class StateFExtension
{
    sealed class StateFRunFreeSemantic<S, T>
        : StateF<S>.ISemantic<Func<S, (S, T)>, Func<S, (S, T)>>
    {
        public Func<S, (S, T)> Get(Func<S, Func<S, (S, T)>> next)
            => s => next(s)(s);

        public Func<S, (S, T)> Put(S state, Func<S, (S, T)> next)
            => _ => next(state);
    }

    private static Func<S, (S, T)> Pure<S, T>(T t)
        => s => (s, t);

    public static IS<Free<StateF<S>>, T> LocalWith<S, T>(this
        IS<Free<StateF<S>>, T> e, Func<S, S> f)
        => from s in StateF<S>.Get()
           let s_ = f(s)
           from _1 in StateF<S>.Put(s_)
           from v in e
           from _2 in StateF<S>.Put(s)
           select v;

    public static (S State, T Value) Run<S, T>(this IS<Free<StateF<S>>, T> e, S state)
        => e.Select(Pure<S, T>).Evaluate(new StateFRunFreeSemantic<S, T>().LiftF())(state);
}
