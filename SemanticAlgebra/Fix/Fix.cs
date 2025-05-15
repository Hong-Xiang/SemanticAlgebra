using SemanticAlgebra.Control;
using SemanticAlgebra.Data;

namespace SemanticAlgebra.Fix;

public sealed record class Fix<F>(IS<F, Fix<F>> Unfix)
    where F : IFunctor<F>
{
    public T Fold<W, T>(Folder<F, W, T> folder)
        where W : ICoMonad<W>
        => folder.Fold(this);
}

public static class FixExtension
{
    public static Fix<F> Fix<F>(this IS<F, Fix<F>> e)
        where F : IFunctor<F>
        => new(e);
}

public sealed record class Folder<F, W, T>(
    IDistributive<F, W> Distribute,
    ISemantic1<F, IS<W, T>, T> Semantic
)
    where F : IFunctor<F>
    where W : ICoMonad<W>
{
    public T Fold(Fix<F> e)
    {
        return Step(e).Evaluate(W.Extract<T>());
        //return m.Evaluate(Semantic);

    }

    IS<W, T> Step(Fix<F> value)
    {

        var wnested = value.Unfix.Select(fx => Step(fx).Evaluate(W.Duplicate<T>()));
        var swap = wnested.Evaluate(Distribute.Distribute<IS<W, T>>());
        var folded = swap.Select(fwt => fwt.Evaluate(Semantic));
        return folded;

        //IDiSemantic<F, Fix<F>, IS<W, IS<W, T>>> mapS =
        //    F.Map<Fix<F>, IS<W, IS<W, T>>>(fx =>
        //    {
        //        var r = fx.Unfix.Select(Step);
        //        return r;
        //    });
        //IS<F, IS<W, IS<W, T>>> v = value.Unfix.Evaluate(mapS.Semantic);
        //return v.Evaluate(Distribute.Distribute<IS<W, T>>());
    }
}
