using SemanticAlgebra;
using SemanticAlgebra.Control;
using SemanticAlgebra.Data;
using SemanticAlgebra.Syntax;

namespace LambdaLang.Tests.LambdaLang.Language;

public partial interface Arith
    : IFunctor<Arith>
    , IWithAlgebra<Arith, ShowAlgebra, string>
    , IEvalAlgebra<Arith>
{
    //public static IS<Arith, T> Add<T>(T left, T right) => new Add<T>(left, right);
    //public static IS<Arith, T> Mul<T>(T left, T right) => new Mul<T>(left, right);

    //static ISemantic1<Arith, TS, TR> IKind1<Arith>.Compose<TS, TI, TR>(ISemantic1<Arith, TS, TI> s, Func<TI, TR> f)
    //    => new ArithComposeSemantic<TS, TI, TR>(s.Prj(), f);

    //static ISemantic1<Arith, T, IS<Arith, T>> IKind1<Arith>.Id<T>()
    //    => new ArithIdSemantic<T>();

    //static ISemantic1<Arith, TS, IS<Arith, TR>> IFunctor<Arith>.MapS<TS, TR>(Func<TS, TR> f)
    //    => new ArithMapSemantic<TS, TR>(f);

    static ISemantic1<Arith, string, string> IWithAlgebra<Arith, ShowAlgebra, string>.Get()
        => new ArithShowFolder();

    static ISemantic1<Arith, IS<M, ISigValue>, IS<M, ISigValue>> IEvalAlgebra<Arith>.Get<M>()
        => new ArithEvalFolder<M>();

    [Semantic1]
    public partial interface ISemantic<in TS, out TR> 
        : ISemantic1<Arith, TS, TR>
    {
        TR Add(TS l, TS r);
        TR Sub(TS l, TS r);
        TR Mul(TS l, TS r);
    }
}

//public interface IArithSemantic<in TS, out TR> : ISemantic1<Arith, TS, TR>
//{
//    TR Add(TS l, TS r);
//    TR Mul(TS l, TS r);
//}

//static class ArithExtension
//{
//    public static Arith.ISemantic<TS, TR> Prj<TS, TR>(this ISemantic1<Arith, TS, TR> s)
//        => (Arith.ISemantic<TS, TR>)s;
//}

//sealed record class Add<T>(T Left, T Right)
//    : IS<Arith, T>
//{
//    public TR Evaluate<TR>(ISemantic1<Arith, T, TR> semantic)
//        => semantic.Prj().Add(Left, Right);
//}

//sealed record class Mul<T>(T Left, T Right)
//    : IS<Arith, T>
//{
//    public TR Evaluate<TR>(ISemantic1<Arith, T, TR> semantic)
//        => semantic.Prj().Mul(Left, Right);
//}

//public sealed class ArithComposeSemantic<TS, TI, TR>(Arith.ISemantic<TS, TI> S, Func<TI, TR> F) : Arith.ISemantic<TS, TR>
//{
//    public TR Add(TS l, TS r)
//        => F(S.Add(l, r));

//    public TR Mul(TS l, TS r)
//        => F(S.Mul(l, r));
//}

//public sealed class ArithIdSemantic<T>() : Arith.ISemantic<T, IS<Arith, T>>
//{
//    public IS<Arith, T> Add(T l, T r)
//        => Arith.Add(l, r);

//    public IS<Arith, T> Mul(T l, T r)
//        => Arith.Mul(l, r);
//}

//public sealed class ArithMapSemantic<TS, TR>(Func<TS, TR> F) : Arith.ISemantic<TS, IS<Arith, TR>>
//{
//    public IS<Arith, TR> Add(TS l, TS r)
//        => Arith.Add(F(l), F(r));

//    public IS<Arith, TR> Mul(TS l, TS r)
//        => Arith.Mul(F(l), F(r));
//}

public sealed class ArithShowFolder : Arith.ISemantic<string, string>
{
    public string Sub(string l, string r)
        => $"({l} - {r})";

    string Arith.ISemantic<string, string>.Add(string l, string r)
        => $"({l} + {r})";

    string Arith.ISemantic<string, string>.Mul(string l, string r)
        => $"({l} * {r})";
}

public sealed class ArithEvalFolder<M> : Arith.ISemantic<IS<M, ISigValue>, IS<M, ISigValue>>
    where M : IMonad<M>
{
    public IS<M, ISigValue> Add(IS<M, ISigValue> l, IS<M, ISigValue> r)
        => from l_ in l
           from r_ in r
           select (l_, r_) switch
           {
               (SigInt left, SigInt right) => (ISigValue)new SigInt(left.Value + right.Value),
               _ => throw new EvalRuntimeException($"Add requires int arguments, got {l_}, {r_}")
           };

    public IS<M, ISigValue> Mul(IS<M, ISigValue> l, IS<M, ISigValue> r)
        => from l_ in l
           from r_ in r
           select (l_, r_) switch
           {
               (SigInt left, SigInt right) => (ISigValue)new SigInt(left.Value * right.Value),
               _ => throw new EvalRuntimeException($"Mul requires int arguments, got {l_}, {r_}")
           };

    public IS<M, ISigValue> Sub(IS<M, ISigValue> l, IS<M, ISigValue> r)
        => from l_ in l
           from r_ in r
           select (l_, r_) switch
           {
               (SigInt left, SigInt right) => (ISigValue)new SigInt(left.Value - right.Value),
               _ => throw new EvalRuntimeException($"Sub requires int arguments, got {l_}, {r_}")
           };
}