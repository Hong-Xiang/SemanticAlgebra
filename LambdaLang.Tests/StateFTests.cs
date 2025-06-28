using SemanticAlgebra.Data;
using SemanticAlgebra;
using Xunit;
using SemanticAlgebra.Free;
using Xunit.Abstractions;

namespace LambdaLang.Tests;

public class StateFTests
{
    [Fact]
    public void GetShouldWork()
    {
        var s = StateF<int>.B.Get(Prelude.Id).LiftF();
        var r = s.Run(3);
        Assert.Equal(3, r.Value);
        Assert.Equal(3, r.State);
    }

    [Fact]
    public void GetSelectShouldWork()
    {
        var s = StateF<int>.B.Get(Prelude.Id).LiftF().Select(x => x + x);
        var r = s.Run(3);
        Assert.Equal(6, r.Value);
        Assert.Equal(3, r.State);
    }


    [Fact]
    public void PutGetShouldWork()
    {
        var s = from _ in StateF<int>.B.Put(5, Unit.Default).LiftF()
                from x in StateF<int>.B.Get(Prelude.Id).LiftF()
                select x;
        var (state, val) = s.Run(3);
        Assert.Equal(5, val);
        Assert.Equal(5, state);
    }
}