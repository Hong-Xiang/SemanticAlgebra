using SemanticAlgebra.Control;
using SemanticAlgebra.Syntax;

namespace SemanticAlgebra.Data;

public abstract partial class Identity
    : IMonad<Identity>
    , IComonad<Identity>

{
    public static class B
    {
        public static IS<Identity, T> From<T>(T value)
            => Alias1.B.From<Identity, T, T>(value);
    }


    // public static IS<Identity, T> Wrap<T>(T value) => new Id<T>(value);

    public static ISemantic1<Identity, T, IS<Identity, T>> Id<T>()
        => Alias1.Id<Identity, T, T>();

    //
    public static ISemantic1<Identity, TS, TR> Compose<TS, TI, TR>(ISemantic1<Identity, TS, TI> s, Func<TI, TR> f)
        => Alias1.Compose<Identity, TS, TI, TR, TS>(s, f);

    //
    public static ISemantic1<Identity, TS, IS<Identity, TR>> MapS<TS, TR>(Func<TS, TR> f)
        => Id<TS>().Compose(e => B.From(f(e.Unwrap())));


    // public static T Unwrap<T>(IS<Identity, T> value) => value.Evaluate(ExtractS<T>());


    public static ISemantic1<Identity, Func<TS, TR>, ISemantic1<Identity, TS, IS<Identity, TR>>> ApplyS<TS, TR>()
        => Id<Func<TS, TR>>().Compose(f => Kind1K<Identity>.Semantic<TS, IS<Identity, TR>>(e => e.Select(f.Unwrap())));

    public static IS<Identity, T> Pure<T>(T value) => B.From(value);

    public static ISemantic1<Identity, TS, IS<Identity, TR>> ExtendS<TS, TR>(ISemantic1<Identity, TS, TR> s)
        => s.Compose(PureK<Identity>.Pure);

    public static ISemantic1<Identity, T, T> ExtractS<T>()
        // => new ExtractSemantic<T>();
        => Id<T>().Compose(static e => e.Unwrap());

    public static ISemantic1<Identity, IS<Identity, T>, IS<Identity, T>> JoinS<T>()
        => Id<IS<Identity, T>>().Compose(e => e.Unwrap());
    // => Kind1K<Identity>.Semantic<IS<Identity, T>, IS<Identity, T>>(ffs => ffs.Extract());

    // sealed class ExtractSemantic<T> : ISemantic<T, T>
    // {
    //     public T From(T value) => value;
    // }

    public interface ISemantic<in TS, out TR>
        : Alias1.ISemantic<Identity, TS, TR, TS>
    {
    }
}

static class IdentityExtension
{
    public static Identity.ISemantic<TS, TR> Prj<TS, TR>(this ISemantic1<Identity, TS, TR> s)
        => (Identity.ISemantic<TS, TR>)s;


    public static T Unwrap<T>(this IS<Identity, T> e)
        => ((Alias1.D.From<Identity, T, T>)e).Value;
}

// sealed record class Id<T>(T Value) : IS<Identity, T>
// {
//     public TR Evaluate<TR>(ISemantic1<Identity, T, TR> semantic)
//         => semantic.Prj().Unwrap(Value);
// }
//
// sealed class IdentityIdSemantic<T> : IIdentitySemantic<T, IS<Identity, T>>
// {
//     public IS<Identity, T> Unwrap(T value)
//         => Identity.Wrap(value);
// }

// sealed class IdentityComposeFSemantic<TS, TI, TR>(
//     IIdentitySemantic<TS, TI> semantic,
//     Func<TI, TR> f
// ) : IIdentitySemantic<TS, TR>
// {
//     public TR Unwrap(TS value) => f(semantic.Unwrap(value));
// }

// sealed class
//     IdentityApplySemantic<TS, TR> : Identity.ISemantic<Func<TS, TR>, ISemantic1<Identity, TS, IS<Identity, TR>>>
// {
//     public ISemantic1<Identity, TS, IS<Identity, TR>> From(Func<TS, TR> value)
//         => Kind1K<Identity>.Semantic<TS, IS<Identity, TR>>(fs => fs.Select(value));
// }