using SemanticAlgebra.Data;
using SemanticAlgebra;

namespace LambdaLang.Language;

interface Arith : IFunctor<Arith>
{
    public static IS<Arith, T> Add<T>(T left, T right) => new Add<T>(left, right);
    public static IS<Arith, T> Mul<T>(T left, T right) => new Mul<T>(left, right);

    static ISemantic1<Arith, TS, TR> IKind1<Arith>.Compose<TS, TI, TR>(ISemantic1<Arith, TS, TI> s, Func<TI, TR> f)
        => new ArithComposeSemantic<TS, TI, TR>(s.Prj(), f);

    static ISemantic1<Arith, T, IS<Arith, T>> IKind1<Arith>.Id<T>()
        => new ArithIdSemantic<T>();

    static ISemantic1<Arith, TS, IS<Arith, TR>> IFunctor<Arith>.MapS<TS, TR>(Func<TS, TR> f)
        => new ArithMapSemantic<TS, TR>(f);
}

interface IArithSemantic<in TS, out TR> : ISemantic1<Arith, TS, TR>
{
    TR Add(TS l, TS r);
    TR Mul(TS l, TS r);
}

static class ArithExtension
{
    public static IArithSemantic<TS, TR> Prj<TS, TR>(this ISemantic1<Arith, TS, TR> s)
        => (IArithSemantic<TS, TR>)s;
}

sealed record class Add<T>(T Left, T Right)
    : IS<Arith, T>
{
    public TR Evaluate<TR>(ISemantic1<Arith, T, TR> semantic)
        => semantic.Prj().Add(Left, Right);
}

sealed record class Mul<T>(T Left, T Right)
    : IS<Arith, T>
{
    public TR Evaluate<TR>(ISemantic1<Arith, T, TR> semantic)
        => semantic.Prj().Mul(Left, Right);
}

sealed class ArithComposeSemantic<TS, TI, TR>(IArithSemantic<TS, TI> S, Func<TI, TR> F) : IArithSemantic<TS, TR>
{
    public TR Add(TS l, TS r)
        => F(S.Add(l, r));

    public TR Mul(TS l, TS r)
        => F(S.Mul(l, r));
}

sealed class ArithIdSemantic<T>() : IArithSemantic<T, IS<Arith, T>>
{
    public IS<Arith, T> Add(T l, T r)
        => Arith.Add(l, r);

    public IS<Arith, T> Mul(T l, T r)
        => Arith.Mul(l, r);
}

sealed class ArithMapSemantic<TS, TR>(Func<TS, TR> F) : IArithSemantic<TS, IS<Arith, TR>>
{
    public IS<Arith, TR> Add(TS l, TS r)
        => Arith.Add(F(l), F(r));

    public IS<Arith, TR> Mul(TS l, TS r)
        => Arith.Mul(F(l), F(r));
}

sealed class ArithShowFolder : IArithSemantic<string, string>
{
    string IArithSemantic<string, string>.Add(string l, string r)
        => $"({l} + {r})";

    string IArithSemantic<string, string>.Mul(string l, string r)
        => $"({l} * {r})";
}

sealed class ArithEvalFolder : IArithSemantic<SigEvalData, SigEvalData>
{
    public SigEvalData Add(SigEvalData l, SigEvalData r)
        => from l_ in l
           from r_ in r
           select (l_, r_) switch
           {
               (SigInt left, SigInt right) => new SigInt(left.Value + right.Value),
               _ => throw new InvalidOperationException("Addition requires two integers")
           };

    public SigEvalData Mul(SigEvalData l, SigEvalData r)
        => from l_ in l
           from r_ in r
           select (l_, r_) switch
           {
               (SigInt left, SigInt right) => new SigInt(left.Value * right.Value),
               _ => throw new InvalidOperationException("Addition requires two integers")
           };
}
