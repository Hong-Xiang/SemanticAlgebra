using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SemanticAlgebra.Data;
using SemanticAlgebra;

namespace LambdaLang.Language;

interface Lit : IFunctor<Lit>
{
    public static IS<Lit, T> LitI<T>(int value) => new LitI<T>(value);

    static ISemantic1<Lit, TS, TR> IKind1<Lit>.Compose<TS, TI, TR>(ISemantic1<Lit, TS, TI> s, Func<TI, TR> f)
        => new LitComposeSemantic<TS, TI, TR>(s.Prj(), f);

    static ISemantic1<Lit, T, IS<Lit, T>> IKind1<Lit>.Id<T>()
        => new LitIdSemantic<T>();

    static ISemantic1<Lit, TS, IS<Lit, TR>> IFunctor<Lit>.MapS<TS, TR>(Func<TS, TR> f)
        => new LitMapSemantic<TS, TR>();
}

interface ILitSemantic<in TS, out TR> : ISemantic1<Lit, TS, TR>
{
    TR LitI(int value);
}

static class LitExtension
{
    public static ILitSemantic<TS, TR> Prj<TS, TR>(this ISemantic1<Lit, TS, TR> s)
        => (ILitSemantic<TS, TR>)s;
}

sealed record class LitI<T>(int Value)
    : IS<Lit, T>
{
    public TR Evaluate<TR>(ISemantic1<Lit, T, TR> semantic)
        => semantic.Prj().LitI(Value);
}

sealed class LitComposeSemantic<TS, TI, TR>(ILitSemantic<TS, TI> S, Func<TI, TR> F) : ILitSemantic<TS, TR>
{
    public TR LitI(int value)
        => F(S.LitI(value));
}

sealed class LitIdSemantic<T>() : ILitSemantic<T, IS<Lit, T>>
{
    public IS<Lit, T> LitI(int value)
        => Lit.LitI<T>(value);
}

sealed class LitMapSemantic<TS, TR> : ILitSemantic<TS, IS<Lit, TR>>
{
    public IS<Lit, TR> LitI(int value)
        => Lit.LitI<TR>(value);
}

sealed class LitShowFolder : ILitSemantic<string, string>
{
    string ILitSemantic<string, string>.LitI(int value)
        => $"{value}";
}

sealed class LitEvalFolder : ILitSemantic<SigEvalData, SigEvalData>
{
    public SigEvalData LitI(int value)
        => SigEvalState.From(s => (s, new SigInt(value)));
}
