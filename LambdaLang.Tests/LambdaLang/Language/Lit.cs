using SemanticAlgebra;
using SemanticAlgebra.Control;
using SemanticAlgebra.Data;
using SemanticAlgebra.Syntax;

namespace LambdaLang.Tests.LambdaLang.Language;

public partial interface Lit
    : IFunctor<Lit>
    , IImplements<Lit, ShowAlgebra, string>
    , IEvalAlgebra<Lit>
{
    [Semantic1]
    public partial interface ISemantic<in TS, out TR>
        : ISemantic1<Lit, TS, TR>
    {
        TR LitI(int value);
    }

    //public static class B
    //{
    //    public static IS<Lit, T> LitI<T>(int value) => new LitI<T>(value);
    //}

    //public static IS<Lit, T> LitI<T>(int value) => new LitI<T>(value);

    //static ISemantic1<Lit, TS, TR> IKind1<Lit>.Compose<TS, TI, TR>(ISemantic1<Lit, TS, TI> s, Func<TI, TR> f)
    //    => new LitComposeSemantic<TS, TI, TR>(s.Prj(), f);

    //static ISemantic1<Lit, T, IS<Lit, T>> IKind1<Lit>.Id<T>()
    //    => new LitIdSemantic<T>();

    //static ISemantic1<Lit, TS, IS<Lit, TR>> IFunctor<Lit>.MapS<TS, TR>(Func<TS, TR> f)
    //    => new LitMapSemantic<TS, TR>();

    static ISemantic1<Lit, string, string> IImplements<Lit, ShowAlgebra, string>.Get()
        => new LitShowFolder();
    static ISemantic1<Lit, IS<M, ISigValue>, IS<M, ISigValue>> IEvalAlgebra<Lit>.Get<M>()
        => new LitEvalFolder<M>();

}

//public interface ILitSemantic<in TS, out TR> : ISemantic1<Lit, TS, TR>
//{
//    TR LitI(int value);
//}

//static class LitExtension
//{
//    public static Lit.ISemantic<TS, TR> Prj<TS, TR>(this ISemantic1<Lit, TS, TR> s)
//        => (Lit.ISemantic<TS, TR>)s;
//}

sealed record class LitI<T>(int Value)
    : IS<Lit, T>
{
    public TR Evaluate<TR>(ISemantic1<Lit, T, TR> semantic)
        => semantic.Prj().LitI(Value);
}

public sealed class LitComposeSemantic<TS, TI, TR>(Lit.ISemantic<TS, TI> S, Func<TI, TR> F) : Lit.ISemantic<TS, TR>
{
    public TR LitI(int value)
        => F(S.LitI(value));
}

public sealed class LitIdSemantic<T>() : Lit.ISemantic<T, IS<Lit, T>>
{
    public IS<Lit, T> LitI(int value)
        => Lit.B.LitI<T>(value);
}

public sealed class LitMapSemantic<TS, TR>() : Lit.ISemantic<TS, IS<Lit, TR>>
{
    public IS<Lit, TR> LitI(int value)
        => Lit.B.LitI<TR>(value);
}

public sealed class LitShowFolder : Lit.ISemantic<string, string>
{
    string Lit.ISemantic<string, string>.LitI(int value)
        => $"{value}";
}

public sealed class LitEvalFolder<M> : Lit.ISemantic<IS<M, ISigValue>, IS<M, ISigValue>>
    where M : IMonad<M>
{
    public IS<M, ISigValue> LitI(int value)
        => M.Pure<ISigValue>(new SigInt(value));
}