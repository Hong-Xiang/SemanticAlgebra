using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticAlgebra;

/// <summary>
/// CoSemantic encodes s -> f r
/// </summary>
public interface ICoSemantic1<out TF, in TS, out TR>
    where TF : IKind1<TF>
{
    TO CoEvaluate<TSemantic, TO>(TS x, TSemantic semantic)
        where TSemantic : ISemantic1<TF, TR, TO>;
}

sealed class FuncCoSemantic<TF, TS, TR>(Func<TS, IS<TF, TR>> F) : ICoSemantic1<TF, TS, TR>
    where TF : IKind1<TF>
{
    public TO CoEvaluate<TSemantic, TO>(TS x, TSemantic semantic)
        where TSemantic : ISemantic1<TF, TR, TO>
        => F(x).Evaluate<TSemantic, TO>(semantic);
}
