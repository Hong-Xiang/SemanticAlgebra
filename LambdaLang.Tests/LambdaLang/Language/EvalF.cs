using SemanticAlgebra;
using SemanticAlgebra.Data;
using SemanticAlgebra.Free;
using SemanticAlgebra.Syntax;
using System.Collections.Immutable;

namespace LambdaLang.Tests.LambdaLang.Language;

// free-monad style interpreter


public abstract partial class EvalF : IFunctor<EvalF>
{
    [Semantic1]
    public interface ISemantic<in TS, out TR>
        : ISemantic1<EvalF, TS, TR>
    {
        TR Define(Identifier name, ISigValue value, TS next);
        TR Lookup(Identifier name, Func<ISigValue, TS> value);
        TR Put(ImmutableDictionary<Identifier, ISigValue> env, TS next);
        TR Get(Func<ImmutableDictionary<Identifier, ISigValue>, TS> getEnv);
    }
    public static ISemantic1<EvalF, TS, IS<EvalF, TR>> MapS<TS, TR>(Func<TS, TR> f)
              => new MapSemantic<TS, TR>(f);

    sealed class MapSemantic<TS, TR>(Func<TS, TR> func)
        : ISemantic<TS, IS<EvalF, TR>>
    {
        public IS<EvalF, TR> Define(Identifier name, ISigValue value, TS next)
            => B.Define<TR>(name, value, func(next));

        public IS<EvalF, TR> Get(Func<ImmutableDictionary<Identifier, ISigValue>, TS> getEnv)
            => B.Get(e => func(getEnv(e)));

        public IS<EvalF, TR> Lookup(Identifier name, Func<ISigValue, TS> value)
            => B.Lookup(name, v => func(value(v)));

        public IS<EvalF, TR> Put(ImmutableDictionary<Identifier, ISigValue> env, TS next)
            => B.Put<TR>(env, func(next));
    }

    public static IS<Free<EvalF>, T> Local<T>(
        Func<ImmutableDictionary<Identifier, ISigValue>,
             ImmutableDictionary<Identifier, ISigValue>> f,
        IS<Free<EvalF>, T> e
    )
        => from s in B.Get(Prelude.Id).LiftF()
           let s_ = f(s)
           from _1 in B.Put(s_, Unit.Default).LiftF()
           from r in e
           from _2 in B.Put(s, Unit.Default).LiftF()
           select r;
}


public sealed class EvalFToStateTNaturalTransform : INaturalTransform<EvalF, StateT<Identity, ImmutableDictionary<Identifier, ISigValue>>>
{
    sealed class Semantic<M, TS> : EvalF.ISemantic<TS, IS<M, TS>>
        where M : IMonadState<M, ImmutableDictionary<Identifier, ISigValue>>
    {
        public IS<M, TS> Define(Identifier name, ISigValue value, TS next)
            => from s in M.Get()
               from _ in M.Put(s.Add(name, value))
               select next;

        public IS<M, TS> Get(Func<ImmutableDictionary<Identifier, ISigValue>, TS> getEnv)
            => from s in M.Get()
               select getEnv(s);

        public IS<M, TS> Lookup(Identifier name, Func<ISigValue, TS> value)
            => from s in M.Get()
               select value(s[name]);        public IS<M, TS> Put(ImmutableDictionary<Identifier, ISigValue> env, TS next)
            => from _ in M.Put(env)
               select next;
    }

    public IS<StateT<Identity, ImmutableDictionary<Identifier, ISigValue>>, T> Invoke<T>(IS<EvalF, T> e)
        => e.Evaluate(new Semantic<StateT<Identity, ImmutableDictionary<Identifier, ISigValue>>, T>());
}
