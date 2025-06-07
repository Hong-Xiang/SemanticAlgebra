using SemanticAlgebra.Data;
using SemanticAlgebra;
using Xunit;

namespace LambdaLang.Tests;

public class StateTTests
{
    [Fact]
    public void GetShouldWork()
    {
        var s = State<int>.Get();
        var (val, state) = s.Run(3);
        Assert.Equal(3, val);
        Assert.Equal(3, state);
    }

    [Fact]
    public void GetSelectShouldWork()
    {
        var s = from x in State<int>.Get()
                select x + x;
        var (val, state) = s.Run(3);
        Assert.Equal(6, val);
        Assert.Equal(3, state);
    }

    [Fact]
    public void PutGetShouldWork()
    {
        var s = from _ in State<int>.Put(5)
                from x in State<int>.Get()
                select x;
        var (val, state) = s.Run(3);
        Assert.Equal(5, val);
        Assert.Equal(5, state);
    }
}