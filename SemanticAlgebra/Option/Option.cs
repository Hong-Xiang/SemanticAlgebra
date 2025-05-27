using SemanticAlgebra.Control;
using SemanticAlgebra.Data;

namespace SemanticAlgebra.Option;

public sealed class Option
    : IMonad<Option>
{
    public static IS<Option, T> None<T>() => new None<T>();
    public static IS<Option, T> Some<T>(T value) => new Some<T>(value);


    public static ISemantic1<Option, Func<TS, TR>, ISemantic1<Option, TS, IS<Option, TR>>> ApplyS<TS, TR>()
        => new OptionApplySemantic<TS, TR>();

    public static IS<Option, T> Pure<T>(T value) => Some(value);

    public static ISemantic1<Option, IS<Option, T>, IS<Option, T>> JoinS<T>()
        => new OptionJoinSemantic<T>();


    public static ISemantic1<Option, T, IS<Option, T>> Id<T>()
        => new OptionIdSemantic<T>();

    public static ISemantic1<Option, TS, IS<Option, TR>> MapS<TS, TR>(Func<TS, TR> f)
        => new OptionMapSemantic<TS, TR>(f);

    public static ISemantic1<Option, TS, TR> Compose<TS, TI, TR>(ISemantic1<Option, TS, TI> s, Func<TI, TR> f)
        => new OptionComposeFSemantic<TS, TI, TR>(s.Prj(), f);
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

sealed class OptionMapSemantic<TS, TR>(
    Func<TS, TR> f) : IOptionSemantic<TS, IS<Option, TR>>
{
    public IS<Option, TR> None()
        => Option.None<TR>();

    public IS<Option, TR> Some(TS value)
        => Option.Some(f(value));
}

sealed class OptionIdSemantic<T>() : IOptionSemantic<T, IS<Option, T>>
{
    public IS<Option, T> None() => Option.None<T>();
    public IS<Option, T> Some(T value) => Option.Some(value);
}

sealed class OptionComposeFSemantic<TS, TI, TR>(
    IOptionSemantic<TS, TI> s,
    Func<TI, TR> f
)
    : IOptionSemantic<TS, TR>
{
    public TR None() => f(s.None());
    public TR Some(TS value) => f(s.Some(value));
}

sealed class OptionApplySemantic<TS, TR>()
    : IOptionSemantic<Func<TS, TR>, ISemantic1<Option, TS, IS<Option, TR>>>
{
    public ISemantic1<Option, TS, IS<Option, TR>> None()
        => Kind1K<Option>.Semantic<TS, IS<Option, TR>>(static _ => Option.None<TR>());
    public ISemantic1<Option, TS, IS<Option, TR>> Some(Func<TS, TR> value)
        => Kind1K<Option>.Semantic<TS, IS<Option, TR>>(fs => fs.Select(value));
}

sealed class OptionJoinSemantic<T>() : IOptionSemantic<IS<Option, T>, IS<Option, T>>
{
    public IS<Option, T> None() => Option.None<T>();
    public IS<Option, T> Some(IS<Option, T> value) => value;
}