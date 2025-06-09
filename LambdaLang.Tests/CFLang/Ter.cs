using SemanticAlgebra;
using SemanticAlgebra.Data;
using SemanticAlgebra.Syntax;

namespace LambdaLang.Tests.CFLang;

public partial interface Ter
    : IFunctor<Ter>
    , IImplementsM<Ter, ShowState, string>
{
    [Semantic1]
    public interface ISemantic<in TS, out TR>
        : ISemantic1<Ter, TS, TR>
    {
        TR Br(TS blockExpr);
        TR BrIf(Value value, TS trueBlock, TS falseBlock);
        TR Return();
        TR ReturnValue(TS val);
    }

  static ISemantic1<Ter,
               IS<M, string>,
               IS<M, string>>
           IImplementsM<Ter, ShowState, string>.Get_<M>()
           => new ShowSemantic<M>();

    sealed class ShowSemantic<M>
        : ISemantic<IS<M, string>, IS<M, string>>
        where M : IMonadState<M, ShowState>
    {
        public IS<M, string> Br(IS<M, string> blockExpr)
        {
            throw new NotImplementedException();
        }

        public IS<M, string> BrIf(Value value, IS<M, string> trueBlock, IS<M, string> falseBlock)
        {
            throw new NotImplementedException();
        }

        public IS<M, string> Return()
            => from s in M.Get()
               select s.Line("ret");

        public IS<M, string> ReturnValue(IS<M, string> val)
            => from s in M.Get()
               from v in val
               select s.Line($"ret {v}");
    }
}
