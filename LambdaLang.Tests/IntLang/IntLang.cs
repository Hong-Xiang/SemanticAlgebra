using SemanticAlgebra;
using SemanticAlgebra.Data;
using SemanticAlgebra.Fix;
using SemanticAlgebra.Syntax;

namespace LambdaLang.Tests.IntLang;

public sealed partial class IntLang : IFunctor<IntLang>
{
    [Semantic1]
    public interface ISemantic<in TS, out TR> : ISemantic1<IntLang, TS, TR>
    {
        TR LitI(int value);
        TR Neg(TS e);
        TR Add(TS a, TS b);
    }


    public static ISemantic<Fix<IntLang>, Fix<IntLang>> SyntaxFactory => Fix<IntLang>.SyntaxFactory.Prj();
}

sealed class IntFolder : IntLang.ISemantic<int, int>
{
    public int LitI(int value) => value;
    public int Neg(int e) => -e;
    public int Add(int a, int b) => a + b;
}

sealed class PushNegTransform :
    IntLang.ISemantic<Fix<IntLang>, IS<IntLang, IS<Either<Fix<IntLang>>, Fix<IntLang>>>>

{
    public IS<IntLang, IS<Either<Fix<IntLang>>, Fix<IntLang>>> LitI(int value)
        => IntLang.B.LitI<IS<Either<Fix<IntLang>>, Fix<IntLang>>>(value);

    public IS<IntLang, IS<Either<Fix<IntLang>>, Fix<IntLang>>> Neg(Fix<IntLang> e)
        => e.Unfix switch
        {
            IntLang.D.Neg<Fix<IntLang>> { e: var x }
                => x.Unfix.Select(static e => e.UnfoldRecursive()),
            IntLang.D.Add<Fix<IntLang>>
            {
                a: var e1,
                b: var e2
            } => IntLang.B.Add(
                IntLang.B.Neg(e1).Fix().UnfoldRecursive(),
                IntLang.B.Neg(e2).Fix().UnfoldRecursive()),
            _ => IntLang.B.Neg(e.UnfoldRecursive())
        };


    public IS<IntLang, IS<Either<Fix<IntLang>>, Fix<IntLang>>> Add(Fix<IntLang> a, Fix<IntLang> b)
        => IntLang.B.Add(a.UnfoldRecursive(), b.UnfoldRecursive());
}