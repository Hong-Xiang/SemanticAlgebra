using SemanticAlgebra.Data;
using SemanticAlgebra;
using Xunit;
using SemanticAlgebra.Free;

namespace LambdaLang.Tests;

public class StateFTests
{
    [Fact]
    public void GetShouldWork()
    {
        var s = StateF<int>.B.Get(Prelude.Id).LiftF();
        var (val, state) = s.Run(3);
        Assert.Equal(3, val);
        Assert.Equal(3, state);
    }

    [Fact]
    public void GetSelectShouldWork()
    {
        var s = StateF<int>.B.Get(Prelude.Id).LiftF().Select(x => x + x);
        var (val, state) = s.Run(3);
        Assert.Equal(6, val);
        Assert.Equal(3, state);
    }

    [Fact]
    public void PutGetShouldWork()
    {
        var s = from _ in StateF<int>.B.Put(5, Unit.Default).LiftF()
                from x in StateF<int>.B.Get(Prelude.Id).LiftF()
                select x;
        var (val, state) = s.Run(3);
        Assert.Equal(5, val);
        Assert.Equal(5, state);
    }
}