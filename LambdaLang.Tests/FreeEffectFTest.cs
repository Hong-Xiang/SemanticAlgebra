using SemanticAlgebra;
using SemanticAlgebra.Data;
using SemanticAlgebra.Free;
using SemanticAlgebra.Syntax;
using Xunit;
using Xunit.Abstractions;

namespace LambdaLang.Tests;

public abstract partial class EffCountF : IFunctor<EffCountF>
{
    [Semantic1]
    public interface ISemantic<in TS, out TR>
        : ISemantic1<EffCountF, TS, TR>
    {
        TR Get(Func<int, TS> next);
        TR Add(TS next);
    }

    public static ISemantic1<EffCountF, TS, IS<EffCountF, TR>> MapS<TS, TR>(Func<TS, TR> f)
        => new MapSemantic<TS, TR>(f);

    public sealed class MapSemantic<TS, TR>(
        Func<TS, TR> Func
    ) : ISemantic<TS, IS<EffCountF, TR>>
    {
        IS<EffCountF, TR> ISemantic<TS, IS<EffCountF, TR>>.Add(TS next)
            => B.Add(Func(next));

        IS<EffCountF, TR> ISemantic<TS, IS<EffCountF, TR>>.Get(Func<int, TS> next)
            => B.Get(v => Func(next(v)));
    }
}

sealed class EffCountStateFNaturalTransform
    : INaturalTransform<EffCountF, Free<StateF<int>>>
{
    sealed class TransformSemantic<T> : EffCountF.ISemantic<T, IS<Free<StateF<int>>, T>>
    {
        public IS<Free<StateF<int>>, T> Add(T next)
            => from _ in StateF<int>.Modify(n => n + 1)
               select next;

        public IS<Free<StateF<int>>, T> Get(Func<int, T> next)
            => from s in StateF<int>.Get()
               select next(s);
    }

    public IS<Free<StateF<int>>, T> Invoke<T>(IS<EffCountF, T> e)
        => e.Evaluate(new TransformSemantic<T>());
}

public class FreeEffectFTest(ITestOutputHelper Output)
{
    sealed class StatefulSemantic
        : EffCountF.ISemantic<Func<int>, Func<int>>
    {
        private int state = 0;

        public Func<int> Add(Func<int> next)
        {
            state++;
            return next;
        }

        public Func<int> Get(Func<int, Func<int>> next)
            => () => next(state)();
    }

    [Fact]
    public void SimpleCountZeroShouldWork()
    {
        var z = EffCountF.B.Get<Func<int>>(x => () => x).LiftF();
        Assert.Equal(0, z.Evaluate(new StatefulSemantic().LiftF())());
    }

    [Fact]
    public void SimpleCountOneShouldWork()
    {
        var s = from _1 in EffCountF.B.Add(Unit.Default).LiftF()
                from v in EffCountF.B.Get(Prelude.Id).LiftF()
                select v;
        Output.WriteLine(s.ToString());
        Assert.Equal(1, s.Select<Free<EffCountF>, int, Func<int>>(x => () => x).Evaluate(new StatefulSemantic().LiftF())());
    }

    [Fact]
    public void SimpleCountZeroShouldWorkUsingFreeInterperter()
    {
        var z = EffCountF.B.Get(Prelude.Id).LiftF();
        var r = z.Interp(new EffCountStateFNaturalTransform()).Run(0);
        Assert.Equal(0, r.Value);
    }

    [Fact]
    public void SimpleCountOneShouldWorkUsingFreeInterpreter()
    {
        var s = from _1 in EffCountF.B.Add(Unit.Default).LiftF()
                from v in EffCountF.B.Get(Prelude.Id).LiftF()
                select v;
        Output.WriteLine(s.ToString());
        var r = s.Interp(new EffCountStateFNaturalTransform()).Run(0);
        Assert.Equal(1, r.Value);
    }

}
