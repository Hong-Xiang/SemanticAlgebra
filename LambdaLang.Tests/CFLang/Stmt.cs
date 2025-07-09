using SemanticAlgebra;
using SemanticAlgebra.Data;
using SemanticAlgebra.Free;
using SemanticAlgebra.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LambdaLang.Tests.CFLang;

public sealed record class Loc(string Name) { }
public sealed record class Arg(string Name) { }

public abstract partial class Stmt<TV, TE>
    : IFunctor<Stmt<TV, TE>>
{

    [Semantic1]
    public interface ISemantic<in TS, out TR>
        : ISemantic1<Stmt<TV, TE>, TS, TR>
    {
        TR Bind(TV val, TE expr, TS next);
        TR GetL(TV val, Loc loc, TS next);
        TR GetA(TV val, Arg arg, TS next);
        TR SetL(Loc loc, TV val, TS next);
        TR SetA(Arg arg, TV val, TS next);
    }

    sealed class ValNaturalTransform<TVR>(Func<TV, TVR> F)
        : INaturalTransform<
            Stmt<TV, TE>,
            Stmt<TVR, TE>>
    {
        sealed class Semantic<T>(Func<TV, TVR> F) : ISemantic<T, IS<Stmt<TVR, TE>, T>>
        {
            public IS<Stmt<TVR, TE>, T> Bind(TV val, TE expr, T next)
                => Stmt<TVR, TE>.B.Bind(F(val), expr, next);

            public IS<Stmt<TVR, TE>, T> GetA(TV val, Arg arg, T next)
                => Stmt<TVR, TE>.B.GetA(F(val), arg, next);

            public IS<Stmt<TVR, TE>, T> GetL(TV val, Loc loc, T next)
                => Stmt<TVR, TE>.B.GetL(F(val), loc, next);


            public IS<Stmt<TVR, TE>, T> SetA(Arg arg, TV val, T next)
                => Stmt<TVR, TE>.B.SetA(arg, F(val), next);


            public IS<Stmt<TVR, TE>, T> SetL(Loc loc, TV val, T next)
                => Stmt<TVR, TE>.B.SetL(loc, F(val), next);
        }

        public IS<Stmt<TVR, TE>, T> Invoke<T>(IS<Stmt<TV, TE>, T> e)
            => e.Evaluate(new Semantic<T>(F));
    }

    sealed class ExpNaturalTransform<TR>(Func<TE, TR> F)
        : INaturalTransform<
            Stmt<TV, TE>,
            Stmt<TV, TR>>
    {
        sealed class Semantic<T>(Func<TE, TR> F) : ISemantic<T, IS<Stmt<TV, TR>, T>>
        {
            public IS<Stmt<TV, TR>, T> Bind(TV val, TE expr, T next)
                => Stmt<TV, TR>.B.Bind(val, F(expr), next);

            public IS<Stmt<TV, TR>, T> GetA(TV val, Arg arg, T next)
                => Stmt<TV, TR>.B.GetA(val, arg, next);

            public IS<Stmt<TV, TR>, T> GetL(TV val, Loc loc, T next)
                => Stmt<TV, TR>.B.GetL(val, loc, next);


            public IS<Stmt<TV, TR>, T> SetA(Arg arg, TV val, T next)
                => Stmt<TV, TR>.B.SetA(arg, val, next);


            public IS<Stmt<TV, TR>, T> SetL(Loc loc, TV val, T next)
                => Stmt<TV, TR>.B.SetL(loc, val, next);
        }

        public IS<Stmt<TV, TR>, T> Invoke<T>(IS<Stmt<TV, TE>, T> e)
            => e.Evaluate(new Semantic<T>(F));
    }

    public static INaturalTransform<Stmt<TV, TE>, Stmt<TVR, TE>> ValTransform<TVR>(
        Func<TV, TVR> f
    ) => new ValNaturalTransform<TVR>(f);
    public static INaturalTransform<Stmt<TV, TE>, Stmt<TV, TR>> ExpTransform<TR>(
        Func<TE, TR> f
    ) => new ExpNaturalTransform<TR>(f);

}



public sealed class StmtShowSemantic()
    : Stmt<string, string>.ISemantic<string, string>
{
    public string Bind(string val, string expr, string next)
        => $"let {val} = {expr};{Environment.NewLine}{next}";

    public string GetA(string val, Arg loc, string next)
        => $"{val} = $A{loc.Name};{Environment.NewLine}{next}";

    public string GetL(string val, Loc loc, string next)
        => $"{val} = $L{loc.Name};{Environment.NewLine}{next}";

    public string SetA(Arg loc, string val, string next)
        => $"$L({loc.Name}) = {val};{Environment.NewLine}{next}";

    public string SetL(Loc loc, string val, string next)
        => $"$A({loc.Name}) = {val};{Environment.NewLine}{next}";
}

public static class StmtExtension
{
    public static string Show(this IS<Free<Stmt<string, string>>, string> stmt)
        => stmt.Evaluate(new StmtShowSemantic().LiftF());
}

