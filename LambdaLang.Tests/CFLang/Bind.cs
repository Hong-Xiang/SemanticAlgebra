using SemanticAlgebra;
using SemanticAlgebra.Control;
using SemanticAlgebra.Data;
using SemanticAlgebra.Syntax;

namespace LambdaLang.Tests.CFLang;

public partial interface Bind
    : IFunctor<Bind>
    , IImplementsM<Bind, ShowState, string>
{
    [Semantic1]
    public interface ISemantic<in TS, out TR> : ISemantic1<Bind, TS, TR>
    {
        TR SeqV(TS step, TS next);
        TR LetV(Value name, TS value, TS next);
        TR LetL(Label name, TS value, TS next);
        TR Val(Value name);
        TR Reg(Label name);
    }

    static
          ISemantic1<Bind,
              IS<M, string>,
              IS<M, string>>
          IImplementsM<Bind, ShowState, string>.GetS<M>()
          => new ShowSemantic<M>();

    sealed class ShowSemantic<M>
        : ISemantic<IS<M, string>, IS<M, string>>
        where M : IMonadState<M, ShowState>
    {
        public IS<M, string> LetL(Label name, IS<M, string> value, IS<M, string> next)
        {
            throw new NotImplementedException();
        }

        public IS<M, string> LetV(Value name, IS<M, string> value, IS<M, string> next)
        {
            throw new NotImplementedException();
        }

        public IS<M, string> Reg(Label name)
        {
            throw new NotImplementedException();
        }

        public IS<M, string> SeqV(IS<M, string> step, IS<M, string> next)
        {
            throw new NotImplementedException();
        }

        public IS<M, string> Val(Value name)
        {
            throw new NotImplementedException();
        }
    }

}
