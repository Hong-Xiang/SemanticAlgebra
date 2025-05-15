namespace SemanticAlgebra.Data;

public sealed class Identity
    : IFunctor<Identity>
{
    public static IDiSemantic<Identity, T, T> Id<T>()
        => new IdentityIdSemantic<T>().AsDiSemantic();

    public static IS<Identity, T> Wrap<T>(T value) => new Id<T>(value);

    public static IDiSemantic<Identity, TS, TR> Map<TS, TR>(Func<TS, TR> f)
        => new IdentityMapSemantic<TS, TR>(f).AsDiSemantic();
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

sealed class IdentityMapSemantic<TS, TR>(Func<TS, TR> F)
    : IIdentitySemantic<TS, IS<Identity, TR>>
{
    public IS<Identity, TR> Unwrap(TS value)
        => Identity.Wrap(F(value));
}
