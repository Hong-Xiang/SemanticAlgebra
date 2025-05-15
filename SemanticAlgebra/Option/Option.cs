using SemanticAlgebra.Data;

namespace SemanticAlgebra.Option;

public sealed class Option
    : IMonad<Option>
{
    //public static ISemantic1<Option, TS, TR> Semantic<TS, TR>(Func<IS<Option, TS>, TR> f)
    //    => new OptionFuncSemantic<TS, TR>(f);

    static ISemantic1<Option, TS, TR> IFunctor<Option>.SemanticDiMap<TS, TI, TO, TR>(ISemantic1<Option, TI, TO> semantic, Func<TS, TI> f, Func<TO, TR> g)
       => new OptionDiMapSemantic<TS, TI, TO, TR>(semantic, f, g);

    public static IS<Option, T> None<T>() => Prelude.IdSemantic<Option, T>().Prj().None();
    public static IS<Option, T> Some<T>(T value) => Prelude.IdSemantic<Option, T>().Prj().Some(value);

    public static ISemantic1<Option, Func<TS, TR>, IDiSemantic<Option, TS, TR>> ApplySemantic<TS, TR>()
        => new OptionApplySemantic<TS, TR>();

    public static ICoSemantic1<Option, T, T> PureCoSemantic<T>()
        => new OptionPureCoSemantic<T>();

    public static ISemantic1<Option, TS, TR> Semantic<TS, TR>(Func<IS<Option, TS>, TR> f)
        => new OptionFuncSemantic<TS, TR>(f);

    public static ISemantic1<Option, IS<Option, T>, IS<Option, T>> JoinSemantic<T>()
        => new OptionJoinSemantic<T>();
}

public static class OptionExtension
{
    public static IOptionSemantic<TS, TR> Prj<TS, TR>(
        this ISemantic1<Option, TS, TR> semantic
    ) => (IOptionSemantic<TS, TR>)semantic;
}

public interface IOptionSemantic<in TS, out TR>
    : ISemantic1<Option, TS, TR>
{
    TR None();
    TR Some(TS value);
}

public sealed record class None<T>() : IS<Option, T>
{
    public TR Evaluate<TSemantic, TR>(TSemantic semantic)
        where TSemantic : ISemantic1<Option, T, TR>
        => semantic.Prj().None();
}

public sealed record class Some<T>(T Value) : IS<Option, T>
{
    public TR Evaluate<TSemantic, TR>(TSemantic semantic)
        where TSemantic : ISemantic1<Option, T, TR>
        => semantic.Prj().Some(Value);
}

sealed class OptionFuncSemantic<TS, TR>(Func<IS<Option, TS>, TR> F)
       : IOptionSemantic<TS, TR>
{
    public TR None() => F(new None<TS>());
    public TR Some(TS value) => F(new Some<TS>(value));
}

sealed record class OptionDiMapSemantic<TS, TI, TO, TR>(
    ISemantic1<Option, TI, TO> Semantic,
    Func<TS, TI> F,
    Func<TO, TR> G
) : IOptionSemantic<TS, TR>
{
    public TR None() => G(Semantic.Prj().None());
    public TR Some(TS value) => G(Semantic.Prj().Some(F(value)));
}

sealed class OptionPureCoSemantic<T>() : ICoSemantic1<Option, T, T>
{
    public TR CoEvaluate<TSemantic, TR>(T x, TSemantic semantic)
        where TSemantic : ISemantic1<Option, T, TR>
        => semantic.Prj().Some(x);
}

sealed class OptionApplySemantic<TS, TR>()
    : IOptionSemantic<Func<TS, TR>, IDiSemantic<Option, TS, TR>>
{
    public IDiSemantic<Option, TS, TR> None() => DiSemantic.FromFunc<Option, TS, TR>(static fs => Option.None<TR>());
    public IDiSemantic<Option, TS, TR> Some(Func<TS, TR> value)
        => DiSemantic.FromFunc<Option, TS, TR>(fs => fs.Select(value));
}

sealed class OptionJoinSemantic<T>() : IOptionSemantic<IS<Option, T>, IS<Option, T>>
{
    public IS<Option, T> None()
        => Option.None<T>();

    public IS<Option, T> Some(IS<Option, T> value)
        => value;
}
