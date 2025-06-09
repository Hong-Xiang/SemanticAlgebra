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
          IImplementsM<Bind, ShowState, string>.Get<M>()
          => new ShowSemantic<M>();

    sealed class ShowSemantic<M>
        : ISemantic<IS<StateT<M, ShowState>, string>, IS<StateT<M, ShowState>, string>>
        where M : IMonadState<M>
    {
        public IS<StateT<M, ShowState>, string> LetL(Label name, IS<StateT<M, ShowState>, string> value, IS<StateT<M, ShowState>, string> next)
        {
            throw new NotImplementedException();
        }

        public IS<StateT<M, ShowState>, string> LetV(Value name, IS<StateT<M, ShowState>, string> value, IS<StateT<M, ShowState>, string> next)
        {
            throw new NotImplementedException();
        }

        public IS<StateT<M, ShowState>, string> Reg(Label name)
        {
            throw new NotImplementedException();
        }

        public IS<StateT<M, ShowState>, string> SeqV(IS<StateT<M, ShowState>, string> step, IS<StateT<M, ShowState>, string> next)
        {
            throw new NotImplementedException();
        }

        public IS<StateT<M, ShowState>, string> Val(Value name)
        {
            throw new NotImplementedException();
        }
    }

}
