using SemanticAlgebra.Control;
using SemanticAlgebra.Syntax;

namespace SemanticAlgebra.Option;

public abstract partial class Option
    : IMonad<Option>
{
    [Semantic1]
    public interface ISemantic<in TS, out TR>
        : ISemantic1<Option, TS, TR>
    {
        TR None();
        TR Some(TS value);
    }

    public static ISemantic1<Option, Func<TS, TR>, ISemantic1<Option, TS, IS<Option, TR>>> ApplyS<TS, TR>()
        => new OptionApplySemantic<TS, TR>();

    public static IS<Option, T> Pure<T>(T value) => B.Some(value);

    public static ISemantic1<Option, IS<Option, T>, IS<Option, T>> JoinS<T>()
        => new OptionJoinSemantic<T>();
}

sealed class OptionApplySemantic<TS, TR>()
    : Option.ISemantic<Func<TS, TR>, ISemantic1<Option, TS, IS<Option, TR>>>
{
    public ISemantic1<Option, TS, IS<Option, TR>> None()
        => Kind1K<Option>.Semantic<TS, IS<Option, TR>>(static _ => Option.B.None<TR>());

    public ISemantic1<Option, TS, IS<Option, TR>> Some(Func<TS, TR> value)
        => Kind1K<Option>.Semantic<TS, IS<Option, TR>>(fs => fs.Select(value));
}

sealed class OptionJoinSemantic<T> : Option.ISemantic<IS<Option, T>, IS<Option, T>>
{
    public IS<Option, T> None() => Option.B.None<T>();
    public IS<Option, T> Some(IS<Option, T> value) => value;
}