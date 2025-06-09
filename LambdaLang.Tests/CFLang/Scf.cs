using SemanticAlgebra;
using SemanticAlgebra.Control;
using SemanticAlgebra.Data;
using SemanticAlgebra.Syntax;

namespace LambdaLang.Tests.CFLang;

public partial interface Scf : IFunctor<Scf>
    , IImplementsM<Scf, ShowState, string>
{
    [Semantic1]
    public partial interface ISemantic<in TS, out TR>
        : ISemantic1<Scf, TS, TR>
    {
        TR Block(TS body);
        TR Loop(TS body);
    }

    static ISemantic1<Scf,
               IS<M, string>,
               IS<M, string>>
           IImplementsM<Scf, ShowState, string>.Get_<M>()
           => new ShowSemantic<M>();

    sealed class ShowSemantic<M>
        : ISemantic<IS<M, string>, IS<M, string>>
        where M : IMonadState<M, ShowState>
    {
        public IS<M, string> Block(IS<M, string> body)
            => from st in M.Get()
               let head = st.Line("block():")
               from _i in M.Modify(static s => s.Indent())
               from b in body
               from _o in M.Modify(static s => s.Unindent())
               select head + b;

        public IS<M, string> Loop(IS<M, string> body)
        {
            throw new NotImplementedException();
        }
    }

}
