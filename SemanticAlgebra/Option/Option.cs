using SemanticAlgebra.Control;
using SemanticAlgebra.Data;

namespace SemanticAlgebra.Option;

public sealed class Option
    : IMonad<Option>
{
    public static IS<Option, T> None<T>() => new None<T>();
    public static IS<Option, T> Some<T>(T value) => new Some<T>(value);

    public static IDiSemantic<Option, TS, TR> Map<TS, TR>(Func<TS, TR> f)
        => new OptionMapSemantic<TS, TR>(f).AsDiSemantic();

    public static ISemantic1<Option, Func<TS, TR>, IDiSemantic<Option, TS, TR>> Apply<TS, TR>()
        => new OptionApplySemantic<TS, TR>();

    public static ICoSemantic1<Option, T, T> Pure<T>()
        => new OptionPureCoSemantic<T>();

    public static ISemantic1<Option, IS<Option, T>, IS<Option, T>> Join<T>()
        => new OptionJoinSemantic<T>();

    public static IDiSemantic<Option, T, T> Id<T>()
        => new OptionIdSemantic<T>().AsDiSemantic();
}

public interface IOptionSemantic<in TS, out TR>
    : ISemantic1<Option, TS, TR>
{
    TR None();
    TR Some(TS value);
}

public static class OptionExtension
{
    public static IOptionSemantic<TS, TR> Prj<TS, TR>(
        this ISemantic1<Option, TS, TR> semantic
    ) => (IOptionSemantic<TS, TR>)semantic;
}



public sealed record class None<T>() : IS<Option, T>
{
    public TR Evaluate<TR>(ISemantic1<Option, T, TR> semantic)
        => semantic.Prj().None();
}

public sealed record class Some<T>(T Value) : IS<Option, T>
{
    public TR Evaluate<TR>(ISemantic1<Option, T, TR> semantic)
        => semantic.Prj().Some(Value);
}

sealed class OptionIdSemantic<T>() : IOptionSemantic<T, IS<Option, T>>
{
    public IS<Option, T> None() => Option.None<T>();
    public IS<Option, T> Some(T value) => Option.Some(value);
}

sealed class OptionMapSemantic<TS, TR>(Func<TS, TR> F) : IOptionSemantic<TS, IS<Option, TR>>
{
    public IS<Option, TR> None() => Option.None<TR>();

    public IS<Option, TR> Some(TS value) => Option.Some(F(value));
}

sealed class OptionPureCoSemantic<T>() : ICoSemantic1<Option, T, T>
{
    public TR CoEvaluate<TR>(T x, ISemantic1<Option, T, TR> semantic)
        => semantic.Prj().Some(x);
}

sealed class OptionApplySemantic<TS, TR>()
    : IOptionSemantic<Func<TS, TR>, IDiSemantic<Option, TS, TR>>
{
    public IDiSemantic<Option, TS, TR> None()
        => Kind1K<Option>.Id<TS>()
            .Semantic
            .MapR(static _ => Option.None<TR>()).AsDiSemantic();

    public IDiSemantic<Option, TS, TR> Some(Func<TS, TR> value)
        => DiSemantic.FromFunc<Option, TS, TR>(fs => fs.Select(value));
}

sealed class OptionJoinSemantic<T>() : IOptionSemantic<IS<Option, T>, IS<Option, T>>
{
    public IS<Option, T> None() => Option.None<T>();
    public IS<Option, T> Some(IS<Option, T> value) => value;
}
