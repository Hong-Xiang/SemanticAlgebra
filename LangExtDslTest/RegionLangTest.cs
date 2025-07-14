using LanguageExt;
using LanguageExt.Traits;
using System.Diagnostics.Contracts;
using Xunit.Abstractions;
namespace LangExtDslTest;
using static SyntaxFactory;


//interface PairF<THead>
//    : Functor<PairF<THead>>
//{
//    interface ISemantic<in TI, out TO>
//    {
//        TO Unwrap(THead head, TI next);
//    }

//    public sealed record class Pair<T>(THead Head, T Next) : K<PairF<THead>, T> { }


//    static K<PairF<THead>, B> Functor<PairF<THead>>.Map<A, B>(Func<A, B> f, K<PairF<THead>, A> ma)
//    {
//        var ap = ma.Prj();
//        return new Pair<B>(ap.Head, f(ap.Next));
//    }

//    static TO Match<TI, TO>(K<PairF<THead>, TI> ma, ISemantic<TI, TO> s)
//    {
//        var ap = ma.Prj();
//        return s.Unwrap(ap.Head, ap.Next);
//    }
//}

//static partial class Pair
//{
//    public static PairF<THead>.Pair<T> Prj<THead, T>(this K<PairF<THead>, T> e)
//        => (PairF<THead>.Pair<T>)e;
//    public static PairF<THead>.Pair<T> Create<THead, T>(THead head, T next)
//        => new PairF<THead>.Pair<T>(head, next);

//    public static RegionBody<TRegion, T> Fix<TRegion, T>(this K<Free<PairF<TRegion>>, T> value)
//        => new RegionBody<TRegion, T>(value);
//}


//interface IRegionBodyAlgebra<TRegion, TI, TO>
//{
//    TO Single(TI value);
//    TO Append(TRegion region, TI value);
//}

//sealed record class RegionBody<TRegion, T>(
//    K<Free<PairF<TRegion>>, T> Value
//)
//{
//    T FoldFree(K<PairF<TRegion>, Free<PairF<TRegion>, T>> value, IRegionBodyAlgebra<TRegion, T, T> algebra)
//    {
//        var fr = value.Select(x => x.Fix().Fold(algebra));
//        var pr = fr.Prj();
//        return algebra.Append(pr.Head, pr.Next);
//    }

//    T Fold(IRegionBodyAlgebra<TRegion, T, T> algebra)
//    {
//        return Value switch
//        {
//            Pure<PairF<TRegion>, T> { Value: var v } => algebra.Single(v),
//            Bind<PairF<TRegion>, T> { Value: var v } => FoldFree(v, algebra),
//            _ => throw new NotSupportedException()
//        };
//    }
//}

// pair-r semantic i o {
//     single: i -> o
//     concat: r, i -> o
// }
// region-body-semantic r i o=
// {
//   terminal: i -> o,
//   concat: r, free (pair r) i -> o
// }


// free f a x = a | f x
// free f a = fix (free f a)
// alg a x = a -> x | f x -> x


// region-body r t = free (pair r) t
// region-def b = block b | loop b
// region t r = region-def (region-body r t)
// region t = fix (region t)
// (region t) x -> x

// fold (region t) requires region t - semantic
// region-def (region-body x t) -> x

// block (label, args, region-body r t) -> r 
// loop (label, args, region-body r t) -> r 

// fold region-body r t  (t -> x | pair r x -> x) -> x
// 

// fold region t
// alg x:
//   basic-block t -> x
//   seqence (r, x) -> x
//   block (


interface IRegionSemantic<TLabel, T, TBI, TRI, TBO, TRO>
{
    TBO Pure(T value);
    TBO LetR(TRI region, TBI value);
    TRO Block(TLabel label, TBI body);
    TRO Loop(TLabel label, TBI body);
    TRO Label(TLabel label);
}

interface IRegionBody<TLabel, T, TB, TR>
{
    TBO Eval<TBO, TRO>(IRegionSemantic<TLabel, T, TB, TR, TBO, TRO> semantic);
    IRegionBody<TLabel, T, TBO, TR> Select<TBO>(Func<TB, TBO> f);
}



