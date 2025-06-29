using SemanticAlgebra.Control;

namespace SemanticAlgebra.Data;

public abstract partial class Cofree<F> : IComonad<Cofree<F>>
    where F : IFunctor<F>
{
    public static ISemantic1<Cofree<F>, TS, TR> Compose<TS, TI, TR>(ISemantic1<Cofree<F>, TS, TI> s, Func<TI, TR> f)
        => IAlias<TS>.Compose(s, f);

    public static ISemantic1<Cofree<F>, TS, IS<Cofree<F>, TR>> ExtendS<TS, TR>(ISemantic1<Cofree<F>, TS, TR> f)
        => new ExtendSemantic<TS, TR>(f);

    sealed class ExtendSemantic<TS, TR>(ISemantic1<Cofree<F>, TS, TR> f) : IAlias<TS>.ISemantic<IS<Cofree<F>, TR>>
    {
        public IS<Cofree<F>, TR> From((TS Head, IS<F, IS<Cofree<F>, TS>> Tail) value)
        {
            var hd = B.From(value.Head, value.Tail).Evaluate(f);
            IS<F, IS<Cofree<F>, TR>> tl = value.Tail.Select(v => v.Evaluate(this));
            return B.From(hd, tl);
        }
    }

    public static ISemantic1<Cofree<F>, T, T> ExtractS<T>()
        => IAlias<T>.Semantic(value => value.Head);

    public static ISemantic1<Cofree<F>, T, IS<Cofree<F>, T>> Id<T>()
        => IAlias<T>.Id();

    public static ISemantic1<Cofree<F>, TS, IS<Cofree<F>, TR>> MapS<TS, TR>(Func<TS, TR> f)
        => new MapSemantic<TS, TR>(f);

    public interface IAlias<TS> : Alias1.ISpec<Cofree<F>, TS, (TS Head, IS<F, IS<Cofree<F>, TS>> Tail)>
    {
    }

    sealed class MapSemantic<TS, TR>(Func<TS, TR> Func) : IAlias<TS>.ISemantic<IS<Cofree<F>, TR>>
    {
        public IS<Cofree<F>, TR> From((TS Head, IS<F, IS<Cofree<F>, TS>> Tail) value)
            => B.From(Func(value.Head), value.Tail.Select(v => v.Select(Func)));
    }

    public static class B
    {
        public static IS<Cofree<F>, T> From<T>(T head, IS<F, IS<Cofree<F>, T>> tail)
           => IAlias<T>.From((head, tail));
    }


    //sealed class DistributeTransformImpl<G>
    //    : IDistributeTransform<G, Cofree<F>>
    //    where G : IFunctor<G>
    //{
    //    public IS<Cofree<F>, IS<G, T>> Distribute<T>(IS<G, IS<Cofree<F>, T>> fg)
    //    {
    //        IS<G, T> hd = fg.Select(v => v.Unwrap().Head);
    //        IS<F, IS<Cofree<F>, IS<G, T>>> tl = null;
    //        return B.From(hd, tl);
    //    }

    //    sealed class DistributeSemantic<T>
    //        : IAlias<IS<G, T>>.ISemantic<IS<Cofree<F>, IS<G, T>>>
    //    {
    //        public IS<Cofree<F>, IS<G, T>> From((IS<G, T> Head, IS<F, IS<Cofree<F>, IS<G, T>>> Tail) value)
    //        {
    //            throw new NotImplementedException();
    //        }
    //    }
    //}


    //public static IDistributeTransform<G, Cofree<F>> DistributeTransform<G>()
    //    where G : IFunctor<G>
    //    => throw new NotImplementedException();
}

public static partial class CofreeExtensions
{
    public static (TS Head, IS<F, IS<Cofree<F>, TS>> Tail) Unwrap<F, TS>(this IS<Cofree<F>, TS> e)
        where F : IFunctor<F>
        => Cofree<F>.IAlias<TS>.Unwrap(e);
}
