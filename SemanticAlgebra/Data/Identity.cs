using SemanticAlgebra.Control;

namespace SemanticAlgebra.Data;

public abstract partial class Identity
    : IMonad<Identity>
    , IComonad<Identity>

{
    public interface IAlias1<TS> : Alias1.ISpec<Identity, TS, TS>
    {
    }

    public static class B
    {
        public static IS<Identity, T> From<T>(T value)
            => Alias1.B.From<Identity, T, T>(value);
    }


    public static ISemantic1<Identity, T, IS<Identity, T>> Id<T>()
        => Alias1.Id<Identity, T, T>();

    public static ISemantic1<Identity, TS, TR> Compose<TS, TI, TR>(ISemantic1<Identity, TS, TI> s, Func<TI, TR> f)
        => Alias1.Compose<Identity, TS, TI, TR, TS>(s, f);

    public static ISemantic1<Identity, TS, IS<Identity, TR>> MapS<TS, TR>(Func<TS, TR> f)
        => IAlias1<TS>.Semantic(s => B.From(f(s)));


    public static ISemantic1<Identity, Func<TS, TR>, ISemantic1<Identity, TS, IS<Identity, TR>>> ApplyS<TS, TR>()
        => IAlias1<Func<TS, TR>>.Semantic(MapS);

    public static IS<Identity, T> Pure<T>(T value) => B.From(value);

    public static ISemantic1<Identity, TS, IS<Identity, TR>> ExtendS<TS, TR>(ISemantic1<Identity, TS, TR> s)
        => s.Compose(B.From);

    public static ISemantic1<Identity, T, T> ExtractS<T>()
        => IAlias1<T>.Semantic(Prelude.Id);


    public static ISemantic1<Identity, IS<Identity, T>, IS<Identity, T>> JoinS<T>()
        => IAlias1<IS<Identity, T>>.Semantic(Prelude.Id);

    public static IDistributeTransform<Identity, F> DistributeTransformFromIdentity<F>()
        where F : IFunctor<F>
        => new DistributeIdentityFunctor<F>();

    public static IDistributeTransform<F, Identity> DistributeTransformToIdentity<F>()
        where F : IFunctor<F>
        => new DistributeFunctorIdentity<F>();
}

public static class IdentityExtension
{
    public static Identity.IAlias1<TS>.ISemantic<TR> Prj<TS, TR>(this ISemantic1<Identity, TS, TR> s)
        => (Identity.IAlias1<TS>.ISemantic<TR>)s;


    public static T Unwrap<T>(this IS<Identity, T> e)
        => Identity.IAlias1<T>.Unwrap(e);
}

public sealed class DistributeFunctorIdentity<G> : IDistributeTransform<G, Identity>
    where G : IFunctor<G>
{
    public IS<Identity, IS<G, T>> Distribute<T>(IS<G, IS<Identity, T>> fg)
        => Identity.B.From(fg.Select(static x => x.Unwrap()));
}

public sealed class DistributeIdentityFunctor<F> : IDistributeTransform<Identity, F>
    where F : IFunctor<F>
{
    public IS<F, IS<Identity, T>> Distribute<T>(IS<Identity, IS<F, T>> fg)
        => fg.Unwrap().Select(Identity.B.From);
}