interface IRegion<TLabel, T, TB, TR>
{
    TLabel Label { get; }
    TRO Eval<TBO, TRO>(IRegionSemantic<TLabel, T, TB, TR, TBO, TRO> semantic);
    IRegion<TLabel, T, TB, TRO> Select<TRO>(Func<TR, TRO> f);
}


sealed record class RegionBody<TLabel, TRD, T>(IRegionBody<TLabel, T, RegionBody<TLabel, TRD, T>, Region<TLabel, TRD>> Value)
{
    public override string ToString()
        => $"[RB]({Value})";
    sealed class SelectManySemantic<TM, TR>(Func<T, RegionBody<TLabel, TRD, TM>> f, Func<T, TM, TR> g)
        : IRegionSemantic<TLabel, T, RegionBody<TLabel, TRD, T>, Region<TLabel, TRD>, RegionBody<TLabel, TRD, TR>, Region<TLabel, TRD>>
    {
        public Region<TLabel, TRD> Block(TLabel label, RegionBody<TLabel, TRD, T> body)
            => throw new NotImplementedException();

        public Region<TLabel, TRD> Label(TLabel label)
            => Region<TLabel, TRD>.Label(label);

        public RegionBody<TLabel, TRD, TR> LetR(Region<TLabel, TRD> region, RegionBody<TLabel, TRD, T> value)
            => RegionBody<TLabel, TRD, TR>.LetR(region, value.SelectMany(f, g));

        public Region<TLabel, TRD> Loop(TLabel label, RegionBody<TLabel, TRD, T> body)
            => throw new NotImplementedException();

        public RegionBody<TLabel, TRD, TR> Pure(T value)
            => f(value).Select(m => g(value, m));
    }

    sealed class SelectSemantic<TR>(Func<T, TR> f)
         : IRegionSemantic<TLabel, T, RegionBody<TLabel, TRD, T>, Region<TLabel, TRD>, RegionBody<TLabel, TRD, TR>, Region<TLabel, TRD>>
    {
        public Region<TLabel, TRD> Block(TLabel label, RegionBody<TLabel, TRD, T> body)
            => throw new NotImplementedException();

        public Region<TLabel, TRD> Label(TLabel label)
            => throw new NotImplementedException();

        public RegionBody<TLabel, TRD, TR> LetR(Region<TLabel, TRD> region, RegionBody<TLabel, TRD, T> value)
            => RegionBody<TLabel, TRD, TR>.LetR(region, value.Select(f));

        public Region<TLabel, TRD> Loop(TLabel label, RegionBody<TLabel, TRD, T> body)
            => throw new NotImplementedException();

        public RegionBody<TLabel, TRD, TR> Pure(T value)
            => RegionBody<TLabel, TRD, TR>.Pure(f(value));
    }


    public RegionBody<TLabel, TRD, TR> Select<TR>(Func<T, TR> f)
        => new RegionBody<TLabel, TRD, TR>(Value.Eval(new SelectSemantic<TR>(f)));

    public RegionBody<TLabel, TRD, TR> SelectMany<TM, TR>(Func<T, RegionBody<TLabel, TRD, TM>> f, Func<T, TM, TR> g)
        => Value.Eval(new SelectManySemantic<TM, TR>(f, g));

    public static RegionBody<TLabel, TRD, T> Pure(T value)
        => new PureB<TLabel, T, RegionBody<TLabel, TRD, T>, Region<TLabel, TRD>>(value).Fix();

    public static RegionBody<TLabel, TRD, T> LetR(Region<TLabel, TRD> region, RegionBody<TLabel, TRD, T> value)
        => new LetR<TLabel, T, RegionBody<TLabel, TRD, T>, Region<TLabel, TRD>>(region, value).Fix();

}
static class ReginoExtensions
{
    public static RegionBody<TLabel, TRD, TB> Fix<TLabel, TRD, TB>(this IRegionBody<TLabel, TB, RegionBody<TLabel, TRD, TB>, Region<TLabel, TRD>> x)
        => new(x);

    public static Region<TLabel, T> Fix<TLabel, T>(this IRegion<TLabel, T, RegionBody<TLabel, T, T>, Region<TLabel, T>> x)
            => new(x);

    public static RegionBody<TLabel, T, Region<TLabel, T>> Bind<TLabel, T>(this Region<TLabel, T> x) =>
        RegionBody<TLabel, T, Region<TLabel, T>>.LetR(x,
            RegionBody<TLabel, T, Region<TLabel, T>>.Pure(Region<TLabel, T>.Label(x.Value.Label)));

}

