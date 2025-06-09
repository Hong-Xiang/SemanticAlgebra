using SemanticAlgebra;
using SemanticAlgebra.Control;
using SemanticAlgebra.Data;
using SemanticAlgebra.Syntax;

namespace LambdaLang.Tests.CFLang;

public partial interface Lit
    : IFunctor<Lit>
    , IImplementsM<Lit, ShowState, string>
{
    [Semantic1]
    public interface ISemantic<in TS, out TR>
        : ISemantic1<Lit, TS, TR>
    {
        TR LitI(int value);
    }

    static
        ISemantic1<Lit,
            IS<M, string>,
            IS<M, string>>
        IImplementsM<Lit, ShowState, string>.Get_<M>()
        => new ShowSemantic<M>();

    sealed class ShowSemantic<M>
        : ISemantic<IS<M, string>, IS<M, string>>
        where M : IMonadState<M, ShowState>
    {
        public IS<M, string> LitI(int value)
            => M.Pure($"lit {value}");
    }
}

