using SemanticAlgebra;
using SemanticAlgebra.Data;
using SemanticAlgebra.Fix;
using SemanticAlgebra.Syntax;

namespace LambdaLang.Tests.IntLang;

public sealed partial class IntLang : IFunctor<IntLang>
{
    // public static IS<IntLang, T> LitI<T>(int value) => new LitI<T>(value);
    // public static IS<IntLang, T> Add<T>(T a, T b) => new Add<T>(a, b);


    [Semantic1]
    public interface ISemantic<in TS, out TR> : ISemantic1<IntLang, TS, TR>
    {
        TR LitI(int value);
        TR Add(TS a, TS b);
    }

    // public static ISemantic1<IntLang, T, IS<IntLang, T>> Id<T>()
    // => new IntLangIdSemantic<T>();

    // public static ISemantic1<IntLang, TS, TR> Compose<TS, TI, TR>(ISemantic1<IntLang, TS, TI> s, Func<TI, TR> f)
    // => new IntLangComposeSemantic<TS, TI, TR>(s.Prj(), f);

    // public static ISemantic1<IntLang, TS, IS<IntLang, TR>> MapS<TS, TR>(Func<TS, TR> f)
    //     => new IntLangMapSemantic<TS, TR>(f);


    public static ISemantic<Fix<IntLang>, Fix<IntLang>> SyntaxFactory => Fix<IntLang>.SyntaxFactory.Prj();
}

// static class IntLangExtension
// {
//     public static IntLang.ISemantic<TS, TR> Prj<TS, TR>(this ISemantic1<IntLang, TS, TR> s)
//         => (IntLang.ISemantic<TS, TR>)s;
// }

// sealed record class LitI<T>(int Value) : IS<IntLang, T>
// {
//     public TR Evaluate<TR>(ISemantic1<IntLang, T, TR> semantic)
//         => semantic.Prj().LitI(Value);
// }
//
// sealed record class Add<T>(T A, T B) : IS<IntLang, T>
// {
//     public TR Evaluate<TR>(ISemantic1<IntLang, T, TR> semantic)
//         => semantic.Prj().Add(A, B);
// }

// sealed class IntLangIdSemantic<T> : IIntLangSemantic<T, IS<IntLang, T>>
// {
//     public IS<IntLang, T> Add(T a, T b)
//         => IntLang.Add(a, b);
//
//     public IS<IntLang, T> LitI(int value)
//         => IntLang.LitI<T>(value);
// }
//
// sealed class IntLangComposeSemantic<TS, TI, TR>(
//     IIntLangSemantic<TS, TI> s,
//     Func<TI, TR> f
// ) : IIntLangSemantic<TS, TR>
// {
//     public TR LitI(int value) => f(s.LitI(value));
//     public TR Add(TS a, TS b) => f(s.Add(a, b));
// }
//
// sealed class IntLangMapSemantic<TS, TR>(
//     Func<TS, TR> F)
//     : IntLang.ISemantic<TS, IS<IntLang, TR>>
// {
//     public IS<IntLang, TR> LitI(int value)
//         => IntLang.B.LitI<TR>(value);
//
//     public IS<IntLang, TR> Add(TS a, TS b)
//         => IntLang.B.Add(F(a), F(b));
// }

sealed class IntFolder : IntLang.ISemantic<int, int>
{
    public int LitI(int value) => value;
    public int Add(int a, int b) => a + b;
}