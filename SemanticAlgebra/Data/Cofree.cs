using SemanticAlgebra.Control;
using SemanticAlgebra.Fix;

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



    public static Func<IS<Cofree<F>, TA>, TR> Folder<TA, TR>(
        IDistributeTransform<F, Pair<TA>> dist,
        ISemantic1<F, (TA Attr, TR Value), TR> folder)
        => new Recurse<F, Identity, Pair<TA>, IS<Cofree<F>, TA>, TR>(
            Identity.DistributeTransformFromIdentity<F>(),
            x => x.Unwrap().Tail.Select(Identity.B.From),
            dist,
            fp => fp.Select(x => x.Unwrap()).Evaluate(folder)
           ).Run;

    // w would be (a, *), which is comonad, extract is snd, and duplicate is (a, (a, *))
    // s is IS<Cofree<F>, a>, r is tr
    // then dist is forall x. f (a, x) -> (a, f x)
    // now if we use m = identity
    // coalg : cofree f a -> f (cofree f a) = tail
    // codist is just identity's distribute
}

public static partial class CofreeExtensions
{
    public static T Attr<F, T>(this IS<Cofree<F>, T> e)
        where F : IFunctor<F>
        => e.Unwrap().Head;
    public static IS<F, IS<Cofree<F>, T>> Expr<F, T>(this IS<Cofree<F>, T> e)
        where F : IFunctor<F>
        => e.Unwrap().Tail;
    public static IS<Cofree<F>, T> AddAttr<F, T>(this IS<F, IS<Cofree<F>, T>> e, T attr)
        where F : IFunctor<F>
        => Cofree<F>.B.From(attr, e);
}

// CofreeF f a b = (a, f b)

// alg of (CofreeF f a) = CofreeF f a r -> r
//                      = (a, f r) -> r

// Cofree f a = (a, f (Cofree f a))
//          t = (a, f t)
// Fix (CofreeF f a)
//          t = (a, f t) 


// unfold (t -> f t) -> t -> fix f
// let t = fix f
// unfold (fix f -> g (fix f)) -> fix f -> fig g

// let t = (a, fix f)
// unfold ((a, fix f) -> (Cofree<F> a, fix f)) -> fix f -> fix g

// unfold fix f -> (a -> (a, fix f))


// coalg : (a, fix f) -> (a, f (a, fix f))
//                       CoFreeF f a (a, fix f)

// m = identity
// s -> f s : fix f -> f (fix f) = fix
// disg  : forall x. f (w x) -> w (f x)
// coalg : f (w r) -> r
// s : fix f, r : Cofree f a (Fix (CofreeF f a))
// w may not be Cofree f
// input is (a, fix f) or on level open: f (a, fix f)
// since r is Cofree f a, f (w (Cofree f a)) = f (a, fix f)
// (w (Cofree f a)) = (a, fix f)
// 

// (f (Cofree f a) -> a) -> fix f -> a
// if a is (Cofree f a), then coalg is f (Cofree f (Cofree f a)) -> Cofree f a 

// thus given a f (annotated ast) -> annotated ast with one more layer

// for the top-down, we want signature to be 
// annotate ((a, fix f) -> (a, f (a, fix f))) -> (a, fix f) -> fix (CofreeF f a)
//                                                             or Cofree f a

// let af = (a, fix f)
// annotate = (af -> (a, f af)) -> af -> Cofree f a

// if we define p f a x = (a, f x), is functor/comonad over x
// af = (a, fix f) = (a, f (fix f)) = p f a (fix f)
// annotate = af -> p f a af -> af -> Cofree f a
// thus the algebra functor is g = p f a
// cofree f a is just fix of p

// let try it by simple IntLang, thus ExprF a = lit int | add a a

// and 2 simple exprssions 
// e1 = lit 42
// e2 = add (lit 1) (add (lit 2) (lit 3))

// expected output
// 0 <: lit 42
// 0 <: add (1 <: lit 1) (1 <: add (2 <: lit 2) (2 <: lit 3))

// annotate step : (a, f (fix f)) -> (a, f (a, fix f))
// step (a, fe) = (a, fe.Select(x => (a + 1, x)))

// for e1, step's result is 0 <: (1 <: lit 42) : (a, (a, f (fix f)))
// for e2, step's result is 0 <: (1 <: add (lit 1) (add (lit 2) (lit 3)))

