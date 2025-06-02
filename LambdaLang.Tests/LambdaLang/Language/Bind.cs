using SemanticAlgebra;
using SemanticAlgebra.Control;
using SemanticAlgebra.Data;
using SemanticAlgebra.Syntax;

namespace LambdaLang.Tests.LambdaLang.Language;

public partial interface Bind : IFunctor<Bind>
{
    // public static IS<Bind, T> Let<T>(Identifier name, T expr, T body) => new Let<T>(name, expr, body);

    // static ISemantic1<Bind, TS, TR> IKind1<Bind>.Compose<TS, TI, TR>(ISemantic1<Bind, TS, TI> s, Func<TI, TR> f)
    //     => new BindComposeSemantic<TS, TI, TR>(s.Prj(), f);

    // static ISemantic1<Bind, T, IS<Bind, T>> global::SemanticAlgebra.IKind1<Bind>.Id<T>()
    //     => new BindIdSemantic<T>();

    // static ISemantic1<Bind, TS, IS<Bind, TR>> IFunctor<Bind>.MapS<TS, TR>(Func<TS, TR> f)
    //     => new BindMapSemantic<TS, TR>(f);


    [Semantic1]
    public interface ISemantic<in TS, out TR> : ISemantic1<Bind, TS, TR>
    {
        TR Let(Identifier name, TS expr, TS body);
    }
}

public sealed class BindShowFolder : Bind.ISemantic<string, string>
{
    string Bind.ISemantic<string, string>.Let(Identifier name, string expr, string body)
        => $"(let {name.Name} = {expr} in {body})";
}

public sealed class BindEvalFolder : Bind.ISemantic<SigEvalData, SigEvalData>
{
    public SigEvalData Let(Identifier name, SigEvalData expr, SigEvalData body)
        => SigEvalState.From(s =>
        {
            var e = expr.Run(s);
            var s2 = s.Add(name, e.Data);
            var br = body.Run(s2);
            return (br.State, br.Data);
        });
}

// static class BindExtension
// {
//     public static IBindSemantic<TS, TR> Prj<TS, TR>(this ISemantic1<Bind, TS, TR> s)
//         => (IBindSemantic<TS, TR>)s;
// }

// sealed record class Let<T>(Identifier Name, T Expr, T Body)
//     : IS<Bind, T>
// {
//     public TR Evaluate<TR>(ISemantic1<Bind, T, TR> semantic)
//         => semantic.Prj().Let(Name, Expr, Body);
// }

// public sealed class BindComposeSemantic<TS, TI, TR>(Bind.ISemantic<TS, TI> S, Func<TI, TR> F) : Bind.ISemantic<TS, TR>
// {
//     public TR Let(Identifier name, TS expr, TS body)
//         => F(S.Let(name, expr, body));
// }
//
// public sealed class BindIdSemantic<T>() : Bind.ISemantic<T, IS<Bind, T>>
// {
//     public IS<Bind, T> Let(Identifier name, T expr, T body)
//         => Bind.B.Let(name, expr, body);
// }
//
// //
// public sealed class BindMapSemantic<TS, TR>(Func<TS, TR> F) : Bind.ISemantic<TS, IS<Bind, TR>>
// {
//     public IS<Bind, TR> Let(Identifier name, TS expr, TS body)
//         => Bind.B.Let(name, F(expr), F(body));
// }