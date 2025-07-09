using System.Collections.Immutable;
using Xunit.Abstractions;

namespace ExprFTest;

public class ExprFTest(ITestOutputHelper Output)
{
    sealed class IdIncrementGen
    {
        private int currentId = 0;

        public int NextId() => currentId++;

    }

    [Fact]
    public void TestPrint()
    {
        var g = new IdIncrementGen();
        var f = new Factory<int>(g.NextId);

        var e = Test.TestProgram(f);

        Output.WriteLine(e.ToString());
    }

    [Fact]
    public void TestEval()
    {
        var g = new IdIncrementGen();
        var f = new Factory<int>(g.NextId);

        var e = Test.TestProgram(f);
        var r = e.Evaluate(new SimpleEvalAlgebra<int>(
            new Dictionary<string, int>
            {
                ["x"] = 42
            },
            ImmutableDictionary<int, int>.Empty
        ));

        Assert.Equal((1 + 2) + (42 + (1 + 2)), r);
    }

}
