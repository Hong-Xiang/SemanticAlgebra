using SemanticAlgebra.Control;
using SemanticAlgebra.Syntax;

namespace SemanticAlgebra.Data;

public sealed partial class State<S> : IMonad<State<S>>
{
    // public static ISemantic1<State<S>, TS, TR> Compose<TS, TI, TR>(ISemantic1<State<S>, TS, TI> s, Func<TI, TR> f)
    //     => new ComposeSemantic<TS, TI, TR>(s.Prj(), f);
    //
    //
    // public static ISemantic1<State<S>, T, IS<State<S>, T>> Id<T>()
    //     => new IdSemantic<T>();

    public static ISemantic1<State<S>, TS, IS<State<S>, TR>> MapS<TS, TR>(Func<TS, TR> f)
        => new MapSemantic<TS, TR>(f);

    public interface IStateResult<out T>
    {
        S State { get; }
        T Data { get; }
    }

    [Semantic1]
    public interface ISemantic<in TS, out TR>
        : ISemantic1<State<S>, TS, TR>
    {
        TR RunState(Func<S, IStateResult<TS>> fn);
    }

    public sealed record class StateResult<T>(S State, T Data) : IStateResult<T>
    {
        public static implicit operator StateResult<T>((S, T) t) => new(t.Item1, t.Item2);
        public static implicit operator (S, T)(StateResult<T> r) => (r.State, r.Data);
    }


    public sealed record class Data<T>(Func<S, StateResult<T>> F)
        : IS<State<S>, T>
    {
        public TR Evaluate<TR>(ISemantic1<State<S>, T, TR> semantic)
            => semantic.Prj().RunState(F);
    }


    // sealed class IdSemantic<T> : ISemantic<T, IS<State<S>, T>>
    // {
    //     public IS<State<S>, T> RunState(Func<S, IStateResult<T>> f)
    //         => From(s =>
    //         {
    //             var r = f(s);
    //             return (r.State, r.Data);
    //         });
    // }
    //
    //
    // sealed class ComposeSemantic<TS, TI, TR>(ISemantic<TS, TI> S, Func<TI, TR> F) : ISemantic<TS, TR>
    // {
    //     public TR RunState(Func<S, IStateResult<TS>> f) => F(S.RunState(f));
    // }

    public sealed class MapSemantic<TS, TR>(Func<TS, TR> F) : ISemantic<TS, IS<State<S>, TR>>
    {
        public IS<State<S>, TR> RunState(Func<S, IStateResult<TS>> f)
            => From(s =>
            {
                var r = f(s);
                return (r.State, F(r.Data));
            });
    }

    public static IS<State<S>, T> From<T>(Func<S, (S, T)> f)
        => new Data<T>(s => f(s));


    sealed class RunSemantic<T>(S s) : ISemantic<T, IStateResult<T>>
    {
        public IStateResult<T> RunState(Func<S, IStateResult<T>> f)
            => f(s);
    }

    public static Func<S, IStateResult<T>> Unwrap<T>(IS<State<S>, T> state)
        => s => state.Evaluate(new RunSemantic<T>(s));

    public static ISemantic1<State<S>, Func<TS, TR>, ISemantic1<State<S>, TS, IS<State<S>, TR>>> ApplyS<TS, TR>()
        => new ApplySemantic<TS, TR>();


    sealed class ApplySemantic<TS, TR>() : ISemantic<Func<TS, TR>, ISemantic<TS, IS<State<S>, TR>>>
    {
        sealed class ResultSemantic(Func<S, IStateResult<Func<TS, TR>>> F) : ISemantic<TS, IS<State<S>, TR>>
        {
            public IS<State<S>, TR> RunState(Func<S, IStateResult<TS>> f)
                => From(s =>
                {
                    var funcR = F(s);
                    var valueR = f(funcR.State);
                    return (valueR.State, funcR.Data(valueR.Data));
                });
        }

        public ISemantic<TS, IS<State<S>, TR>> RunState(Func<S, IStateResult<Func<TS, TR>>> f)
            => new ResultSemantic(f);
    }

    public static IS<State<S>, T> Pure<T>(T x)
        => From(s => (s, x));

    public static ISemantic1<State<S>, IS<State<S>, T>, IS<State<S>, T>> JoinS<T>()
        => new JoinSemantic<T>();

    sealed class JoinSemantic<T>() : ISemantic<IS<State<S>, T>, IS<State<S>, T>>
    {
        public IS<State<S>, T> RunState(Func<S, IStateResult<IS<State<S>, T>>> f)
            => From(s =>
            {
                var r = f(s);
                var r_ = r.Data.Run(r.State);
                return (r_.State, r_.Data);
            });
    }
}

public static class State
{
    // public static State<S>.ISemantic<TS, TR> Prj<S, TS, TR>(this ISemantic1<State<S>, TS, TR> s)
    //     => (State<S>.ISemantic<TS, TR>)s;

    public static State<S>.IStateResult<T> Run<S, T>(this IS<State<S>, T> state, S s)
        => State<S>.Unwrap(state)(s);
}

// s -> (s, a)
// s -> (r, a)




interface ISem2<TI1, TO1, TI2, TO2>
{
}

// indexed state 
// operations lookup :: state<k, t> -> key -> t
//            update :: state<k, t> -> key -> t -> state<k, t>
interface IIndexedState<F, TKey>
{
}