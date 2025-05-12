using SemanticAlgebra.CoData;

namespace SemanticAlgebra.Option;

public sealed class Option
    : IDiMapSemantic<Option>
{

    public static ISemantic1<Option, TS, TR> DiMap<TS, TI, TO, TR>(ISemantic1<Option, TI, TO> semantic, Func<TS, TI> f, Func<TO, TR> g)
        => new OptionDiMapSemantic<TS, TI, TO, TR>(semantic, f, g);
    public static ISemantic1<Option, T, IS<Option, T>> IdSemantic<T>()
        => new OptionIdSemantic<T>();


    public static IS<Option, T> None<T>() => IdSemantic<T>().Prj().None();
    public static IS<Option, T> Some<T>(T value) => IdSemantic<T>().Prj().Some(value);
}

public static class OptionExtension
{
    public static IOptionSemantic<TS, TR> Prj<TS, TR>(
        this ISemantic1<Option, TS, TR> semantic
    ) => (IOptionSemantic<TS, TR>)semantic;
}

public interface IOptionSemantic<in TS, out TR>
    : ISemantic1<Option, TS, TR>
{
    TR None();
    TR Some(TS value);
}

public sealed record class None<T>() : IS<Option, T>
{
    public TR Evaluate<TSemantic, TR>(TSemantic semantic)
        where TSemantic : ISemantic1<Option, T, TR>
        => semantic.Prj().None();
}

public sealed record class Some<T>(T Value) : IS<Option, T>
{
    public TR Evaluate<TSemantic, TR>(TSemantic semantic)
        where TSemantic : ISemantic1<Option, T, TR>
        => semantic.Prj().Some(Value);
}

sealed class OptionIdSemantic<T>()
       : IOptionSemantic<T, IS<Option, T>>
{
    public IS<Option, T> None() => new None<T>();
    public IS<Option, T> Some(T value) => new Some<T>(value);
}

sealed record class OptionDiMapSemantic<TS, TI, TO, TR>(
    ISemantic1<Option, TI, TO> Semantic,
    Func<TS, TI> F,
    Func<TO, TR> G
) : IOptionSemantic<TS, TR>
{
    public TR None() => G(Semantic.Prj().None());
    public TR Some(TS value) => G(Semantic.Prj().Some(F(value)));
}

sealed class OptionPureCoSemantic<T>() : ICoSemantic1<Option, T, T>
{
    public TR CoEvaluate<TSemantic, TR>(T x, TSemantic semantic)
        where TSemantic : ISemantic1<Option, T, TR>
        => semantic.Prj().Some(x);
}

sealed class OptionDiSemantic<TS, TR>(Func<TS, TR> F)
    : IDiSemantic<Option, TS, TR>
{
    public ISemantic1<Option, TS, IS<Option, TR>> Forward =>
        Option.IdSemantic<TS>().DiMap(Prelude.Id, e => e.Select(F));

    public TR1 CoEvaluate<TSemantic, TR1>(IS<Option, TS> x, TSemantic semantic)
        where TSemantic : ISemantic1<Option, TR, TR1>
    {
        throw new NotImplementedException();
    }
}

sealed class OptionApplySemantic<TS, TR>()
    : IOptionSemantic<Func<TS, TR>, IDiSemantic<Option, TS, TR>>
{
    public IDiSemantic<Option, TS, TR> None()
    {
        throw new NotImplementedException();
    }

    public IDiSemantic<Option, TS, TR> Some(Func<TS, TR> value)
    {
        throw new NotImplementedException();
    }
}
