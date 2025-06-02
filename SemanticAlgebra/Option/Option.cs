using System.Runtime.CompilerServices;
using SemanticAlgebra.Control;
using SemanticAlgebra.Syntax;

namespace SemanticAlgebra.Option;

// [SemanticKind1Brand]
public sealed partial class Option
    : IMonad<Option>
{
    [Semantic1]
    public interface ISemantic<in TS, out TR>
        : ISemantic1<Option, TS, TR>
    {
        TR None();
        TR Some(TS value);
    }

    /// <summary>
    /// Option data value builders
    /// </summary>
    // public static partial class B
    // {
    //     public static IS<Option, T> None<T>() => new D.None<T>();
    //     public static IS<Option, T> Some<T>(T value) => new D.Some<T>(value);
    // }
    //
    // /// <summary>
    // /// Option data definitions 
    // /// </summary>
    // public static class D
    // {
    //     public sealed record class None<T>() : IS<Option, T>
    //     {
    //         public TR Evaluate<TR>(ISemantic1<Option, T, TR> semantic)
    //             => semantic.Prj().None();
    //     }
    //
    //     public sealed record class Some<T>(T Value) : IS<Option, T>
    //     {
    //         public TR Evaluate<TR>(ISemantic1<Option, T, TR> semantic)
    //             => semantic.Prj().Some(Value);
    //     }
    // }
    public static ISemantic1<Option, Func<TS, TR>, ISemantic1<Option, TS, IS<Option, TR>>> ApplyS<TS, TR>()
        => new OptionApplySemantic<TS, TR>();

    public static IS<Option, T> Pure<T>(T value) => B.Some(value);

    public static ISemantic1<Option, IS<Option, T>, IS<Option, T>> JoinS<T>()
        => new OptionJoinSemantic<T>();

    // public static ISemantic1<Option, T, IS<Option, T>> Id<T>()
    // => new IdSemantic<T>();
    // public static global::SemanticAlgebra.ISemantic1<Option, TS, global::SemanticAlgebra.IS<Option, TS>> Id<TS>() => new global::SemanticAlgebra.Option.Option.IdSemantic<TS>();

    // public static
    //     global::SemanticAlgebra.ISemantic1<global::SemanticAlgebra.Option.Option, TS,
    //         global::SemanticAlgebra.IS<global::SemanticAlgebra.Option.Option, TR>>
    //     MapS<TS, TR>(global::System.Func<TS, TR> f) => new global::SemanticAlgebra.Option.Option.MapSemantic<TS, TR>(f);
    //
    // sealed class MapSemantic<TS, TR>(
    //     Func<TS, TR> f) : Option.ISemantic<TS, IS<Option, TR>>
    // {
    //     public IS<Option, TR> None()
    //         => Option.B.None<TR>();
    //
    //     public IS<Option, TR> Some(TS value)
    //         => Option.B.Some(f(value));
    // }
    // public static ISemantic1<Option, TS, IS<Option, TR>> MapS<TS, TR>(Func<TS, TR> f)
    // => new OptionMapSemantic<TS, TR>(f);

    // public static global::SemanticAlgebra.ISemantic1<global::SemanticAlgebra.Option.Option, TS, TR> Compose<TS, TI, TR>(global::SemanticAlgebra.ISemantic1<global::SemanticAlgebra.Option.Option, TS, TI> s, global::System.Func<TI, TR> f) => new global::SemanticAlgebra.Option.Option.ComposeSemantic<TS, TI, TR>(s.Prj(), f);
    // public static ISemantic1<Option, TS, TR> Compose<TS, TI, TR>(ISemantic1<Option, TS, TI> s, Func<TI, TR> f)
    //     => new ComposeSemantic<TS, TI, TR>(s.Prj(), f);

    // sealed class ComposeSemantic<TS, TI, TR>(ISemantic<TS, TI> s, global::System.Func<TI, TR> f)
    //     : global::SemanticAlgebra.ISemantic1<global::SemanticAlgebra.Option.Option, TS, TR>
    // {
    //     public TR None() => f(s.None());
    //     public TR Some(TS value) => f(s.Some(value));
    // }

    // sealed class IdSemantic<TS> : global::SemanticAlgebra.ISemantic1<Option, TS, global::SemanticAlgebra.IS<Option, TS>>
    // {
    //     public global::SemanticAlgebra.IS<Option, TS> None<TS>() => B.None<TS>();
    //     public global::SemanticAlgebra.IS<Option, TS> Some<TS>(TS value) => B.Some<TS>(value);
    // }
    // sealed class IdSemantic<TI>() : ISemantic<TI, IS<Option, TI>>
    // {
    //     public IS<Option, TI> None() => B.None<TI>();
    //     public IS<Option, TI> Some(TI value) => B.Some(value);
    // }
}

// static class OptionExtension
// {
//     public static Option.ISemantic<TS, TR> Prj<TS, TR>(this ISemantic1<Option, TS, TR> s)
//         => (Option.ISemantic<TS, TR>)s;
// }

// sealed class OptionComposeFSemantic<TS, TI, TR>(
//     Option.ISemantic<TS, TI> s,
//     Func<TI, TR> f
// )
//     : Option.ISemantic<TS, TR>
// {
//     public TR None() => f(s.None());
//     public TR Some(TS value) => f(s.Some(value));
// }

sealed class OptionApplySemantic<TS, TR>()
    : Option.ISemantic<Func<TS, TR>, ISemantic1<Option, TS, IS<Option, TR>>>
{
    public ISemantic1<Option, TS, IS<Option, TR>> None()
        => Kind1K<Option>.Semantic<TS, IS<Option, TR>>(static _ => Option.B.None<TR>());

    public ISemantic1<Option, TS, IS<Option, TR>> Some(Func<TS, TR> value)
        => Kind1K<Option>.Semantic<TS, IS<Option, TR>>(fs => fs.Select(value));
}

sealed class OptionJoinSemantic<T>() : Option.ISemantic<IS<Option, T>, IS<Option, T>>
{
    public IS<Option, T> None() => Option.B.None<T>();
    public IS<Option, T> Some(IS<Option, T> value) => value;
}