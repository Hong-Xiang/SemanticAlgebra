using SemanticAlgebra;
using SemanticAlgebra.Data;
using SemanticAlgebra.Syntax;

namespace LambdaLang.Tests.CFLang;

public partial interface Ref : IFunctor<Ref>
        , IImplementsM<Ref, ShowState, string>
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

    static ISemantic1<Ref,
                 IS<M, string>,
                 IS<M, string>>
             IImplementsM<Ref, ShowState, string>.GetS<M>()
             => new ShowSemantic<M>();

    sealed class ShowSemantic<M>
        : ISemantic<IS<M, string>, IS<M, string>>
        where M : IMonadState<M, ShowState>
    {
        public static ISemantic1<Ref, TS, TR> Compose<TS, TI, TR>(ISemantic1<Ref, TS, TI> s, Func<TI, TR> f)
        {
            throw new NotImplementedException();
        }

        public static ISemantic1<Ref, IS<M1, string>, IS<M1, string>> GetS<M1>() where M1 : IMonadState<M1, ShowState>
        {
            throw new NotImplementedException();
        }

        public static ISemantic1<Ref, T, IS<Ref, T>> Id<T>()
        {
            throw new NotImplementedException();
        }

        public static ISemantic1<Ref, TS, IS<Ref, TR>> MapS<TS, TR>(Func<TS, TR> f)
        {
            throw new NotImplementedException();
        }

        public IS<M, string> LdArg(Value name)
        {
            throw new NotImplementedException();
        }

        public IS<M, string> LdLoc(Value name)
        {
            throw new NotImplementedException();
        }

        public IS<M, string> StArg(Value name, IS<M, string> value)
            => throw new NotImplementedException();



        public IS<M, string> StLoc(Value name, IS<M, string> value)
        {
            throw new NotImplementedException();
        }
    }

}
