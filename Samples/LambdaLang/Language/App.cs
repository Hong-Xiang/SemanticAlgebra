using SemanticAlgebra;
using SemanticAlgebra.Data;
using System;

namespace LambdaLang.Language;

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

public sealed class AppEvalFolder : IAppSemantic<SigEvalData, SigEvalData>
{
    public SigEvalData Apply(SigEvalData f, SigEvalData x)
    {
        return SigEvalState.From(s =>
        {
            var fr = f.Run(s);
            var xr = x.Run(fr.State);
            return (fr.Data, xr.Data) switch
            {
                (SigLam func, ISigValue arg) => (xr.State, func.F(arg)),
                _ => throw new InvalidOperationException("Function application requires a function and an integer argument")
            };
        });
    }

}

