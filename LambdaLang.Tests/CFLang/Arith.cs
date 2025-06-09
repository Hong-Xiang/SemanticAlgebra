using SemanticAlgebra;
using SemanticAlgebra.Data;
using SemanticAlgebra.Syntax;

namespace LambdaLang.Tests.CFLang
{
    public partial interface Arith : IFunctor<Arith>
    {
        [Semantic1]
        public interface ISemantic<in TS, out TR>
            : ISemantic1<Arith, TS, TR>
        {
            TR Add(TS a, TS b);
            TR Clt(TS a, TS b);
        }
    }
}