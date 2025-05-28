using SemanticAlgebra;
using SemanticAlgebra.Data;

namespace LambdaLang;

class Val : IFunctor<Val>
{
    public static ISemantic1<Val, TS, TR> Compose<TS, TI, TR>(ISemantic1<Val, TS, TI> s, Func<TI, TR> f)
    {
        throw new NotImplementedException();
    }

    public static ISemantic1<Val, T, IS<Val, T>> Id<T>()
    {
        throw new NotImplementedException();
    }

    public static ISemantic1<Val, TS, IS<Val, TR>> MapS<TS, TR>(Func<TS, TR> f)
    {
        throw new NotImplementedException();
    }
}

static class ValExtension
{
    public static IValSemantic<TS, TR> Prj<TS, TR>(this ISemantic1<Val, TS, TR> s)
        => (IValSemantic<TS, TR>)s;
}

interface IValSemantic<TI, TO> : ISemantic1<Val, TI, TO>
{
    TO Val(TI val);
}

sealed record class Val<T>(T value) : IS<Val, T>
{
    public TR Evaluate<TR>(ISemantic1<Val, T, TR> semantic)
        => semantic.Prj().Val(value);
}

sealed class AddC : IFunctor<AddC>
{
    public static ISemantic1<AddC, TS, TR> Compose<TS, TI, TR>(ISemantic1<AddC, TS, TI> s, Func<TI, TR> f)
    {
        throw new NotImplementedException();
    }

    public static ISemantic1<AddC, T, IS<AddC, T>> Id<T>()
    {
        throw new NotImplementedException();
    }

    public static ISemantic1<AddC, TS, IS<AddC, TR>> MapS<TS, TR>(Func<TS, TR> f)
    {
        throw new NotImplementedException();
    }
}

static class AddCExtension
{
    public static IAddCSemantic<TS, TR> Prj<TS, TR>(this ISemantic1<AddC, TS, TR> s)
        => (IAddCSemantic<TS, TR>)s;
}

interface IAddCSemantic<TI, TO> : ISemantic1<AddC, TI, TO>
{
    TO Add(TI a, TI b);
}

sealed record class AddC<T>(T L, T R) : IS<AddC, T>
{
    public TR Evaluate<TR>(ISemantic1<AddC, T, TR> semantic)
        => semantic.Prj().Add(L, R);
}
