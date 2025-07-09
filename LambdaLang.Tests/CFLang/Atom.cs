using SemanticAlgebra;
using SemanticAlgebra.Data;
using SemanticAlgebra.Free;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LambdaLang.Tests.CFLang;

public partial interface Atom
    : IFunctor<Atom>
    , Lit, Arith
    , IMergedSemantic1<Atom, Lit, Arith>
{
    static ISemantic1<Atom, TS, TR> IMergedSemantic1<Atom, Lit, Arith>.MergeSemantic<TS, TR>(
     ISemantic1<Lit, TS, TR> s1,
     ISemantic1<Arith, TS, TR> s2)
     => CreateMergeSemantic(s1, s2);

    static ISemantic1<Atom, TS, TR> CreateMergeSemantic<TS, TR>(
     ISemantic1<Lit, TS, TR> s1,
     ISemantic1<Arith, TS, TR> s2)
     => new MergedSemantic<TS, TR>(s1.Prj(), s2.Prj());



    public interface ISemantic<in TI, out TO>
        : ISemantic1<Atom, TI, TO>
        , Lit.ISemantic<TI, TO>
        , Arith.ISemantic<TI, TO>
    {
    }

    sealed class MergedSemantic<TI, TO>(
        Lit.ISemantic<TI, TO> Lit,
        Arith.ISemantic<TI, TO> Arith
    ) : ISemantic<TI, TO>
    {
        public TO Add(TI a, TI b)
            => Arith.Add(a, b);

        public TO Ceq(TI a, TI b)
            => Arith.Ceq(a, b);

        public TO Clt(TI a, TI b)
            => Arith.Clt(a, b);

        public TO LitI(int value)
            => Lit.LitI(value);
    }

}

public static class AtomExtension
{
    public static string Show(this IS<Free<Atom>, string> e)
        => e.Evaluate(Atom.CreateMergeSemantic(new Lit.ShowSemantic(), new Arith.ShowSemantic()).LiftF());
}