// (a, fix f) -> f (a, fix f)

// (a, fix f) -> fix (p f a), where p f a x = (a, f x)

// f (fix (p f a)) -> fix p
// f ((p f a) (fix p f a)) -> (p f a) (fix p f a)
// f (a, f (fix p f a)) -> (a, f (fix p f a))
// let t = (fix p f a)
// f (a, f t) -> (a, f t)
// let h = p f a
// f (h t) -> h t


//    (a, fix f) 
// -> (a, f (a, fix f))
// -> (a, f (a, f (a, fix f)))           

// let t = (a, fix f)
//    t
// -> (a, f t)
// -> (a, f (a, f t)) ...
// -> (a, f (a, f (a, f (a, f t)))) ...

// let p f a x = (a, f x)
// then t = (a, fix f) ~ (a, f (fix f)) = p f a (fix f)
// let h = p f a, t = h (fix f), (a, f t) = p f a t = h t
//    t
// -> h t
// -> h (h t) ...

// t -> h t, h = p f a
// t is (a, fix f) ~ (a, f (fix f)) = h (fix f)

// map f' (h t) = map f' (a, f t) over t, 

// given h (fix f) -> h (h (fix f)), we have fix f -> fix h

// seed is (fix f) -> h (fix f)





// cofreef f a b = (a, f b)
// fix (cofree f a) = (a, f (fix (cofree f a)))

public partial class CofreeF<F> : IFunctor2<CofreeF<F>>
    where F : IFunctor<F>
{
    public static class D
    {
        public sealed record class Data<TA, TB>(TA Head, IS<F, TB> Tail)
            : IS2<CofreeF<F>, TA, TB>
        {
            public TR Evaluate<TR>(ISemantic2<CofreeF<F>, TA, TB, TR> semantic)
                => semantic.Prj().From(Head, Tail);

            public IS2<CofreeF<F>, TRA, TRB> Select<TRA, TRB>(Func<TA, TRA> f, Func<TB, TRB> g)
                => new Data<TRA, TRB>(f(Head), Tail.Select(g));

            public override string ToString()
                => $"({Head}, {Tail})";
        }
    }

    public static class B
    {
        public static IS2<CofreeF<F>, TA, TB> From<TA, TB>(TA head, IS<F, TB> tail)
            => new D.Data<TA, TB>(head, tail);
    }

    public interface ISemantic<TA, TB, out TR>
        : ISemantic2<CofreeF<F>, TA, TB, TR>
    {
        TR From(TA head, IS<F, TB> tail);
    }

    sealed class FuncSemantic<TA, TB, TR>(Func<IS2<CofreeF<F>, TA, TB>, TR> f)
        : ISemantic<TA, TB, TR>
    {
        public TR From(TA head, IS<F, TB> tail)
            => f(B.From(head, tail));
    }

    public static ISemantic2<CofreeF<F>, TA, TB, TR> Semantic<TA, TB, TR>(Func<IS2<CofreeF<F>, TA, TB>, TR> f)
        => new FuncSemantic<TA, TB, TR>(f);
}

public static partial class CofreeFExtensions
{
    public static CofreeF<F>.ISemantic<TA, TB, TR> Prj<F, TA, TB, TR>(
        this ISemantic2<CofreeF<F>, TA, TB, TR> semantic
    )
        where F : IFunctor<F>
        => (CofreeF<F>.ISemantic<TA, TB, TR>)semantic;

    public static CofreeF<F>.D.Data<TA, TB> Unwrap<F, TA, TB>(
        this IS2<CofreeF<F>, TA, TB> e
    )
        where F : IFunctor<F>
        => (CofreeF<F>.D.Data<TA, TB>)e;
    public static CofreeF<F>.D.Data<TA, TB> Inj<F, TA, TB>(
         this IS<K21<CofreeF<F>, TA>, TB> e
     )
         where F : IFunctor<F>
         => (CofreeF<F>.D.Data<TA, TB>)e;
}


public static partial class CofreeExtensions
{
    public static (TS Head, IS<F, IS<Cofree<F>, TS>> Tail) Unwrap<F, TS>(this IS<Cofree<F>, TS> e)
        where F : IFunctor<F>
        => Cofree<F>.IAlias<TS>.Unwrap(e);
}
