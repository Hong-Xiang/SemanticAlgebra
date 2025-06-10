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
        TR Define(Identifier name, ISigValue value);
        TR Lookup(Identifier name, Func<ISigValue, TS> value);
        TR Put(ImmutableDictionary<Identifier, ISigValue> env);
        TR Get(Func<ImmutableDictionary<Identifier, ISigValue>, TS> getEnv);
    }
    public static ISemantic1<EvalF, TS, IS<EvalF, TR>> MapS<TS, TR>(Func<TS, TR> f)
              => new MapSemantic<TS, TR>(f);

    sealed class MapSemantic<TS, TR>(Func<TS, TR> func)
        : ISemantic<TS, IS<EvalF, TR>>
    {
        public IS<EvalF, TR> Define(Identifier name, ISigValue value)
            => B.Define<TR>(name, value);

        public IS<EvalF, TR> Get(Func<ImmutableDictionary<Identifier, ISigValue>, TS> getEnv)
            => B.Get(e => func(getEnv(e)));

        public IS<EvalF, TR> Lookup(Identifier name, Func<ISigValue, TS> value)
            => B.Lookup(name, v => func(value(v)));

        public IS<EvalF, TR> Put(ImmutableDictionary<Identifier, ISigValue> env)
            => B.Put<TR>(env);
    }

    public static IS<Free<EvalF>, T> Local<T>(
        Func<ImmutableDictionary<Identifier, ISigValue>,
             ImmutableDictionary<Identifier, ISigValue>> f,
        IS<Free<EvalF>, T> e
    )
        => from s in B.Get(Prelude.Id).LiftF()
           let s_ = f(s)
           from _1 in B.Put<T>(s_).LiftF()
           from r in e
           from _2 in B.Put<T>(s).LiftF()
           select r;
}

