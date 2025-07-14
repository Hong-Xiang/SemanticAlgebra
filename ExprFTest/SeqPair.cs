using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprFTest;

interface ISeqPairSemantic<THead, TTail, TSeq, TO>
{
    TO Single(TTail value);
    TO Concat(THead head, TSeq tail);
}

interface ISeqPair<TR, TP, TB>
{
    TBO Eval<TBO>(ISeqPairSemantic<TR, TP, TB, TBO> semantic);
    ISeqPair<TR, TP, TBO> Select<TBO>(Func<TB, TBO> f);
    ISeqPair<TRR, TPR, TBR> Select<TRR, TPR, TBR>(Func<TR, TRR> f, Func<TP, TPR> g, Func<TB, TBR> h);
}


sealed record class Single<TR, TP, TB>(TP Value)
    : ISeqPair<TR, TP, TB>
{

    public TBO Eval<TBO>(ISeqPairSemantic<TR, TP, TB, TBO> semantic)
        => semantic.Single(Value);

    public ISeqPair<TR, TP, TBO> Select<TBO>(Func<TB, TBO> f)
        => new Single<TR, TP, TBO>(Value);

    public ISeqPair<TRR, TPR, TBR> Select<TRR, TPR, TBR>(Func<TR, TRR> f, Func<TP, TPR> g, Func<TB, TBR> h)
        => new Single<TRR, TPR, TBR>(g(Value));
    public override string ToString()
        => $"[{Value}]";
}

sealed record class Concat<TR, TP, TB>(TR Head, TB Tail)
    : ISeqPair<TR, TP, TB>
{
    public TBO Eval<TBO>(ISeqPairSemantic<TR, TP, TB, TBO> semantic)
        => semantic.Concat(Head, Tail);

    public ISeqPair<TR, TP, TBO> Select<TBO>(Func<TB, TBO> f)
        => new Concat<TR, TP, TBO>(Head, f(Tail));

    public ISeqPair<TRR, TPR, TBR> Select<TRR, TPR, TBR>(Func<TR, TRR> f, Func<TP, TPR> g, Func<TB, TBR> h)
        => new Concat<TRR, TPR, TBR>(f(Head), h(Tail));

    public override string ToString()
        => $"[{Head}; {Tail}]";
}

sealed record class SeqPair<TR, TP>(ISeqPair<TR, TP, SeqPair<TR, TP>> Value)
{
    public override string ToString() => $"{Value}";

    sealed class SelectManySemantic<TPM, TPR>(Func<TP, SeqPair<TR, TPM>> f, Func<TP, TPM, TPR> g)
        : ISeqPairSemantic<TR, TP, SeqPair<TR, TP>, SeqPair<TR, TPR>>
    {

        public SeqPair<TR, TPR> Concat(TR region, SeqPair<TR, TP> value)
            => SeqPair<TR, TPR>.Concat(region, value.SelectMany(f, g));


        public SeqPair<TR, TPR> Single(TP value)
            => f(value).Select(r => r, p => g(value, p));
    }

    public SeqPair<TRR, TPR> Select<TRR, TPR>(Func<TR, TRR> f, Func<TP, TPR> g)
        => Value.Select(f, g, b => b.Select(f, g)).Fix();

    public SeqPair<TR, TPR> SelectMany<TPM, TPR>(Func<TP, SeqPair<TR, TPM>> f, Func<TP, TPM, TPR> g)
        => Value.Eval(new SelectManySemantic<TPM, TPR>(f, g));

    public static SeqPair<TR, TP> Single(TP value)
        => new Single<TR, TP, SeqPair<TR, TP>>(value).Fix();

    public static SeqPair<TR, TP> Concat(TR head, SeqPair<TR, TP> tail)
        => new Concat<TR, TP, SeqPair<TR, TP>>(head, tail).Fix();

    sealed class FoldSemantic<T>(ISeqPairSemantic<TR, TP, T, T> algebra)
        : ISeqPairSemantic<TR, TP, SeqPair<TR, TP>, T>
    {
        public T Concat(TR head, SeqPair<TR, TP> tail)
            => algebra.Concat(head, tail.Value.Eval(this));

        public T Single(TP value)
            => algebra.Single(value);
    }

    public T Fold<T>(ISeqPairSemantic<TR, TP, T, T> algebra)
        => Value.Eval(new FoldSemantic<T>(algebra));

}

static class SeqPair
{
    public static SeqPair<TR, TP> Fix<TR, TP>(this ISeqPair<TR, TP, SeqPair<TR, TP>> x)
         => new(x);

    public static SeqPair<TR, TP> Single<TR, TP>(TP value)
        => SeqPair<TR, TP>.Single(value);

    public static SeqPair<T, T> SingleH<T>(T value)
        => SeqPair<T, T>.Single(value);


    public static SeqPair<TR, TP> Concat<TR, TP>(TR head, SeqPair<TR, TP> tail)
        => SeqPair<TR, TP>.Concat(head, tail);

    public static SeqPair<TR, TP> ConcatL<TR, TP>(TR head, TP tail)
        => SeqPair<TR, TP>.Concat(head, Single<TR, TP>(tail));
}
