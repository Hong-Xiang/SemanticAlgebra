using SemanticAlgebra;
using SemanticAlgebra.Data;
using SemanticAlgebra.Syntax;

namespace LambdaLang.Tests.CFLang;

public abstract partial class Stk : IFunctor<Stk>
{
    [Semantic1]
    public partial interface ISemantic<in TS, out TR>
        : ISemantic1<Stk, TS, TR>
    {
        TR Pop(TS next);
        TR Push(TS value, TS next);
    }
}
