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
    public void TestPrintS()
    {
        var g = new IdIncrementGen();
        var f = new FactoryS<string, int>(g.NextId);

        var e = Test.TestProgram(f);

        Output.WriteLine(e.ToString());

        var code = e.Fold(new ShowAlgebra());
        Output.WriteLine(string.Join(";\n", code));
    }

    [Fact]
    public void TestEvalS()
    {
        var g = new IdIncrementGen();
        var f = new FactoryS<string, int>(g.NextId);

        var e = Test.TestProgram(f);

        Output.WriteLine(e.ToString());

        var code = e.Fold(new ShowAlgebra());
        Output.WriteLine(string.Join(";\n", code));

        var eval = e.Fold(new EvalAlgebra<int>());
        var env = new Env<int>(new Dictionary<Variable, int>()
        {
            [new Variable("x")] = 42
        }.ToImmutableDictionary(), ImmutableDictionary<int, int>.Empty);
        var result = eval(env);
        Output.WriteLine(result.ToString());
        Assert.Equal(new Ret<string, int>(((1 + 2) + (42 + (1 + 2)))), result);
    }
}