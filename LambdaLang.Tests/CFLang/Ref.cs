using SemanticAlgebra;
using SemanticAlgebra.Data;
using SemanticAlgebra.Syntax;

namespace LambdaLang.Tests.CFLang;

public partial interface Ref : IFunctor<Ref>
{
    [Semantic1]
    public interface ISemantic<in TS, out TR>
        : ISemantic1<Ref, TS, TR>
    {
        TR LdLoc(Value name);
        TR StLoc(Value name, TS value);
        TR LdArg(Value name);
        TR StArg(Value name, TS value);
    }
}