sealed record class Region<TLabel, T>(IRegion<TLabel, T, RegionBody<TLabel, T, T>, Region<TLabel, T>> Value)
{
    public Region<TLabel, TR> Select<TR>(Func<T, TR> f)
        => throw new NotImplementedException();
    public override string ToString()
        => $"[R]({Value})";
    public static Region<TLabel, T> Block(TLabel label, RegionBody<TLabel, T, T> body) => new BlockR<TLabel, T, RegionBody<TLabel, T, T>, Region<TLabel, T>>(label, body).Fix();
    public static Region<TLabel, T> Loop(TLabel label, RegionBody<TLabel, T, T> body) => new LoopR<TLabel, T, RegionBody<TLabel, T, T>, Region<TLabel, T>>(label, body).Fix();
    public static Region<TLabel, T> Label(TLabel label) => new LabelR<TLabel, T, RegionBody<TLabel, T, T>, Region<TLabel, T>>(label).Fix();

    sealed class FoldSemantic<TBR, TRR>(IRegionSemantic<TLabel, T, TBR, TRR, TBR, TRR> semantic)
        : IRegionSemantic<TLabel, T, RegionBody<TLabel, T, T>, Region<TLabel, T>, TBR, TRR>
    {
        public TRR Block(TLabel label, RegionBody<TLabel, T, T> body)
            => semantic.Block(label, body.Value.Eval(this));

        public TRR Label(TLabel label)
            => semantic.Label(label);

        public TBR LetR(Region<TLabel, T> region, RegionBody<TLabel, T, T> value)
            => semantic.LetR(region.Value.Eval(this), value.Value.Eval(this));

        public TRR Loop(TLabel label, RegionBody<TLabel, T, T> body)
            => semantic.Loop(label, body.Value.Eval(this));

        public TBR Pure(T value)
            => semantic.Pure(value);
    }

    public TR Fold<TBR, TR>(IRegionSemantic<TLabel, T, TBR, TR, TBR, TR> semantic)
        => Value.Eval(new FoldSemantic<TBR, TR>(semantic));
}

sealed record class PureB<TLabel, T, TB, TR>(T Value)
    : IRegionBody<TLabel, T, TB, TR>
{
    public TBO Eval<TBO, TRO>(IRegionSemantic<TLabel, T, TB, TR, TBO, TRO> semantic)
        => semantic.Pure(Value);

    public IRegionBody<TLabel, T, TBO, TR> Select<TBO>(Func<TB, TBO> f)
        => new PureB<TLabel, T, TBO, TR>(Value);
}

sealed record class LetR<TLabel, T, TB, TR>(TR Region, TB Next)
    : IRegionBody<TLabel, T, TB, TR>
{
    public TBO Eval<TBO, TRO>(IRegionSemantic<TLabel, T, TB, TR, TBO, TRO> semantic)
        => semantic.LetR(Region, Next);

    public IRegionBody<TLabel, T, TBO, TR> Select<TBO>(Func<TB, TBO> f)
        => new LetR<TLabel, T, TBO, TR>(Region, f(Next));
}

sealed record class BlockR<TLabel, T, TB, TR>(TLabel Label, TB Body)
    : IRegion<TLabel, T, TB, TR>
{
    public TRO Eval<TBO, TRO>(IRegionSemantic<TLabel, T, TB, TR, TBO, TRO> semantic)
        => semantic.Block(Label, Body);

    public IRegion<TLabel, T, TB, TRO> Select<TRO>(Func<TR, TRO> f)
        => new BlockR<TLabel, T, TB, TRO>(Label, Body);
}

sealed record class LoopR<TLabel, T, TB, TR>(TLabel Label, TB Body)
    : IRegion<TLabel, T, TB, TR>
{
    public TRO Eval<TBO, TRO>(IRegionSemantic<TLabel, T, TB, TR, TBO, TRO> semantic)
        => semantic.Loop(Label, Body);

    public IRegion<TLabel, T, TB, TRO> Select<TRO>(Func<TR, TRO> f)
        => new LoopR<TLabel, T, TB, TRO>(Label, Body);
}

