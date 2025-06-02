using SemanticAlgebra;
using SemanticAlgebra.Control;
using SemanticAlgebra.Data;

namespace LambdaLang.Tests.LambdaLang.Language;

public interface App : IFunctor<App>
{
    public static IS<App, T> Apply<T>(T f, T x) => new Apply<T>(f, x);

    static ISemantic1<App, TS, TR> IKind1<App>.Compose<TS, TI, TR>(ISemantic1<App, TS, TI> s, Func<TI, TR> f)
        => new AppComposeSemantic<TS, TI, TR>(s.Prj(), f);

    static ISemantic1<App, T, IS<App, T>> IKind1<App>.Id<T>()
        => new AppIdSemantic<T>();

    static ISemantic1<App, TS, IS<App, TR>> IFunctor<App>.MapS<TS, TR>(Func<TS, TR> f)
        => new AppMapSemantic<TS, TR>(f);
}

public interface IAppSemantic<in TS, out TR> : ISemantic1<App, TS, TR>
{
    TR Apply(TS f, TS x);
}

static class AppExtension
{
    public static IAppSemantic<TS, TR> Prj<TS, TR>(this ISemantic1<App, TS, TR> s)
        => (IAppSemantic<TS, TR>)s;
}

sealed record class Apply<T>(T F, T X)
    : IS<App, T>
{
    public TR Evaluate<TR>(ISemantic1<App, T, TR> semantic)
        => semantic.Prj().Apply(F, X);
}

public sealed class AppComposeSemantic<TS, TI, TR>(IAppSemantic<TS, TI> S, Func<TI, TR> F) : IAppSemantic<TS, TR>
{
    public TR Apply(TS f, TS x)
        => F(S.Apply(f, x));
}

public sealed class AppIdSemantic<T>() : IAppSemantic<T, IS<App, T>>
{
    public IS<App, T> Apply(T f, T x)
        => App.Apply(f, x);
}

public sealed class AppMapSemantic<TS, TR>(Func<TS, TR> F) : IAppSemantic<TS, IS<App, TR>>
{
    public IS<App, TR> Apply(TS f, TS x)
        => App.Apply(F(f), F(x));
}

public sealed class AppShowFolder : IAppSemantic<string, string>
{
    string IAppSemantic<string, string>.Apply(string f, string x)
        => $"{f}({x})";
}

public sealed class AppEvalFolder<M> : IAppSemantic<IS<M, ISigValue>, IS<M, ISigValue>>
    where M : IMonad<M>
{
    public IS<M, ISigValue> Apply(IS<M, ISigValue> f, IS<M, ISigValue> x)
        => from f_ in f
           from x_ in x
           select f_ switch
           {
               SigLam func => func.F(x_),
               _ => throw new InvalidOperationException(
                   "Function application requires a function and an integer argument")
           };
}