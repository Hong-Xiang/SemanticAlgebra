using SemanticAlgebra.Control;

namespace SemanticAlgebra.Data;

public abstract class Identity
    : IMonad<Identity>
    , ICoMonad<Identity>

{
    public static IDiSemantic<Identity, T, T> Id<T>()
        => new IdentityIdSemantic<T>().AsDiSemantic();

    public static IDiSemantic<Identity, TS, TR> Map<TS, TR>(Func<TS, TR> f)
        => new IdentityMapSemantic<TS, TR>(f).AsDiSemantic();

    public static ISemantic1<Identity, TS, TR> ComposeF<TS, TI, TR>(ISemantic1<Identity, TS, TI> s, Func<TI, TR> f)
        => new IdentityComposeFSemantic<TS, TI, TR>(s.Prj(), f);

    public static IS<Identity, T> Wrap<T>(T value) => new Id<T>(value);

    public static T Unwrap<T>(IS<Identity, T> value) => value.Evaluate(Extract<T>());

    public static ISemantic1<Identity, Func<TS, TR>, IDiSemantic<Identity, TS, TR>> Apply<TS, TR>()
        => new IdentityApplySemantic<TS, TR>();

    public static ICoSemantic1<Identity, T, T> Pure<T>()
        => new IdentityPureCoSemantic<T>();

    public static IDiSemantic<Identity, TS, TR> Extend<TS, TR>(ISemantic1<Identity, TS, TR> s)
        => new IdentityExtendSemantic<TS, TR>(s).AsDiSemantic();

    public static ISemantic1<Identity, T, T> Extract<T>()
        => new IdentityExtractSemantic<T>();
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

sealed class IdentityPureCoSemantic<T> : ICoSemantic1<Identity, T, T>
{
    public TO CoEvaluate<TO>(T x, ISemantic1<Identity, T, TO> s)
        => s.Prj().Unwrap(x);
}

sealed class IdentityApplySemantic<TS, TR> : IIdentitySemantic<Func<TS, TR>, IDiSemantic<Identity, TS, TR>>
{
    public IDiSemantic<Identity, TS, TR> Unwrap(Func<TS, TR> value)
        => Kind1K<Identity>.DiSemantic<TS, TR>(fs => fs.Select(value));
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