sealed record class LabelR<TLabel, T, TB, TR>(TLabel Label)
    : IRegion<TLabel, T, TB, TR>
{
    public TRO Eval<TBO, TRO>(IRegionSemantic<TLabel, T, TB, TR, TBO, TRO> semantic)
        => semantic.Label(Label);

    public IRegion<TLabel, T, TB, TRO> Select<TRO>(Func<TR, TRO> f)
        => new LabelR<TLabel, T, TB, TRO>(Label);
}



//sealed record class Region<T>(IRegionDefinition<RegionBody<Region<T>, T>> Value)
//{
//    sealed record class Folder<TR>(
//        Func<RegionBody<TR, TR>, TR> FoldBody,
//        Func<T, TR> FoldBaiscBlock
//    )
//    {
//        public TR Run(Region<T> e)
//            => e.Value.Select()
//        }

//    public TR Fold<TR>(Func<RegionBody<TR, TR>, TR> foldBody, Func<T, TR> foldBaiscBlock)
//        => new Folder<TR>(foldBody, foldBaiscBlock).Run(this);
//}

//interface IRegionDefinition<TLable>
//{
//    IRegionDefinition<TR> Select<TR>(Func<T, TR> f);
//}

//sealed record class BlockRegion<TLabel, TValue, T>(
//    TLabel Label,
//    IReadOnlyList<TValue> Args,
//    T Body
//) : IRegionDefinition<T>
//{
//    public IRegionDefinition<TR> Select<TR>(Func<T, TR> f)
//        => new BlockRegion<TLabel, TValue, TR>(
//            Label,
//            Args,
//            f(Body)
//        );
//}

//sealed record class LoopRegion<TLabel, TValue, T>(
//    TLabel Label,
//    IReadOnlyList<TValue> Args,
//    T Body
//) : IRegionDefinition<T>
//{
//    public IRegionDefinition<TR> Select<TR>(Func<T, TR> f)
//        => new LoopRegion<TLabel, TValue, TR>(
//            Label,
//            Args,
//            f(Body)
//        );
//}


static class SyntaxFactory
{
    public static Region<int, string> Block_(int label, RegionBody<int, string, string> body)
        => Region<int, string>.Block(label, body);
    public static Region<int, string> Loop_(int label, RegionBody<int, string, string> body)
        => Region<int, string>.Loop(label, body);
    public static Region<int, string> Label_(int label)
        => Region<int, string>.Label(label);
    public static RegionBody<int, string, string> BasicBlock(string block)
        => RegionBody<int, string, string>.Pure(block);
}

sealed record class ShowSemantic : IRegionSemantic<int, string, IEnumerable<string>, IEnumerable<string>, IEnumerable<string>, IEnumerable<string>>
{
    public IEnumerable<string> Block(int label, IEnumerable<string> body)
        => [$"block ^{label}", .. body.Select(b => $"\t{b}")];

    public IEnumerable<string> Label(int label)
        => [$"^{label}"];

    public IEnumerable<string> LetR(IEnumerable<string> region, IEnumerable<string> value)
        => [.. region, "in", .. value];

    public IEnumerable<string> Loop(int label, IEnumerable<string> body)
        => [$"loop ^{label}", .. body.Select(b => $"\t{b}")];

    public IEnumerable<string> Pure(string value)
        => [value];
}

public class RegionLangTest(ITestOutputHelper Output)
{
    [Fact]
    public void Test1()
    {
        var e = Block_(0, from m in Block_(3, BasicBlock("ret")).Bind()
                          from t in Block_(1, BasicBlock($"br ^{m}")).Bind()
                          from f in Block_(2, BasicBlock($"br ^{m}")).Bind()
                          select $"brif({t}, {f})");
        var code = e.Fold(new ShowSemantic());
        Output.WriteLine(string.Join(Environment.NewLine, code));
        Output.WriteLine(e.ToString());

        //var x = from l1 in Block_(0, BasicBlock("bb0")).Bind()
        //        from l2 in Block_(1, BasicBlock("bb1")).Bind()
        //        select $"br{l1}{l2}";
        var x = Block_(0, BasicBlock("bb0")).Bind().SelectMany(l1 =>
        {
            return Block_(1, BasicBlock("bb1")).Bind();
        }, (l1, l2) => $"brif {l1} - {l2}");
        Output.WriteLine(x.ToString());

    }
}
