using SemanticAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LambdaLang.Tests;

public class FuncFixTest
{
    static Func<int, int> FacR(Lazy<Func<int, int>> f)
        => n => n <= 0 ? 1 : n * f.Value(n - 1);
    static Func<int, int> FibR(Lazy<Func<int, int>> f)
        => n => n switch
        {
            0 => 1,
            1 => 1,
            _ => f.Value(n - 1) + f.Value(n - 2)
        };



    [Fact]
    public void FacShouldWork()
    {
        var fac = Prelude.Fix<Func<int, int>>(FacR);
        Assert.Equal(1, fac(0));
        Assert.Equal(2, fac(2));
        Assert.Equal(6, fac(3));
        Assert.Equal(24, fac(4));
    }

    [Fact]
    public void FibShouldWork()
    {
        var fac = Prelude.Fix<Func<int, int>>(FibR);
        Assert.Equal(1, fac(0));
        Assert.Equal(1, fac(1));
        Assert.Equal(2, fac(2));
        Assert.Equal(3, fac(3));
        Assert.Equal(5, fac(4));
        Assert.Equal(8, fac(5));
    }
}
