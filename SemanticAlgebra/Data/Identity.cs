using SemanticAlgebra.Control;

namespace SemanticAlgebra.Data;

public abstract class Identity
    : IMonad<Identity>
    , IComonad<Identity>

{
    public static IS<Identity, T> Wrap<T>(T value) => new Id<T>(value);

    public static ISemantic1<Identity, T, IS<Identity, T>> Id<T>()
        => new IdentityIdSemantic<T>();
    public static ISemantic1<Identity, TS, TR> Compose<TS, TI, TR>(ISemantic1<Identity, TS, TI> s, Func<TI, TR> f)
        => new IdentityComposeFSemantic<TS, TI, TR>(s.Prj(), f);

    public static ISemantic1<Identity, TS, IS<Identity, TR>> MapS<TS, TR>(Func<TS, TR> f)
        => new IdentityMapSemantic<TS, TR>(f);


    public static T Unwrap<T>(IS<Identity, T> value) => value.Evaluate(ExtractS<T>());

    public static ISemantic1<Identity, Func<TS, TR>, ISemantic1<Identity, TS, IS<Identity, TR>>> ApplyS<TS, TR>()
        => new IdentityApplySemantic<TS, TR>();

    public static IS<Identity, T> Pure<T>(T value) => new Id<T>(value);

    public static ISemantic1<Identity, TS, IS<Identity, TR>> ExtendS<TS, TR>(ISemantic1<Identity, TS, TR> s)
        => new IdentityExtendSemantic<TS, TR>(s);

    public static ISemantic1<Identity, T, T> ExtractS<T>()
        => new IdentityExtractSemantic<T>();

    public static ISemantic1<Identity, IS<Identity, T>, IS<Identity, T>> JoinS<T>()
        => Kind1K<Identity>.Semantic<IS<Identity, T>, IS<Identity, T>>(ffs => ffs.Extract());
}

static class IdentityExtension
{
    public static IIdentitySemantic<TS, TR> Prj<TS, TR>(this ISemantic1<Identity, TS, TR> s)
        => (IIdentitySemantic<TS, TR>)s;
}

interface IIdentitySemantic<in TS, out TR> : ISemantic1<Identity, TS, TR>
{
    TR Unwrap(TS value);
}

sealed record class Id<T>(T Value) : IS<Identity, T>
{
    public TR Evaluate<TR>(ISemantic1<Identity, T, TR> semantic)
        => semantic.Prj().Unwrap(Value);
}

sealed class IdentityIdSemantic<T> : IIdentitySemantic<T, IS<Identity, T>>
{
    public IS<Identity, T> Unwrap(T value)
        => Identity.Wrap(value);
}

sealed class IdentityMapSemantic<TS, TR>(Func<TS, TR> f)
    : IIdentitySemantic<TS, IS<Identity, TR>>
{
    public IS<Identity, TR> Unwrap(TS value)
        => Identity.Wrap(f(value));
}

sealed class IdentityComposeFSemantic<TS, TI, TR>(
    IIdentitySemantic<TS, TI> semantic,
    Func<TI, TR> f
) : IIdentitySemantic<TS, TR>
{
    public TR Unwrap(TS value) => f(semantic.Unwrap(value));
}

sealed class IdentityApplySemantic<TS, TR> : IIdentitySemantic<Func<TS, TR>, ISemantic1<Identity, TS, IS<Identity, TR>>>
{
    public ISemantic1<Identity, TS, IS<Identity, TR>> Unwrap(Func<TS, TR> value)
        => Kind1K<Identity>.Semantic<TS, IS<Identity, TR>>(fs => fs.Select(value));
}

sealed class IdentityExtractSemantic<T> : IIdentitySemantic<T, T>
{
    public T Unwrap(T value) => value;
}

sealed class IdentityExtendSemantic<TS, TR>(ISemantic1<Identity, TS, TR> s) : IIdentitySemantic<TS, IS<Identity, TR>>
{
    public IS<Identity, TR> Unwrap(TS value)
        => Identity.Wrap(s.Prj().Unwrap(value));
}