namespace SemanticAlgebra.Data;

public abstract class Constant<S>
    : IFunctor<Constant<S>>
{
    public interface IAlias1<TS>
        : Alias1.ISpec<Constant<S>, TS, S>
    {
    }

    public static class B
    {
        public static IS<Constant<S>, TR> From<TR>(S value)
            => IAlias1<TR>.From(value);
    }

    public static ISemantic1<Constant<S>, T, IS<Constant<S>, T>> Id<T>()
        => IAlias1<T>.Id();

    public static ISemantic1<Constant<S>, TS, TR> Compose<TS, TI, TR>(
        ISemantic1<Constant<S>, TS, TI> s,
        Func<TI, TR> f)
        => IAlias1<TS>.Compose(s, f);

    public static ISemantic1<Constant<S>, TS, IS<Constant<S>, TR>> MapS<TS, TR>(Func<TS, TR> f)
        => IAlias1<TS>.Semantic(B.From<TR>);
}

public static partial class ConstantExtension
{
    public static S Unwrap<S, TR>(this IS<Constant<S>, TR> e)
        => Constant<S>.IAlias1<TR>.Unwrap(e);

    public static Constant<S>.IAlias1<TS>.ISemantic<TR> Prj<S, TS, TR>(this
        ISemantic1<Constant<S>, TS, TR> s)
        => Constant<S>.IAlias1<TS>.Prj(s);


}

