using SemanticAlgebra;
using SemanticAlgebra.Data;
using SemanticAlgebra.Free;
using SemanticAlgebra.Syntax;

namespace LambdaLang.Tests.CFLang;
public partial interface Arith
    : IFunctor<Arith>
    , IImplementsM<Arith, ShowState, string>
{
    [Semantic1]
    public interface ISemantic<in TS, out TR>
        : ISemantic1<Arith, TS, TR>
    {
        TR Add(TS a, TS b);
        TR Clt(TS a, TS b);
        TR Ceq(TS a, TS b);
    }

    static
          ISemantic1<Arith,
              IS<M, string>,
              IS<M, string>>
          IImplementsM<Arith, ShowState, string>.GetS<M>()
          => new ShowSemantic<M>();

    sealed class ShowSemantic<M>
        : ISemantic<IS<M, string>, IS<M, string>>
        where M : IMonadState<M, ShowState>
    {
        public IS<M, string> Add(IS<M, string> a, IS<M, string> b)
            => from va in a
               from vb in b
               from s in M.Get()
               select s.Line($"({va} + {vb})");

        public IS<M, string> Ceq(IS<M, string> a, IS<M, string> b)
            => from va in a
               from vb in b
               from s in M.Get()
               select s.Line($"({va}) == ({vb})");

        public IS<M, string> Clt(IS<M, string> a, IS<M, string> b)
             => from va in a
                from vb in b
                from s in M.Get()
                select s.Line($"({va}) < ({vb})");
    }
}

public sealed class ArithShowF
    : Arith.ISemantic<
        IS<Free<ShowF>, string>,
        IS<Free<ShowF>, string>
        >
{
    public IS<Free<ShowF>, string> Add(IS<Free<ShowF>, string> a, IS<Free<ShowF>, string> b)
        => from sa in a
           from sb in b
           select $"({sa}) + ({sb})";


    public IS<Free<ShowF>, string> Ceq(IS<Free<ShowF>, string> a, IS<Free<ShowF>, string> b)
        => from sa in a
           from sb in b
           select $"({sa}) == ({sb})";
    public IS<Free<ShowF>, string> Clt(IS<Free<ShowF>, string> a, IS<Free<ShowF>, string> b)
        => from sa in a
           from sb in b
           select $"({sa}) < ({sb})";
}