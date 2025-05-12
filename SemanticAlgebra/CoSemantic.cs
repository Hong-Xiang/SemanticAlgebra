using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticAlgebra;

/// <summary>
/// CoSemantic encodes i -> f o
/// </summary>
public interface ICoSemantic1<out TF, in TI, out TO>
    where TF : IKind1<TF>
{
    TR CoEvaluate<TSemantic, TR>(TI x, TSemantic semantic)
        where TSemantic : ISemantic1<TF, TO, TR>;
}

public interface IDiSemantic<TF, in TI, out TO>
    : ICoSemantic1<TF, IS<TF, TI>, TO>
    where TF : IKind1<TF>
{
    // Don't directly using subtype ISemantic1
    // to avoid accidentally forget implement concrete semantic 
    ISemantic1<TF, TI, IS<TF, TO>> Forward { get; }
}
