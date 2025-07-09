using SemanticAlgebra;
using SemanticAlgebra.Free;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LambdaLang.Tests.CFLang;


// expr = Free<Arith, TV>
// stmt = Free<Stmt<TV, TE>, T>
public class ScfLang<TV>
{
    public IS<
        Free<Stmt<TV, IS<Free<Atom>, TV>>>,
        IS<Free<Atom>, TV>> Bind(TV val, IS<Free<Atom>, TV> expr)
    {
        return Stmt<TV, IS<Free<Atom>, TV>>.B.Bind(
            val, expr,
            Free<Atom>.B.Pure(val)
        ).LiftF();
    }

    public IS<Free<Atom>, TV> LitI(int value)
        => ((IS<Atom, TV>)Lit.B.LitI<TV>(value)).LiftF();
    public IS<Free<Atom>, TV> Add(IS<Free<Atom>, TV> l, IS<Free<Atom>, TV> r)
        => Free<Atom>.B.Roll(Arith.B.Add(l, r));
}

public static class ScfLang
{
    public static IS<Free<Stmt<TR, TE>>, TT> SelectVal<TV, TE, TT, TR>(
        this IS<Free<Stmt<TV, TE>>, TT> e,
        Func<TV, TR> f
    )
        => e.Select1(Stmt<TV, TE>.ValTransform(f));

    public static IS<Free<Stmt<TV, TR>>, TT> SelectExp<TV, TE, TT, TR>(
        this IS<Free<Stmt<TV, TE>>, TT> e,
        Func<TE, TR> f
    )
        => e.Select1(Stmt<TV, TE>.ExpTransform(f));
}
